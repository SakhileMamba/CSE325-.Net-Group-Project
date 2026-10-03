# Post Management

The `/my-posts` page manages only the signed-in user's posts. It uses existing
Identity authentication, Bootstrap, and the application's SQLite database.

## Run locally

From `SocialMediaApp`, run `dotnet ef database update` and `dotnet run`.
Register an account and use the confirmation link displayed on the registration
confirmation page before logging in. The application currently has no real email
sender. Open **My Posts** from the navigation.

## Automated checks

From the repository root, run:

```sh
dotnet run --project PostManagementChecks
```

This small console check project needs no additional packages. It applies the
actual migrations to a temporary SQLite database, creates two test users, and
checks create/read/update/delete, the 250-character boundary, invalid content,
ownership rejection, unauthenticated operations, and database constraints.
It deletes the temporary database afterward and does not change `Data/app.db`.
The authentication provider is simulated for these checks; actual cookie login
and the browser UI require the manual checks below.

Verified during implementation:

- Application build succeeded with zero warnings and zero errors.
- All 23 automated SQLite checks passed.
- The initial Identity migration and `20261001204118_AddPosts` were applied to
  the new local `Data/app.db`; the Posts schema and migration history were inspected.
- The application started successfully. An unauthenticated request to `/my-posts`
  returned HTTP 302 to `/Account/Login?ReturnUrl=%2Fmy-posts`.
- The original Identity migration files were not modified.
- The local database is ignored by the existing Git ignore rules.

## Browser acceptance checks

- [ ] Log out and visit `/my-posts`; verify redirection to login.
- [ ] Register and confirm two accounts, A and B.
- [ ] As A, create a post and verify its content and UTC creation time.
- [ ] Create a 250-character post. Verify typing/pasting cannot exceed 250 characters.
- [ ] Submit empty or whitespace-only content; verify it is rejected.
- [ ] Edit A's post and verify the new content remains after refreshing.
- [ ] Cancel an edit and verify no change was saved.
- [ ] Start a deletion, cancel it, and verify the post remains.
- [ ] Confirm a deletion and verify the post disappears after refreshing.
- [ ] As B, verify A's posts do not appear; create B's own post.
- [ ] Return to A and verify only A's remaining posts appear.
- [ ] Restart the application and verify remaining posts persist.

## Decisions to share with teammates

- `Post` contains `Id`, `Content`, `UserId`, `User`, and `CreatedAtUtc`.
- The foreign key uses the existing Identity user's string ID.
- Content is validated before trimming surrounding whitespace; the submitted
  content must be nonblank and at most 250 .NET string characters.
- The database also checks nonempty trimmed content and a maximum length of 250.
- Deleting an account cascades to its posts, matching the existing account
  deletion feature. Future comments and likes need their own deletion policy.
- Post creation times are stored in UTC and are not changed when editing.
- Every service operation gets the user ID from server authentication. Edit and
  delete queries match both the post ID and owner ID; callers cannot supply owners.
- The context factory also registers `ApplicationDbContext` for Identity. Each
  post operation disposes its own context to avoid sharing one for a long-lived
  Interactive Server connection.
- The migration adds only `Posts`; the original Identity migration is preserved.
- Feed, comments, likes, notifications, profiles, and interest filtering are
  outside this feature.
