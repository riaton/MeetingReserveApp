using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;

//[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace MeetingReserveApp;
public class DeleteMeetingSQSReceive
{
    //Todo: こいつ環境変数
    private string? QueueURL = 
      Environment.GetEnvironmentVariable("DLQ_NAME");
    private const string MessageGroupId = "DeleteMeetingFailed";
    private const string MessageDeduplicationId = "DeleteMeetingFailedDup";
    public IAmazonSQS sqsClient = new AmazonSQSClient();
    public IDeleteConferenceRepository repository = new DynamoDBDelete();
    /// <summary>
    /// 会議室予約 削除(受け側)
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    public async Task DeleteMeetingSQSReceiveHandler(
        SQSEvent input, ILambdaContext context)
    {
        try
        {
            foreach(var record in input.Records){
                //SQSからデータ受信
                var model = JsonSerializer.Deserialize<DeleteMeetingRequestModel>(record.Body);
                //削除
                int res = await repository.Delete(model!);
                if(res != CommonResult.OK){
                    //デッドレターキューへ
                    await SendMessage(sqsClient, QueueURL, record.Body);
                }
            }
        }
        catch(Exception e)
        {
            //ログを出して終了
            Console.WriteLine($"Exception occurred, " + e);
        }
    }
    /// <summary>
    /// デッドレターキューへメッセージ送信
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
