using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using MeetingApp.Infrastructure;
using MeetingApp.Models;

//[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace MeetingApp;
public class GetAllMeetings
{
    public IGetConferenceRepository repository = new DynamoDBAccess();
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
    /// <summary>
    /// 会議室情報 全取得
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns>APIGatewayProxyResponseインスタンス</returns>
    public async Task<APIGatewayProxyResponse> GetAllMeetingsHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        try
        {
            //リクエストのバリデーション
            var pathParameters = JsonSerializer.Serialize(input.PathParameters, _options);
            var (validateOk, model) = ModelFactory.CreateModel<GetAllMeetingsRequestModel>(pathParameters);
            if(validateOk == false || model == null) return CreateErrorResponse(CommonResult.ValidateError);

            //DynamoDBからデータ取得
            var res = await repository.GetAll(model.GetPartitionKey(), 
                model.GetSortKeyPrefix());
            if(res == null) return CreateErrorResponse(CommonResult.DataNotFound);
            
            GetAllMeetingsResponseModel resModel = new();
            foreach(var r in res){
                resModel.Body.Add(new GetAllMeetingParts(r, model.Room!));
            }
            
            return new APIGatewayProxyResponse
            {
                StatusCode = CommonResult.OK,
                Headers = CommonResult.ResponseHeader,
                Body = JsonSerializer.Serialize(resModel, _options)
            };
        }
        catch(Exception e)
        {
            Console.WriteLine($"Exception occurred, " + e);
            return CreateErrorResponse(CommonResult.InternalServerError);
        }
    }
    /// <summary>
    /// レスポンス生成
    /// </summary>
    /// <param name="statusCode"></param>
    /// <param name="message"></param>
    /// <returns>APIGatewayProxyResponseインスタンス</returns>
    private APIGatewayProxyResponse CreateErrorResponse(int statusCode){
        return new APIGatewayProxyResponse{
            StatusCode = statusCode,
            Headers = CommonResult.ResponseHeader,
            Body = CommonResult.FromResult(statusCode)
        };
    }
}
