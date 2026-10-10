# Likes

Authenticated users can like and unlike posts and comments on the existing
`/posts/{postId}` discussion page. Each button displays the total count and
changes between **Like** and **Unlike** for the current user.

Likes are stored in two simple join tables: `PostLikes` and `CommentLikes`.
Their composite primary keys (`content ID`, `user ID`) guarantee that each user
can like a specific post or comment only once. `LikeService` always gets the
user ID from server authentication. It never accepts a user ID from the UI.

Deleting a user removes that user's likes. Deleting a post or comment removes
the likes attached to it. The existing post and comment content is unchanged.

## Automated checks

From the repository root:

```sh
dotnet run --project PostManagementChecks
```

The checks cover liking, unliking, counts from multiple users, per-user state,
missing content, unauthenticated operations, separation between posts, and
cascade deletion. They run against a disposable SQLite database.

## Browser checks

1. From `SocialMediaApp`, run `dotnet ef database update`, then `dotnet run`.
2. Log in and open **My Posts**, then **Comments & replies** on a post.
3. Like the post. Confirm the count increases and the button says **Unlike**.
4. Refresh the page. Confirm the count and liked state remain.
5. Unlike the post. Confirm the count decreases and the button says **Like**.
6. Repeat those actions on a root comment and a nested reply.
7. Log in as a second user and like the same content. Confirm the total includes
   both users while each account keeps its own Like/Unlike state.
8. Delete a liked post and confirm its discussion is unavailable.

The feature does not add a feed, notification, or separate liked-content page.
