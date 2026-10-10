# Registration and login

## Start and use

Install the .NET 10 SDK, then run from the repository root:

```sh
ASPNETCORE_ENVIRONMENT=Development dotnet run --project SocialMediaApp
```

Open the local URL printed by the app. Development startup applies the checked-in Identity migration to `SocialMediaApp/Data/app.db`. Register with an email address and a password of 12–100 characters containing uppercase, lowercase, a digit, and a symbol. Confirm the password. Successful registration signs you in and opens **My account**. Use the navigation to log out, then log in with the same email and password. **Remember me** persists the login cookie across browser sessions; leave it unchecked on shared devices. Five failed password attempts lock the account for 15 minutes.

This feature uses ASP.NET Core Identity password hashing and protected authentication cookies. Account forms render on the server and include antiforgery protection. Passwords are never stored as plain text. Email addresses are unique; login failures use a generic message. Return URLs must remain within the app.

Email ownership is not verified in this feature. The existing template email sender is a no-op. Password recovery and verification need a real email provider before the team advertises them; they are not linked from the login form. Registration deliberately signs in immediately. A future verification feature must update registration to send confirmation and enable `RequireConfirmedAccount` together.

## Integration for teammates

- Inject `SocialMediaApp.Services.ICurrentUser` in a Blazor component and await `GetAsync()`. It returns `CurrentUserInfo` (`Id`, `Email`) or `null` for a guest. The ID is the existing Identity primary key; use it for profile/post ownership relationships rather than email.
- Add `@attribute [Authorize]` to protected pages. `/my-posts` is an example. The home page (`/`) uses `<AuthorizeView>` instead: guests see a welcome message and signed-in users see the feed. Login and registration send users to the home feed. `/auth` remains compatible with the original starter project.
- Use `AuthorizeView` for conditional UI. Enforce authorization and ownership again in server services/endpoints; hiding a button does not secure data. For endpoints, use `.RequireAuthorization()` and read the authenticated principal's `ClaimTypes.NameIdentifier`. Do not trust user IDs submitted by clients.
- Existing routes: `/Account/Register`, `/Account/Login`, `/Account/Manage`; logout is an antiforgery-protected POST to `/Account/Logout`. Login and registration accept a local `ReturnUrl` query parameter.
- Authentication and its database are already registered in `Program.cs`. No profile, posts, feed, or other team feature is implemented here.

## Verification

With an isolated development database and the app running:

```sh
python3 tests/authentication_smoke.py http://localhost:5187
```

The test creates random test accounts and checks validation, duplicate accounts, password hashing, protected access, cookies, invalid credentials, lockout, remember-me, logout, antiforgery rejection, and external return URLs. Use a disposable database: accounts are retained after the test. No third-party Python packages are required.

## Deployment handoff

Use HTTPS in production (authentication cookies are Secure outside Development). Apply the existing EF migrations during deployment, persist the SQLite file on a writable volume, and persist ASP.NET Core Data Protection keys so cookies survive restarts. Production does not automatically migrate the database. Choose a shared database/key store before scaling to multiple app instances. Configure the real email sender as a separate integration task. Cloud deployment, Trello management, and full-project accessibility audits belong to the group project.
