using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using MeetingApp.Infrastructure;
using MeetingApp.Models;
using Amazon.DynamoDBv2;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace MeetingApp;
public class UpdateMeeting
{
    private readonly IUpdateConferenceRepository _repository;

    public UpdateMeeting()
    {
        _repository = new DynamoDBUpdate(new AmazonDynamoDBClient());
    }
    public UpdateMeeting(IUpdateConferenceRepository repository)
    {
        _repository = repository;
    }
    private SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    /// 会議室情報 更新
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns>APIGatewayProxyResponseインスタンス</returns>
    public async Task<APIGatewayProxyResponse> UpdateMeetingHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        await _semaphore.WaitAsync();
        try
        {
            //リクエストのバリデーション
            var (validateOk, model) = ModelFactory.CreateModel<UpdateMeetingRequestModel>(input.Body);
            if(validateOk == false || model == null) return CreateResponse(CommonResult.ValidateError);
            //DynamoDBデータ更新
            int res = await _repository.Update(model);
            if(res != CommonResult.OK) {
                Console.WriteLine("A meeting update failed");
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
            Headers = CommonResult.ResponseHeader,
        };
    }
}
