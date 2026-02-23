using AlohaAI.Models;
using SQLite;

namespace AlohaAI.Services;

public class StreakService : IStreakService
{
    private SQLiteAsyncConnection? _db;

    private async Task<SQLiteAsyncConnection> GetDbAsync()
    {
        if (_db != null) return _db;
        var path = Path.Combine(FileSystem.AppDataDirectory, "alohaai.db");
        _db = new SQLiteAsyncConnection(path);
        await _db.CreateTableAsync<UserStreak>();
        return _db;
    }

    public async Task RecordActivityAsync()
    {
        var db = await GetDbAsync();
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var existing = await db.Table<UserStreak>()
            .Where(s => s.Date == today)
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            existing.LessonsCompleted++;
            await db.UpdateAsync(existing);
        }
        else
        {
            await db.InsertAsync(new UserStreak
            {
                Date = today,
                LessonsCompleted = 1
            });
        }
    }

    public async Task<int> GetCurrentStreakAsync()
    {
        var db = await GetDbAsync();
        var streaks = await db.Table<UserStreak>()
            .OrderByDescending(s => s.Date)
            .ToListAsync();

        if (streaks.Count == 0) return 0;

        var streak = 0;
        var expectedDate = DateTime.UtcNow.Date;

        foreach (var entry in streaks)
        {
            var entryDate = DateTime.Parse(entry.Date).Date;

            if (entryDate == expectedDate)
            {
                streak++;
                expectedDate = expectedDate.AddDays(-1);
            }
            else if (entryDate == expectedDate.AddDays(-1) && streak == 0)
            {
                // Allow yesterday to count if nothing today yet
                streak++;
                expectedDate = entryDate.AddDays(-1);
            }
            else
            {
                break;
            }
        }

        return streak;
    }

    public async Task<int> GetBestStreakAsync()
    {
        var db = await GetDbAsync();
        var streaks = await db.Table<UserStreak>()
            .OrderBy(s => s.Date)
            .ToListAsync();

        if (streaks.Count == 0) return 0;

        var best = 1;
        var current = 1;

        for (var i = 1; i < streaks.Count; i++)
        {
            var prev = DateTime.Parse(streaks[i - 1].Date).Date;
            var curr = DateTime.Parse(streaks[i].Date).Date;

            if ((curr - prev).Days == 1)
            {
                current++;
                if (current > best) best = current;
            }
            else
            {
                current = 1;
            }
        }

        return best;
    }

    public async Task<int> GetTodayLessonsCountAsync()
    {
        var db = await GetDbAsync();
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var entry = await db.Table<UserStreak>()
            .Where(s => s.Date == today)
            .FirstOrDefaultAsync();
        return entry?.LessonsCompleted ?? 0;
    }
}
