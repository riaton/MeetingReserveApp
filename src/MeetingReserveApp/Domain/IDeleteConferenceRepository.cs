public interface IDeleteConferenceRepository {
    Task<int> Delete(DeleteMeetingRequestModel model);
}