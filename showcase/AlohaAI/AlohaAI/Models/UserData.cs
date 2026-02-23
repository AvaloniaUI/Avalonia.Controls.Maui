using SQLite;

namespace AlohaAI.Models;

[Table("UserProgress")]
public class UserProgress
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string PathId { get; set; } = string.Empty;

    [Indexed]
    public string ModuleId { get; set; } = string.Empty;

    public string LessonId { get; set; } = string.Empty;

    public bool Completed { get; set; }

    public DateTime? CompletedAt { get; set; }

    public double? QuizScore { get; set; }

    public int XpEarned { get; set; }
}

[Table("UserStreaks")]
public class UserStreak
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique]
    public string Date { get; set; } = string.Empty;

    public int LessonsCompleted { get; set; }
}

[Table("UserSettings")]
public class UserSetting
{
    [PrimaryKey]
    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}
