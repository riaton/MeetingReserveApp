using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using MeetingApp.Infrastructure;
using MeetingApp.Models;
using Amazon.DynamoDBv2;

//[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace MeetingApp;
public class RegisterMeeting
{
    private readonly IUpdateConferenceRepository _repository;

    public RegisterMeeting()
    {
        _repository = new DynamoDBUpdate(new AmazonDynamoDBClient());
    }
    public RegisterMeeting(IUpdateConferenceRepository repository)
    {
        _repository = repository;
    }
    private SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    /// 会議室情報 登録
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns>APIGatewayProxyResponseインスタンス</returns>
    public async Task<APIGatewayProxyResponse> RegisterMeetingHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        await _semaphore.WaitAsync();
        try
        {
            //リクエストのバリデーション
            var (validateOk, model) = ModelFactory.CreateModel<RegisterMeetingRequestModel>(input.Body);
            if(validateOk == false || model == null) return CreateResponse(CommonResult.ValidateError);

            //DynamoDBへデータ登録
            int res = await _repository.Register(model);
            if(res != CommonResult.OK) {
                Console.WriteLine("A meeting register failed");
                return CreateResponse(CommonResult.InternalServerError);
            }
            
            return CreateResponse(CommonResult.OK);
        }
        catch(Exception e)
        {
            Console.WriteLine($"Exception occurred, " + e);
            return CreateResponse(CommonResult.InternalServerError);
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    /// <summary>
    /// レスポンス生成
    /// </summary>
    /// <param name="statusCode"></param>
    /// <param name="message"></param>
    /// <returns>APIGatewayProxyResponseインスタンス</returns>
    private static APIGatewayProxyResponse CreateResponse(int statusCode){
        return new APIGatewayProxyResponse{
            StatusCode = statusCode,
            Headers = CommonResult.ResponseHeader
        };
    }
}
