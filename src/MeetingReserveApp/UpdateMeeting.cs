using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace MeetingReserveApp;
public class UpdateMeeting
{
    public IUpdateConferenceRepository repository = new DynamoDBUpdate();
    /// <summary>
    /// 会議室情報 更新
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns>APIGatewayProxyResponseインスタンス</returns>
    public async Task<APIGatewayProxyResponse> UpdateMeetingHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        try
        {
            //リクエストのバリデーション
            var (validateOk, model) = ModelFactory.CreateModel<UpdateMeetingRequestModel>(input.Body);
            if(validateOk == false || model == null) return CreateResponse(CommonResult.ValidateError);

            //DynamoDBデータ更新
            int res = await repository.Update(model);
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
