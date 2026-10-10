using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Data;
using SocialMediaApp.Services;

// Uses the existing check runner's temporary migrated SQLite database.
internal static class InterestFeedChecks
{
    public static async Task RunAsync(IDbContextFactory<ApplicationDbContext> factory)
    {
        var auth = new CheckAuthenticationStateProvider();
        var service = new PostService(factory, auth);
        void Check(bool condition, string name)
        {
            if (!condition) throw new Exception($"FAILED: {name}");
            Console.WriteLine($"PASS: interest feed: {name}");
        }

        int musicId, hikingId;
        await using (var db = await factory.CreateDbContextAsync())
        {
            db.Users.AddRange(
                new ApplicationUser { Id = "interest-reader", UserName = "reader", Interests = " Music, Hiking, music, , " },
                new ApplicationUser { Id = "interest-author", UserName = "author", Interests = "Science" });
            await db.SaveChangesAsync();
            var music = new Post { UserId = "interest-author", Content = "MUSIC festival", CreatedAtUtc = DateTime.UtcNow };
            var hiking = new Post { UserId = "interest-reader", Content = "Weekend hiking", CreatedAtUtc = DateTime.UtcNow };
            db.Posts.AddRange(music, hiking);
            await db.SaveChangesAsync();
            musicId = music.Id;
            hikingId = hiking.Id;
            for (var i = 0; i < 25; i++)
                db.Posts.Add(new Post { UserId = "interest-author", Content = "Unrelated news", CreatedAtUtc = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        auth.SignIn("interest-reader");
        Check((await service.GetInterestsAsync()).SequenceEqual(new[] { "Music", "Hiking" }),
            "Reads saved multiple interests, trimming blanks and deduplicating");
        var matches = await service.GetFeedAsync(filterByInterests: true);
        Check(matches.Select(p => p.Id).SequenceEqual(new[] { hikingId, musicId }),
            "Matches either interest across authors and ignores ASCII case");
        Check(matches.All(p => p.User.UserName is not null), "Preserves author data for existing post cards");
        var firstPage = await service.GetFeedAsync(pageSize: 1, filterByInterests: true);
        var secondPage = await service.GetFeedAsync(firstPage.Single().Id, 1, filterByInterests: true);
        Check(firstPage.Single().Id == hikingId && secondPage.Single().Id == musicId &&
            (await service.GetFeedAsync(musicId, 1, filterByInterests: true)).Count == 0,
            "Filters before pagination despite 25 newer unrelated posts; no duplicates");
        Check((await service.GetFeedAsync()).Count == 20 &&
            (await service.GetFeedAsync()).All(p => p.Content == "Unrelated news"),
            "All posts keeps the existing default ordering and page size");

        async Task SetInterestsAsync(string? interests)
        {
            await using var db = await factory.CreateDbContextAsync();
            (await db.Users.SingleAsync(u => u.Id == "interest-reader")).Interests = interests;
            await db.SaveChangesAsync();
        }

        await SetInterestsAsync("Music");
        Check((await service.GetFeedAsync(filterByInterests: true)).Single().Id == musicId,
            "A saved single-interest update is read on the next request");
        await SetInterestsAsync("Astronomy");
        Check((await service.GetFeedAsync(filterByInterests: true)).Count == 0, "No matches returns an empty feed");
        foreach (var empty in new string?[] { null, "", " , , " })
        {
            await SetInterestsAsync(empty);
            Check((await service.GetInterestsAsync()).Length == 0 &&
                (await service.GetFeedAsync(filterByInterests: true)).Count == 0, "No selected interests returns an empty filtered feed");
        }
        await SetInterestsAsync("%, _");
        Check((await service.GetFeedAsync(filterByInterests: true)).Count == 0,
            "SQL wildcard characters are matched literally");
        await service.CreateAsync("A literal 100% match");
        Check((await service.GetFeedAsync(filterByInterests: true)).Single().Content == "A literal 100% match",
            "Literal special-character interests can match");
        auth.SignIn("interest-author");
        Check((await service.GetFeedAsync(filterByInterests: true)).Count == 0,
            "Uses the signed-in reader's interests, not another account's");
        auth.SignOut();
        try { await service.GetFeedAsync(filterByInterests: true); throw new Exception("Anonymous filtering was allowed"); }
        catch (UnauthorizedAccessException) { Check(true, "Filtering still requires authentication"); }
        try { await service.GetInterestsAsync(); throw new Exception("Anonymous interests were allowed"); }
        catch (UnauthorizedAccessException) { Check(true, "Reading selected interests requires authentication"); }
        Console.WriteLine("All interest feed checks passed.");
    }
}
