using Amazon.DynamoDBv2.Model;
using MeetingApp.Models;

public interface IGetConferenceRepository {
    Task<List<DynamoDBMeetingsTableItem>?> GetAll(string partitionKey, string sortKeyPrefix);
    Task<GetItemResponse?> GetOne(string partitionKey, string sortKey);
}