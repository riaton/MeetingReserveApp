using MeetingApp.Models;

public interface IUpdateConferenceRepository {
    Task<int> Register(RegisterMeetingRequestModel model);
    Task<int> Update(UpdateMeetingRequestModel model);
}