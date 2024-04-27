namespace MeetingApp.Models;

public static class CommonResult {
    public const int OK = 200;
    public const int ValidateError = 400;
    public const int DataNotFound = 404;
    public const int InternalServerError = 500;
    public static IDictionary<string, string> ResponseHeader { get; set; }
        = new Dictionary<string, string>(){
            {"Access-Control-Allow-Headers", "Content-Type"},
            {"Access-Control-Allow-Origin", "*"},
            {"Access-Control-Allow-Methods", "OPTIONS,POST,GET"}
        };

    public static string FromResult(int resultCode){
        switch(resultCode){
            case OK:
                return "OK";
            case ValidateError:
                return "Validation Error";
            case DataNotFound:
                return "DB data not found";
            case InternalServerError:
                return "Internal Server Error";
            default:
                return "What is this error?";
        }
    }
}