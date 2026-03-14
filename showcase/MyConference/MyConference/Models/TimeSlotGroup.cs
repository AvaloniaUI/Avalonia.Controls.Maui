namespace MyConference.Models;

public class TimeSlotGroup : List<Session>
{
    public string TimeSlot { get; }

    public string StartTime { get; }

    public TimeSlotGroup(string timeSlot, IEnumerable<Session> sessions) : base(sessions)
    {
        TimeSlot = timeSlot;
        StartTime = timeSlot.Split(" \u2013 ")[0];
    }
}
