using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.SQS;
using Amazon.SQS.Model;
using MeetingApp.Models;

//[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace MeetingApp;
public class DeleteMeetingSQSSend
{
    private readonly IAmazonSQS _sqsClient;
    private readonly string? QueueURL = 
      Environment.GetEnvironmentVariable("QUEUE_URL");
    private const string MessageGroupId = "DeleteMeeting";
    private const string MessageDeduplicationId = "DeleteMeetingDup";

    public DeleteMeetingSQSSend()
    {
        _sqsClient = new AmazonSQSClient();
    }
    public DeleteMeetingSQSSend(IAmazonSQS client)
    {
        _sqsClient = client;
    }
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
    private static APIGatewayProxyResponse CreateResponse(int statusCode){
        return new APIGatewayProxyResponse{
            StatusCode = statusCode,
            Headers = CommonResult.ResponseHeader,
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
        IAmazonSQS sqsClient, string? queueUrl, string messageBody)
    {
        SendMessageRequest sendMessageRequest = new SendMessageRequest();
        sendMessageRequest.QueueUrl = queueUrl;
        sendMessageRequest.MessageGroupId = MessageGroupId;
        sendMessageRequest.MessageBody = messageBody;
        sendMessageRequest.MessageDeduplicationId = MessageDeduplicationId;

        SendMessageResponse response =
            await sqsClient.SendMessageAsync(sendMessageRequest);
        Console.WriteLine($"Message added to queue\n  {queueUrl}");
        //ステータスコード200以外はNG
        if((int)response.HttpStatusCode != CommonResult.OK){
            Console.WriteLine("Failed to send message");
            throw new Exception();
        }
    }
}
