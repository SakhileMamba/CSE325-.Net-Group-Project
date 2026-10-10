# In-App Notifications

The authenticated `/notifications` page shows when another user likes one of the
current user's posts or comments. Each item identifies the user who liked the
content and links to the post discussion. Self-likes do not create notifications.
Unliking removes the matching notification, and **Mark all read** clears the
unread highlighting.

Notifications are stored in SQLite and are scoped to their recipient in
`NotificationService`. The current user ID comes from server authentication;
the browser does not provide the recipient ID.

## Automated checks

From the repository root, run:

```sh
dotnet build SocialMediaApp/SocialMediaApp.csproj
dotnet run --project PostManagementChecks
```

The checks cover post and comment notifications, recipient privacy, read status,
unlike removal, reliking, and rejection of unauthenticated reads and updates.

## Browser checks

1. Apply migrations with `dotnet ef database update --project SocialMediaApp`.
2. Sign in as user A and create a post and comment.
3. Sign in as user B and like A's post and comment.
4. Return as A and open **Notifications**. Verify both unread items name B and
   link to the correct discussion.
5. Select **Mark all read** and verify the unread highlighting disappears.
6. As B, unlike one item. Return as A and verify its notification is gone.
7. Like your own content and verify it does not create a notification.
8. Log out and verify `/notifications` redirects to login.

This feature provides in-app records only. It does not send browser, push, email,
or phone notifications.
