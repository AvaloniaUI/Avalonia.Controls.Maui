namespace AlohaAI.Models;

public class UserProgress
{
    public int Id { get; set; }

    public string PathId { get; set; } = string.Empty;

    public string ModuleId { get; set; } = string.Empty;

    public string LessonId { get; set; } = string.Empty;

    public bool Completed { get; set; }

    public DateTime? CompletedAt { get; set; }

    public double? QuizScore { get; set; }

    public int XpEarned { get; set; }
}

public class UserStreak
{
    public int Id { get; set; }

    public string Date { get; set; } = string.Empty;

    public int LessonsCompleted { get; set; }
}

public class UserSetting
{
    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}
