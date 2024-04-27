using System.ComponentModel.DataAnnotations;
using Amazon.DynamoDBv2.Model;

namespace MeetingApp.Models;
public class GetOneRequestModel {
    private const string _ = "_";
    private const string prefix = "begins@";
    [Required]
    public string? Room { get; set; }
    [Required]
    [MaxLength(12)]
    [MinLength(12)]
    [RegularExpression(@"^[0-9]*$")]
    public string? StartAt { get; set; }

    public string GetPartitionKey(){
        string day = StartAt!.Substring(0, 8);
        return day + _ + Room;
    }

    public string GetSortKey(){
        string time = StartAt!.Substring(8, 4);
        return prefix + time;
    }
}

public class GetOneResponseModel {
    private const int EndAtLength = 4;
    public GetOneResponseModel(GetItemResponse? table = null)
    {
        if(table == null) return;

        var listDateFrom = new List<int>();
        var listHourFrom = new List<int>();
        var listHourTo = new List<int>();
        foreach(var item in table.Item){
            if(item.Key == "title") Title = item.Value.S;
            if(item.Key == "date_room") {
                Room = ConvertRoom(item.Value.S);
                listDateFrom = ConvertDateFrom(item.Value.S);
            }
            if(item.Key == "time") listHourFrom = ConvertHourFrom(item.Value.S);
            if(item.Key == "contents") Contents = item.Value.S;
            if(item.Key == "members") Participants = item.Value.SS;
            if(item.Key == "email") Email = item.Value.S;
            if(item.Key == "end_at") listHourTo = ConvertEndAt(item.Value.S);
            if(listHourTo == null){
                Status = false;
                return;
            }
        }
        YearFrom = listDateFrom[0];
        MonthFrom = listDateFrom[1];
        DayFrom = listDateFrom[2];
        HourFrom = listHourFrom[0];
        MinFrom = listHourFrom[1];
        HourTo = listHourTo[0];
        MinTo = listHourTo[1];
    }
    public bool Status { get; set; } = true;
    public string? Email { get; }
    public string? Title { get; }
    public string? Room { get; }
    public int? YearFrom { get; }
    public int? MonthFrom { get; }
    public int? DayFrom { get; }
    public int? HourFrom { get; }
    public int? MinFrom { get; }
    public int? HourTo { get; }
    public int? MinTo { get; }
    public string? Contents { get; }
    public List<string>? Participants { get; }
    private List<int> ConvertHourFrom(string time){
        string hourFrom = time.Split('@')[1];
        string hour = hourFrom.Substring(0, 2);
        string min = hourFrom.Substring(2, 2);

        return new List<int>(){Convert.ToInt32(hour), Convert.ToInt32(min)};
    }

    private List<int>? ConvertEndAt(string? endAt){
        if(endAt == null) return null;
        if(endAt.Length != EndAtLength) return null;

        string hour = endAt.Substring(0, 2);
        string min = endAt.Substring(2, 2);

        return new List<int>(){Convert.ToInt32(hour), Convert.ToInt32(min)};
    }

    private string ConvertRoom(string dateRoom){
        return dateRoom.Split('_')[1];
    }

    private List<int> ConvertDateFrom(string dateRoom){
        string date = dateRoom.Split('_')[0];
        string year = date.Substring(0, 4);
        string month = date.Substring(4, 2);
        string day = date.Substring(6, 2);

        return new List<int>(){Convert.ToInt32(year), 
            Convert.ToInt32(month), Convert.ToInt32(day)};
    }
}
