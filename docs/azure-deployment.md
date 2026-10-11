# Azure App Service Deployment

Social Net is prepared for one Linux Azure App Service instance using its
existing SQLite database. Azure persistent storage keeps the database, uploaded
profile pictures, and ASP.NET Core Data Protection keys across deployments.

## 1. Create the web app

In the Azure portal, create an **App Service Web App** with these choices:

- Publish: **Code**
- Runtime stack: **.NET 10**
- Operating system: **Linux**
- Instance count: **1**
- App Service plan: **Basic B1** is suitable for a course demonstration and
  supports Always On. Monitor the Azure for Students credit balance.

Use a region near the expected users. Do not scale beyond one instance while the
application uses SQLite.

## 2. Configure application settings

Open **Settings > Environment variables** for the web app and add:

| Name | Value |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `WEBSITES_ENABLE_APP_SERVICE_STORAGE` | `true` |
| `Storage__RootPath` | `/home/data` |
| `ConnectionStrings__DefaultConnection` | `Data Source=/home/data/socialnet.db;Cache=Shared` |

The double underscores represent nested .NET configuration keys. Keep the
connection string in Azure settings rather than committing it to Git.

At startup, the application creates `/home/data/uploads` and `/home/data/keys`
and applies checked-in EF Core migrations. The `/health` endpoint returns HTTP
200 after startup succeeds.

## 3. Configure Blazor hosting

In the App Service configuration:

- Enable **WebSockets**.
- Keep **Session affinity (ARR affinity)** enabled.
- Enable **Always On** when the selected plan supports it.
- Set the health-check path to `/health` if health checks are available for the
  selected plan.

Blazor Interactive Server depends on a persistent SignalR connection. A sleeping
free instance can interrupt active sessions, which is why Basic B1 is recommended
for a class presentation.

## 4. Connect GitHub Actions

The repository contains `.github/workflows/azure-deploy.yml`. In GitHub, open
**Settings > Secrets and variables > Actions** and configure:

1. Repository variable `AZURE_WEBAPP_NAME` with the exact App Service name.
2. Repository secret `AZURE_WEBAPP_PUBLISH_PROFILE` containing the publish
   profile downloaded from the App Service **Overview** page.

If Azure disables publish-profile/basic authentication, enable SCM basic
authentication under the App Service configuration before downloading the
profile. Keep the profile secret; it grants deployment access.

Every push to `main` then builds and deploys the application. The workflow can
also be started manually from GitHub's **Actions** page.

## 5. Verify the deployment

After the workflow succeeds:

1. Open `https://<app-name>.azurewebsites.net/health` and confirm the healthy
   response.
2. Register an account and create a post.
3. Upload a profile picture.
4. Restart the App Service.
5. Confirm that the account, post, picture, and login still work.
6. Review the App Service log stream for migration or SQLite errors.

SQLite and local file uploads are appropriate for this single-instance course
project. A future multi-instance deployment should move relational data to a
managed database and profile pictures to object storage.
