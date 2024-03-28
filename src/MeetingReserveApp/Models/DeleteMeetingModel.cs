using System.ComponentModel.DataAnnotations;

public class DeleteMeetingRequestModel {
    private const string FillPrefix = "fill@";
    private const string BeginsPrefix = "begins@";
    private const int BatchDeleteMaxCount = 25;
    private const string _ = "_";
    [Required]
    [MaxLength(50)]
    public string? Email { get; set; }
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
    public string CreatePartitionKey(){
        return Date + _ + Room;
    }
}
