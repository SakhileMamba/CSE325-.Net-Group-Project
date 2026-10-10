# Social Net Architecture

## Application structure

Social Net is a .NET 10 Blazor Web App using Interactive Server rendering.
Bootstrap supplies the base visual styles, ASP.NET Core Identity handles accounts,
Entity Framework Core maps application data, and SQLite stores development data.

- `SocialMediaApp/Components` contains reusable UI components and routable pages.
- `SocialMediaApp/Services` contains the application operations used by components.
- `SocialMediaApp/Data` contains Identity, entity models, the EF Core context, and
  migrations.
- `SocialMediaApp/wwwroot` contains CSS, JavaScript, and static assets.
- `PostManagementChecks` is a console-based integration-check project that uses a
  temporary SQLite database.

The components call small feature services instead of placing database operations
directly in the UI. Services use `IDbContextFactory<ApplicationDbContext>` so each
operation has its own short-lived context, which is appropriate for Blazor's
long-lived interactive circuits.

## Authentication and ownership

ASP.NET Core Identity supplies `ApplicationUser` and the authenticated
`ClaimsPrincipal`. Authenticated pages obtain the current user's Identity ID and
pass it to the relevant service. The services verify that the ID is present and
enforce ownership when a post is updated or deleted. UI controls are hidden when
the current user is not allowed to perform an action, while the service check
provides the server-side protection.

## Data model

- `ApplicationUser` owns posts, comments, likes, and received or created
  notifications.
- `Post` stores its owner, text content, and creation time. Interest filtering
  matches saved interests against the post text.
- `Comment` belongs to a post and author. Its optional parent-comment relationship
  forms a reply tree.
- `PostLike` and `CommentLike` join a user to the item they liked. Unique database
  indexes prevent duplicate likes by the same user.
- `UserNotification` records the recipient, the user who performed the like,
  the affected post or comment, its read state, and its creation time.

Deleting a post cascades to its discussion, likes, and notifications. Unliking
removes the matching notification. Account deletion preserves content required by
the existing discussion behavior where configured, so database relationship
changes should be tested carefully.

## Validation and errors

Models and input models use data annotations for required fields and length
limits. Services repeat important rules and authorization checks so requests
cannot bypass them by changing browser markup. Database constraints and foreign
keys provide a final data-integrity layer.

Pages display validation summaries and actionable failure messages. Unexpected
exceptions are logged through `ILogger`; detailed exception information remains
in server logs rather than being exposed in the browser.

## Verification

Run `dotnet build SocialMediaApp/SocialMediaApp.csproj` for compilation and
`dotnet run --project PostManagementChecks` for the automated SQLite integration
checks. Manual browser guides in `SocialMediaApp/Docs` cover visual behavior and
multi-user workflows that the console checks cannot fully verify.
