# Home Feed

The home page (`/`) is a Twitter-style feed of every user's posts, newest first.
Signed-in users can write a post at the top of the feed. Guests are redirected
to login, like the other post pages.

## How it works

- `Home.razor` loads 20 posts at a time. **Show more posts** loads the next 20
  and disappears when there are no older posts.
- The composer reuses the existing post creation: the same validation as
  **My Posts** and `PostService.CreateAsync`. After posting, the feed reloads
  so the new post is at the top.
- `PostService.GetFeedAsync` is the only new service method. It loads posts
  with their authors and pages by post Id (the next page is posts with a lower
  Id than the oldest one shown).
- `PostCard.razor` shows one post: avatar (profile picture or first letter),
  name, short time ("now", "5m", "3h", then the date), content, and a **Reply**
  link. The name links to `/profile/{username}`; the time and **Reply** link to
  the post's discussion page.
- The name is the user's first and last name from their profile. If they have
  not set one, their username (currently their email) is shown.
- Posts with comments show **Show replies (N)**, where N counts all comments and
  replies. Opening it shows the top-level comments; each comment with replies
  has its own **Show replies (N)** / **Hide replies** button, at every level.
  On the home feed replies start folded; on the discussion page they start open.
- The comments for each page of posts are loaded in one query
  (`CommentService.GetCommentsForPostsAsync`). A comment's **Reply** button on
  the home feed opens the post's discussion page, where replying already works.
- Styles are in `Home.razor.css` and `PostCard.razor.css` (Blazor CSS isolation).

## Profile picture uploads moved

Uploaded pictures are now saved in `SocialMediaApp/uploads/profiles/` instead of
`wwwroot/uploads/profiles/`, and are still served at `/uploads/profiles/...`.
Any new file in `wwwroot` made `dotnet watch` crash ("Unexpected true -
HotReloadMSBuildWorkspace.cs"), which stopped the app after every upload.
The `uploads` folder is ignored by Git.

If you already uploaded a picture, move it so it keeps showing:

```sh
mkdir -p SocialMediaApp/uploads/profiles
mv SocialMediaApp/wwwroot/uploads/profiles/* SocialMediaApp/uploads/profiles/
```

## Automated checks

From the repository root:

```sh
dotnet run --project PostManagementChecks
```

New feed checks: every user's posts appear newest first with their author,
the next page starts after the oldest post shown, comments for several posts
load together, and guests cannot read the feed or its comments.

## Browser acceptance checks

- [ ] Log out and visit `/`; verify redirection to login.
- [ ] As user A, post from the home page; verify it appears at the top as "now".
- [ ] Submit an empty or whitespace-only post; verify "Please enter post content."
- [ ] Verify typing or pasting cannot exceed 250 characters.
- [ ] As user B, verify A's post appears in B's feed with A's name.
- [ ] Set a first and last name on a profile; verify the feed shows that name.
- [ ] With more than 20 posts, click **Show more posts**; verify older posts load
      with no duplicates and the button disappears at the end.
- [ ] Click a post's time or **Reply**; verify the discussion page opens.
- [ ] Click an author's name; verify their profile opens.
- [ ] On a post with no comments, verify there is no **Show replies** button.
- [ ] On a post with a comment, a reply, and a reply to that reply, open
      **Show replies**; verify only top-level comments show, then open and
      close each level of replies.
- [ ] Click **Hide replies** on the post; verify the whole thread folds.
- [ ] Click a comment's **Reply** button; verify the discussion page opens.
- [ ] On the discussion page, verify replies start open and can be folded.
- [ ] Upload a profile picture while running `dotnet watch`; verify the app keeps
      running and the picture appears on your posts in the feed.

## Not included yet

Likes and filtering by interests.
