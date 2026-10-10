"""HTTP integration checks against a running app with a disposable database."""
import html
import http.cookiejar
import re
import sqlite3
import sys
import uuid
from pathlib import Path
from urllib.error import HTTPError
from urllib.parse import urlencode, urlsplit
from urllib.request import HTTPCookieProcessor, HTTPRedirectHandler, Request, build_opener

BASE = (sys.argv[1] if len(sys.argv) > 1 else 'http://localhost:5187').rstrip('/')
PASSWORD = 'Test-Password42!'
EMAIL = f'auth-{uuid.uuid4().hex}@example.com'


class NoRedirect(HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        return None


class Browser:
    def __init__(self):
        self.cookies = http.cookiejar.CookieJar()
        self.client = build_opener(HTTPCookieProcessor(self.cookies), NoRedirect())

    def request(self, path, data=None):
        request = Request(BASE + path, data=None if data is None else urlencode(data).encode())
        try:
            response = self.client.open(request, timeout=15)
        except HTTPError as error:
            response = error
        return response.code, response.headers, response.read().decode()

    def form(self, path, handler, values):
        status, _, body = self.request(path)
        assert status == 200, (path, status)
        token = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', body)
        assert token, 'Missing antiforgery token'
        return self.request(path, {'__RequestVerificationToken': html.unescape(token[1]),
                                   '_handler': handler, **values})


def register(browser, email, password=PASSWORD, confirmation=None, path='/Account/Register'):
    return browser.form(path, 'register', {'Input.Email': email, 'Input.Password': password,
                       'Input.ConfirmPassword': password if confirmation is None else confirmation})


def login(browser, password=PASSWORD, remember=False, path='/Account/Login'):
    return browser.form(path, 'login', {'Input.Email': EMAIL, 'Input.Password': password,
                                     'Input.RememberMe': str(remember).lower()})


def app_cookie(browser):
    return next((cookie for cookie in browser.cookies if cookie.name == '.AspNetCore.Identity.Application'), None)


browser = Browser()
status, headers, _ = browser.request('/my-posts')
assert status == 302 and '/Account/Login' in headers['Location'], 'Guest access must be protected'
status, _, body = browser.request('/')
assert status == 200 and 'Get started' in body, 'Guests see the welcome page on the home page'
assert register(browser, EMAIL, 'short')[0] == 200 and app_cookie(browser) is None
assert register(browser, EMAIL, confirmation='Different-Password42!')[0] == 200
status, headers, _ = register(browser, EMAIL)
assert status == 302 and urlsplit(headers['Location']).path == '/'
assert app_cookie(browser) is not None and app_cookie(browser).discard
assert any(key.lower() == 'httponly' for key in app_cookie(browser)._rest)
assert EMAIL in browser.request('/')[2]
status, _, _ = browser.request('/Account/Logout', {'ReturnUrl': ''})
assert status == 400, 'Logout must reject missing antiforgery token'
_, _, body = browser.request('/')
token = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', body)[1]
assert browser.request('/Account/Logout', {'ReturnUrl': '', '__RequestVerificationToken': html.unescape(token)})[0] == 302
assert browser.request('/my-posts')[0] == 302
assert register(Browser(), EMAIL.upper())[0] == 200, 'Duplicate email should be rejected'
assert login(browser, 'Incorrect-Password42!')[0] == 200 and app_cookie(browser) is None
assert login(browser, remember=True)[0] == 302
assert app_cookie(browser).expires is not None
for return_url in ('https://example.com/', '//example.com/', '/\\example.com/'):
    other = Browser()
    path = '/Account/Login?' + urlencode({'ReturnUrl': return_url})
    status, headers, _ = login(other, path=path)
    assert status == 302 and urlsplit(headers['Location']).path == '/', headers['Location']
local = Browser()
status, headers, _ = login(local, path='/Account/Login?' + urlencode({'ReturnUrl': '/auth'}))
assert status == 302 and urlsplit(headers['Location']).path == '/auth'
external_registration = Browser()
status, headers, _ = register(external_registration, f'auth-{uuid.uuid4().hex}@example.com',
                             path='/Account/Register?' + urlencode({'ReturnUrl': 'https://example.com/'}))
assert status == 302 and urlsplit(headers['Location']).path == '/'
assert Browser().request('/Account/Register', {'Input.Email': EMAIL, 'Input.Password': PASSWORD})[0] == 400
lockout = Browser()
for attempt in range(5):
    status, headers, _ = login(lockout, 'Incorrect-Password42!')
assert status == 302 and '/Account/Lockout' in headers['Location']
assert login(lockout)[0] == 302 and app_cookie(lockout) is None
# Optional local DB assertion: the default database used by the documented launch command.
database = Path(__file__).resolve().parents[1] / 'SocialMediaApp/Data/app.db'
if database.exists():
    with sqlite3.connect(database) as connection:
        row = connection.execute('SELECT PasswordHash FROM AspNetUsers WHERE Email = ?', (EMAIL,)).fetchone()
        assert row, "Test user missing from the default local database"
        assert row[0] and row[0] != PASSWORD and PASSWORD not in row[0]
print('PASS: registration, validation, duplicate email, cookies, authorization, logout, CSRF, redirects, login, and lockout')
