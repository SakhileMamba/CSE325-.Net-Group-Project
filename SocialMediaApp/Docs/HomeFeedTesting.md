# Home Feed

The home page (`/`) is a Twitter-style feed of every user's posts, newest first.
Signed-in users can write a post at the top of the feed. Guests see a welcome
message with **Get started** (register) and **Log in** buttons instead.

## How it works

- `Home.razor` uses `<AuthorizeView>`: signed-in users get the `HomeFeed`
  component (interactive), guests get the welcome message (a plain static page).
- `HomeFeed.razor` loads 20 posts at a time. **Show more posts** loads the next 20
  and disappears when there are no older posts.
- The composer reuses the existing post creation: the same validation as
  **My Posts** and `PostService.CreateAsync`. After posting, the feed reloads
  so the new post is at the top.
- `PostService.GetFeedAsync` is the only new service method. It loads posts
  with their authors and pages by post Id (the next page is posts with a lower
  Id than the oldest one shown).
- `PostCard.razor` shows one post: avatar (profile picture or first letter),
  name, short time ("now", "5m", "3h", then the date), content, and the post's
  replies. The name links to `/profile/{username}`; the time links to the post's
  discussion page.
- The name is the user's first and last name from their profile. If they have
  not set one, their username (currently their email) is shown.
- Posts with comments show **Show replies (N)**, where N counts all comments and
  replies. Opening it shows the top-level comments; each comment with replies
  has its own **Show replies (N)** / **Hide replies** button, at every level.
  On the home feed and My Posts replies start folded; on the discussion page
  they start open.
- Replying happens in place. The post's **Reply** opens a reply box under the
  post; a comment's **Reply** opens one directly under that comment. After
  posting, the thread opens so the new reply shows.
- `PostReplies.razor` holds the Reply button, the foldable thread, and the reply
  boxes (`ReplyBox.razor`). It loads its own post's comments with the existing
  `CommentService.GetCommentsAsync` and posts with `CommentService.AddAsync`.
  The home feed and **My Posts** both use it.
- Styles are in `Home.razor.css` (welcome), `HomeFeed.razor.css`, and
  `PostCard.razor.css` (Blazor CSS isolation).

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
the next page starts after the oldest post shown, and guests cannot read the
feed.

## Browser acceptance checks

- [ ] Log out and visit `/`; verify the welcome message. **Get started** opens
      registration and **Log in** opens login; both return you to the feed.
- [ ] As user A, post from the home page; verify it appears at the top as "now".
- [ ] Submit an empty or whitespace-only post; verify "Please enter post content."
- [ ] Verify typing or pasting cannot exceed 250 characters.
- [ ] As user B, verify A's post appears in B's feed with A's name.
- [ ] Set a first and last name on a profile; verify the feed shows that name.
- [ ] With more than 20 posts, click **Show more posts**; verify older posts load
      with no duplicates and the button disappears at the end.
- [ ] Click a post's time; verify the discussion page opens.
- [ ] Click an author's name; verify their profile opens.
- [ ] On a post with no comments, verify there is no **Show replies** button.
- [ ] On a post with a comment, a reply, and a reply to that reply, open
      **Show replies**; verify only top-level comments show, then open and
      close each level of replies.
- [ ] Click **Hide replies** on the post; verify the whole thread folds.
- [ ] Click a post's **Reply**; verify a reply box opens under the post with the
      cursor in it. Submit an empty reply and verify "Please enter reply content."
- [ ] Post a reply; verify the page does not change and the reply appears in the
      opened thread.
- [ ] Click a comment's **Reply**; verify the box opens under that comment, then
      post and verify the reply appears under it.
- [ ] Click **Cancel**; verify the box closes and nothing is posted.
- [ ] Repeat the reply checks on **My Posts**.
- [ ] On the discussion page, verify replies start open and can be folded.
- [ ] Upload a profile picture while running `dotnet watch`; verify the app keeps
      running and the picture appears on your posts in the feed.

## Not included yet

Likes and filtering by interests.
