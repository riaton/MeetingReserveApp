using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

//[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace MeetingReserveApp;
public class RegisterMeeting
{
    public IUpdateConferenceRepository repository = new DynamoDBUpdate();
    /// <summary>
    /// 会議室情報 登録
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns>APIGatewayProxyResponseインスタンス</returns>
    public async Task<APIGatewayProxyResponse> RegisterMeetingHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        try
        {
            //リクエストのバリデーション
            var (validateOk, model) = ModelFactory.CreateModel<RegisterMeetingRequestModel>(input.Body);
            if(validateOk == false || model == null) return CreateResponse(CommonResult.ValidateError);

            //DynamoDBへデータ登録
            int res = await repository.Register(model);
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
    }
    /// <summary>
    /// レスポンス生成
    /// </summary>
    /// <param name="statusCode"></param>
    /// <param name="message"></param>
    /// <returns>APIGatewayProxyResponseインスタンス</returns>
    private APIGatewayProxyResponse CreateResponse(int statusCode){
        return new APIGatewayProxyResponse{
            StatusCode = statusCode,
            Body = CommonResult.FromResult(statusCode)
        };
    }
}
