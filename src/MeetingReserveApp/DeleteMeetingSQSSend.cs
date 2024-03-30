using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.SQS;
using Amazon.SQS.Model;

//[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace MeetingReserveApp;
public class DeleteMeetingSQSSend
{
    //Todo: こいつ環境変数
    private string? QueueURL = 
      Environment.GetEnvironmentVariable("SEND_QUEUE_NAME");
    private const string MessageGroupId = "DeleteMeeting";
    private const string MessageDeduplicationId = "DeleteMeetingDup";
    public IAmazonSQS _sqsClient = new AmazonSQSClient();
    /// <summary>
    /// 会議室情報 削除(送り側)
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns>APIGatewayProxyResponseインスタンス</returns>
    public async Task<APIGatewayProxyResponse> DeleteMeetingSQSSendHandler(
        APIGatewayProxyRequest input, ILambdaContext context)
    {
        try
        {
            //リクエストのバリデーション
            var (validateOk, model) = ModelFactory.CreateModel<DeleteMeetingRequestModel>(input.Body);
            if(validateOk == false || model == null) return CreateResponse(CommonResult.ValidateError);
            //SQSにデータ送信
            await SendMessage(_sqsClient, QueueURL, input.Body);
            //レスポンス返却
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
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// SQSキューへメッセージ送信
    /// </summary>
    /// <param name="sqsClient"></param>
    /// <param name="queueURL"></param>
    /// <param name="messageBody"></param>
    /// <returns>キューに送信した結果のHTTPステータスコード</returns>
    private static async Task SendMessage(
      IAmazonSQS sqsClient, string queueUrl, string messageBody)
    {
      SendMessageRequest sendMessageRequest = new SendMessageRequest();
      sendMessageRequest.QueueUrl = queueUrl;
      sendMessageRequest.MessageGroupId = MessageGroupId;
      sendMessageRequest.MessageBody = messageBody;
      sendMessageRequest.MessageDeduplicationId = MessageDeduplicationId;

      SendMessageResponse response =
        await sqsClient.SendMessageAsync(sendMessageRequest);
      Console.WriteLine($"Message added to queue\n  {queueUrl}");
      //一旦ステータスコード200以外はNG
      if((int)response.HttpStatusCode != CommonResult.OK){
          Console.WriteLine("Failed to send message");
          throw new Exception();
      }
    }
}
