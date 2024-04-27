using System.ComponentModel.DataAnnotations;
using Amazon.DynamoDBv2.DataModel;

namespace MeetingApp.Models;
public class GetAllMeetingsRequestModel {
    private const string _ = "_";
    private const string Prefix = "begins@";
    [Required]
    public string? Room { get; set; }
    [Required]
    [MaxLength(8)]
    [MinLength(8)]
    [RegularExpression(@"^[0-9]*$")]
    public string? StartDate { get; set; }

    public string GetPartitionKey(){
        return StartDate + _ + Room;
    }

    public string GetSortKeyPrefix(){
        return Prefix;
    }
}

[DynamoDBTable("MeetingsTable")]
public class DynamoDBMeetingsTableItem {
    [DynamoDBHashKey("date_room")]
    public string? PartitionKey { get; set; }
    [DynamoDBRangeKey("time")]
    public string? SortKey { get; set; }
    [DynamoDBProperty("title")]
    public string? Title { get; set; }
    [DynamoDBProperty("end_at")]
    public string? EndAt { get; set; }
    [DynamoDBProperty("email")]
    public string? Email { get; set; }
}

public class GetAllMeetingsResponseModel {
    public List<GetAllMeetingParts> Body { get; set;} 
        = new List<GetAllMeetingParts>();
}

public class GetAllMeetingParts {
    public GetAllMeetingParts(
        DynamoDBMeetingsTableItem item, string room)
    {
        Room = room;
        Title = item.Title;
        StartAt = GetStartAt(item.SortKey!);
        EndAt = item.EndAt;
        Email = item.Email;
    }
    public string? Room { get; set; }
    public string? Title { get; set; }
    public string? StartAt { get; set; }
    public string? EndAt { get; set; }
    public string? Email { get; set; }

    public string GetStartAt(string sortKey){
        return sortKey.Split("@")[1];
    }
}