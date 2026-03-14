using MyConference.Models;

namespace MyConference.Services;

public interface ISessionizeService
{
    Task<SessionizeData?> GetConferenceDataAsync(bool forceRefresh = false);
}
