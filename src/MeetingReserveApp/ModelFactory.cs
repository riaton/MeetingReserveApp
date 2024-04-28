using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace MeetingApp;
internal static class ModelFactory {
    public static (bool validateOk, T? model) CreateModel<T>(string body){
        var result = false;
        T? model = default;
        try
        {
            var req = JsonSerializer.Deserialize<T>(body);
            if(req is null){
                Console.WriteLine("model deserialize error");
            }
            else
            {
                model = req;
                var ctx = new ValidationContext(req);
                var results = new List<ValidationResult>();
                if(Validator.TryValidateObject(req, ctx, results, true))
                {
                    result = true;
                }
                else
                {
                    Console.WriteLine($"Validation error body = {body}");
                    foreach(var r in results)
                    {
                        Console.WriteLine($"Validation error reason: {r}");
                    }
                }
            }
        }
        catch(Exception e){
            Console.WriteLine("Exception occurred at modelfactory, " + e);
        }

        return (result, model);
    }
}