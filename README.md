# Social Net

Social Net is a CSE 325 group project built with .NET 10 Blazor, ASP.NET Core
Identity, Entity Framework Core, SQLite, and Bootstrap.

## Features

- Register, sign in, sign out, and manage an account.
- Create, read, edit, and delete posts of up to 250 characters.
- Browse a home feed and filter it by profile interests.
- Comment on posts and reply in threaded discussions.
- Like and unlike posts and comments.
- Review previously liked posts and comments on the **Liked Content** page.
- See in-app notifications when another user likes your content.
- Upload, replace, or remove a profile picture.

Post updates and deletions are restricted to the post owner. Likes are stored per
user, and authenticated routes use ASP.NET Core Identity to determine the current
user.

## Run the application

Install the .NET 10 SDK and EF Core command-line tools, then run these commands
from the repository root:

```bash
dotnet restore SocialMediaApp/SocialMediaApp.csproj
dotnet ef database update --project SocialMediaApp
dotnet run --project SocialMediaApp
```

Open the local URL printed in the terminal. A newly registered user is signed in
immediately. Email confirmation is currently disabled because the project does
not have a production email provider.

The SQLite database and uploaded profile images are local development data and
are excluded from Git.

## Main routes

| Route | Purpose |
| --- | --- |
| `/` | Home feed |
| `/my-posts` | Create and manage your posts |
| `/posts/{id}` | Read a post discussion and add comments or replies |
| `/liked` | View posts and comments you liked |
| `/notifications` | View likes on your posts and comments |
| `/profile` | View and update your profile |

## Verification

Build the application and run the repository's automated feature checks:

```bash
dotnet build SocialMediaApp/SocialMediaApp.csproj
dotnet run --project PostManagementChecks
```

Authentication also has a browser-based smoke-test script. It requires the app
to be running and changes the local development database, so follow
[`docs/authentication-smoke-test.md`](docs/authentication-smoke-test.md) before
using it.

Feature-specific manual test guides are in [`SocialMediaApp/Docs`](SocialMediaApp/Docs).
These guides complement the automated checks; they are not a claim that every
browser, responsive layout, or assistive technology has been manually tested.

## Project documentation

- [`docs/architecture.md`](docs/architecture.md) explains the application structure,
  data model, authorization, validation, and error-handling approach.
- [`docs/accessibility.md`](docs/accessibility.md) describes the accessibility
  support and remaining manual checks.
- [`docs/authentication.md`](docs/authentication.md) covers account setup, security,
  and teammate integration points.
- [`docs/azure-deployment.md`](docs/azure-deployment.md) explains the prepared
  single-instance Azure App Service deployment.

## Team workflow

Create feature branches from the latest `main`, keep changes limited to the
assigned project-board task, and open a pull request for review. When a feature
changes the EF Core model, add a new migration rather than editing or replacing
another feature's migration. After merging changes that contain migrations, run
`dotnet ef database update --project SocialMediaApp` locally.
