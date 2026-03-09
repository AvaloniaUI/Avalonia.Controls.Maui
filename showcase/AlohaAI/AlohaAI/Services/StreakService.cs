using AlohaAI.Models;

namespace AlohaAI.Services;

public class StreakService : IStreakService
{
    private readonly IDatabaseService _db;

    public StreakService(IDatabaseService db)
    {
        _db = db;
    }

    public async Task RecordActivityAsync()
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var existing = await _db.FindStreakByDateAsync(today);

        if (existing != null)
        {
            existing.LessonsCompleted++;
            await _db.SaveStreakAsync(existing);
        }
        else
        {
            await _db.SaveStreakAsync(new UserStreak
            {
                Date = today,
                LessonsCompleted = 1
            });
        }
    }

    public async Task<int> GetCurrentStreakAsync()
    {
        var streaks = await _db.GetAllStreaksAsync(descending: true);

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
        var streaks = await _db.GetAllStreaksAsync(descending: false);

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
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var entry = await _db.FindStreakByDateAsync(today);
        return entry?.LessonsCompleted ?? 0;
    }
}
