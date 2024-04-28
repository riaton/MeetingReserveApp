using System.ComponentModel.DataAnnotations;

namespace MeetingApp.Models;
public class RegisterMeetingRequestModel : IValidatableObject {
    private const string FillPrefix = "fill@";
    private const string BeginsPrefix = "begins@";
    private const int BatchWriteMaxCount = 25;
    private const string _ = "_";
    [Required]
    [MaxLength(50)]
    public string? Email { get; set; }
    [Required]
    [MaxLength(50)]
    public string? Title { get; set; }
    [Required]
    public string? Room { get; set; }
    [Required]
    [MaxLength(8)]
    [MinLength(8)]
    [RegularExpression(@"^[0-9]*$")]
    public string? Date { get; set; }
    [Required]
    [MaxLength(4)]
    [MinLength(4)]
    [RegularExpression(@"^[0-9]*$")]
    public string? StartAt { get; set; }
    [Required]
    [MaxLength(4)]
    [MinLength(4)]
    [RegularExpression(@"^[0-9]*$")]
    public string? EndAt { get; set; }
    public string? Contents { get; set; }
    public List<string>? Participants { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context){
        var results = new List<ValidationResult>();
        if(int.Parse(StartAt!) - int.Parse(EndAt!) >= 0){
            results.Add(new ValidationResult("StartAt cannot be later than EndAt." +
                $"StartAt={StartAt}, EndAt={EndAt}"));
        }
        return results;
    }

    public string CreatePartitionKey(){
        return Date + _ + Room;
    }

    public string CreateBeginsSortKey(){
        return BeginsPrefix + StartAt;
    }

    public List<string> CreateFill(){
        List<string> fillList = new();
        int iterator = int.Parse(StartAt!);
        int end = int.Parse(EndAt!);
        while(iterator != end){
            fillList.Add(FillPrefix + iterator.ToString());
            if(fillList.Count == BatchWriteMaxCount) return new List<string>();
            iterator += 15;
            if((iterator + 40) % 100 == 0) iterator += 40;
        }
        return fillList;
    }
}
