using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.SQS.Model;
using MeetingApp.Infrastructure;
using MeetingApp.Models;
using Amazon.DynamoDBv2;

//[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace MeetingApp;
public class DeleteMeetingSQSReceive
{
    private readonly IDeleteConferenceRepository _repository;
    private readonly IAmazonSQS _sqsClient;
    private readonly string? QueueURL = 
      Environment.GetEnvironmentVariable("DLQ_URL");

    public DeleteMeetingSQSReceive()
    {
        _sqsClient = new AmazonSQSClient();
        _repository = new DynamoDBDelete(new AmazonDynamoDBClient());
    }
    public DeleteMeetingSQSReceive(IDeleteConferenceRepository repository, 
        IAmazonSQS client)
    {
        _repository = repository;
        _sqsClient = client;
    }
    private SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    /// 会議室予約 削除(受け側)
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    public async Task DeleteMeetingSQSReceiveHandler(
        SQSEvent input, ILambdaContext context)
    {
        await _semaphore.WaitAsync();
        try
        {
            foreach(var record in input.Records){
                //SQSからデータ受信
                var (validateOk, model) = ModelFactory.CreateModel<DeleteMeetingRequestModel>(record.Body);
                if(validateOk == false || model == null){
                    Console.WriteLine($"Receive message validation failed, {record.Body}");
                    //DLQに送信
                    await SendMessage(_sqsClient, QueueURL, record.Body);
                    throw new Exception();
                };
                //削除
                int res = await _repository.Delete(model!);
                if(res == CommonResult.OK){
                    Console.WriteLine($"delete succeeded, {record.Body}");
                }
                else
                {
                    Console.WriteLine($"delete failed, {record.Body}");
                    //DLQに送信
                    await SendMessage(_sqsClient, QueueURL, record.Body);
                }
            }
        }
        catch(Exception e)
        {
            //ログを出して終了
            Console.WriteLine($"Exception occurred, " + e);
        }
        finally
        {
            _semaphore.Release();
        }
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
        sendMessageRequest.MessageBody = messageBody;

        SendMessageResponse response =
            await sqsClient.SendMessageAsync(sendMessageRequest);
        Console.WriteLine($"Message added to dead letter queue\n  {queueUrl}");
        //ステータスコード200以外はNG
        if((int)response.HttpStatusCode != CommonResult.OK){
            Console.WriteLine($"Failed to send message to dead letter queue, {messageBody}");
            throw new Exception();
        }
    }
}
