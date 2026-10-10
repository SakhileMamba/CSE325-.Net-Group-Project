# Comments & Threads

Signed-in users open `/posts/{postId}` to read a post's discussion, add a comment,
or reply to any comment (including a reply). The home feed and My Posts also show
each post's thread inline, with **Reply** buttons that open a reply box in place;
a post's time on the home feed links to its discussion page.

`Comment` stores its post, author, content, UTC creation time, and optional parent
comment ID. A null parent means a root comment. The service reads the author ID
from authentication and checks that reply parents belong to the same post.
The reusable `ThreadComment` component recursively displays replies beneath their
parents, oldest first. Indentation is capped at six levels for readability, but
replies can continue beyond that depth.

Comment content is required; no maximum was specified for comments, so the
250-character post limit is not applied to them. Comment editing and deletion
are outside this task. Deleting a post removes its entire discussion. Deleting
an account removes its comments, while other users' replies to those comments
become root comments on surviving posts.

## Automated checks

From the repository root:

```sh
dotnet run --project PostManagementChecks
```

The existing checks now include comments and nested replies, cross-user
participation, blank content, invalid parents, cross-post replies, unauthenticated
requests, post deletion, and account deletion. They use a disposable SQLite
database with actual migrations and a simulated authentication provider.

Verified during implementation: all 41 checks passed; the application build
had zero warnings and errors. Migration `20261003192844_AddComments` was applied
locally and the Comments table inspected. A logged-out request to `/posts/1`
returned HTTP 302 to login. The browser checks below remain manual.

## Browser checks

1. From `SocialMediaApp`, run `dotnet ef database update`, then `dotnet run`.
2. Log in as A, create a post, and open its discussion page (click the post's time on the home feed).
3. Add two comments. Refresh and verify both remain with author and UTC time.
4. Click **Reply** on a comment, submit a reply, then reply to that reply.
   Verify the nested order and indentation.
5. Click **Reply**, type text, then **Cancel reply**. Verify nothing was posted.
6. Submit empty and whitespace-only comments/replies; verify rejection.
7. Copy the discussion URL. Log in as B and open that URL. Add a comment and a
   reply to A's comment. Verify both authors' contributions appear.
8. Open a different post's discussion and verify its comments are separate.
9. Log out and open the discussion URL; verify login redirection.
10. Open `/posts/2147483647`; verify a missing-post message rather than a crash.
11. As the post owner, delete the post from My Posts; its discussion becomes unavailable.

No feed, likes, notifications, profiles, or interest filtering are added.
