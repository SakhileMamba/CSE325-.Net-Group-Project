# View Liked Content

The authenticated `/liked` page lists posts and comments liked by the current
user. Each item shows its author, content, creation time, total like count, and
a link to the post discussion. Users can unlike an item directly from this page;
it then disappears from their list.

The page uses the existing `PostLikes` and `CommentLikes` tables, so no new
migration is required. Because those tables do not record when a like happened,
items are ordered newest-first by the post or comment creation time.

## Automated checks

From the repository root:

```sh
dotnet run --project PostManagementChecks
```

The checks verify that liked posts and comments are returned only for the
authenticated user, include their related post and author information, show the
current total count, and reject unauthenticated requests. Existing tests also
cover like/unlike behavior and deletion cleanup.

## Browser checks

1. From `SocialMediaApp`, run `dotnet run` and log in.
2. Like a post, a root comment, and a nested reply.
3. Open **Liked Content** from the navigation.
4. Confirm all three items appear in the correct Posts or Comments section.
5. Follow **Open discussion** and confirm it opens the associated post.
6. Return to **Liked Content**, unlike one item, and confirm it disappears.
7. Refresh and confirm the remaining liked content persists.
8. Log in as a different user and confirm the first user's items do not appear.
9. Log out and visit `/liked`; confirm redirection to the login page.
