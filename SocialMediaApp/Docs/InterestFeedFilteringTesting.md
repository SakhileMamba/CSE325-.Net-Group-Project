# Interest-based feed filtering

## Use

1. In Home, choose **Update interests** to open the existing profile form.
2. Enter one or more interests separated by commas, such as `Music, Hiking`, and save.
3. Return to Home and choose **My interests** in **Show posts**.
4. Choose **All posts** to return to the original feed. All posts is the default on each visit.

Selected interests use the existing `ApplicationUser.Interests` column and profile save flow. Entries are trimmed, empty entries ignored, and duplicates removed when reading the filter. Updates take effect when returning to Home. Clearing interests produces a prompt to select interests in filtered mode; no matching posts produces a friendly empty-results message.

Posts have no category or tag fields. This implementation matches any comma-separated interest as a literal substring of post content. SQLite's built-in lowercase operation supports case-insensitive ASCII matching; non-ASCII case folding is limited by SQLite. Interests containing `%` or `_` are literal text, not SQL wildcard patterns. There is no curated interest catalog, semantic matching, or new post metadata.

Filtering occurs in the existing database query before ordering, page size, and the existing ID cursor. No schema, model, migration, dependency, configuration, or post-card changes are needed. `GetFeedAsync` adds an optional `filterByInterests` argument defaulting to false, so existing callers retain the all-posts behavior. `GetInterestsAsync` only reads the authenticated user's interests. The feed retains its composer, author links, discussion links, and replies.

## Automated checks

```sh
dotnet build SocialMediaApp --no-restore
dotnet run --project PostManagementChecks --no-restore
```

On a fresh checkout, restore packages first if needed. The check runner uses a temporary migrated SQLite database and deletes it afterward.

Verified: build passes with no warnings/errors; all 44 existing post/comment checks and all 15 new interest checks pass. New checks cover multiple/single interests, persistence and updates, trimming/deduplication, matching either interest across authors, ASCII case matching, filtering before pagination with 25 newer unrelated posts, unchanged all-posts behavior, empty/no-match results, literal SQL special characters, account isolation, and authentication.

## Browser verification

Tested with a separate temporary database:

- Save `Music, Hiking` through the existing profile form and confirm the saved values on Home.
- Create a music post and an unrelated post; My interests shows only the music post.
- Post a reply from the filtered feed; switch to All posts and verify both posts and the saved reply.
- Change the saved interest to `Astronomy`; return to Home and verify the new value and the no-results message.

Existing profile validation can prevent saving a newly registered account until its email-style username is changed to a valid username, and can reject a blank phone field. Valid test values were used; this feature does not change that unrelated validation. Likes are not implemented in the current project and could not be tested. Existing author/discussion links and PostCard remain unchanged; existing post/comment regression checks pass.

## Collaboration boundaries

Shared files touched: `PostService.cs` (optional filter and saved-interest reader), `HomeFeed.razor` (filter control and paging arguments), and account `Manage/Index.razor` (interest placeholder/help only). Coordinate overlapping edits in those files. The check runner adds one call to a separate feature check file. No profile save logic, authentication logic, post CRUD, comment logic, or other developer branches were changed. No commits or pushes were created.
