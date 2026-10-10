# Home Feed

Guests visiting `/` see a welcome message with links to register and log in.
Signed-in users see every user's posts, newest first, with a box to write a post.

- `HomeFeed.razor` loads 20 posts at a time. **Show more posts** loads the next 20.
- `PostCard.razor` shows one post. The author's name links to their profile and
  the time links to the post's discussion page.
- `PostReplies.razor` shows a post's replies, folded by default. **Reply** opens
  a reply box (`ReplyBox.razor`) under the post or comment. My Posts uses it too.
- `PostService.GetFeedAsync` loads the feed. Replies use the existing
  `CommentService` methods.

## Profile pictures

Uploaded pictures are saved in `SocialMediaApp/uploads/profiles/` (ignored by Git)
and served at `/uploads/profiles/...`. They used to be saved in `wwwroot`, where
each new file made `dotnet watch` crash. If you uploaded a picture before this
change, move it:

```sh
mkdir -p SocialMediaApp/uploads/profiles
mv SocialMediaApp/wwwroot/uploads/profiles/* SocialMediaApp/uploads/profiles/
```

## Checks

Run `dotnet run --project PostManagementChecks` from the repository root. It
includes checks for feed order, paging, and guest access.

In the browser:

- [ ] Logged out, `/` shows the welcome message. **Get started** and **Log in**
      work and return you to the feed.
- [ ] A new post appears at the top. Empty posts are rejected and posts stop at
      250 characters.
- [ ] **Show more posts** loads older posts with no duplicates.
- [ ] Replies open and fold at each level.
- [ ] **Reply** on a post or comment opens a box in place, the reply appears
      after posting, and **Cancel** closes the box.
- [ ] Replying works the same way on My Posts.
- [ ] Uploading a profile picture while running `dotnet watch` does not stop the app.
