using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using MeetingApp.Models;

namespace MeetingApp.Infrastructure;
public class DynamoDBDelete : IDeleteConferenceRepository {
    private readonly IAmazonDynamoDB _client;
    private readonly string? _table;
    public DeleteItemRequest request = new();

    public DynamoDBDelete(IAmazonDynamoDB client, string? tableName=null)
    {
        _client = client;
        if(tableName == null){
            _table = Environment.GetEnvironmentVariable("TABLE_NAME");
        } else {
            _table = tableName;
        }
    }
    
    /// <summary>
    /// 会議室情報 削除(DBアクセス)
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<int> Delete(DeleteMeetingRequestModel model){
        try
        {
            if(_table == null){
                Console.WriteLine("Environment variable of table name is null at Delete()");
                return CommonResult.InternalServerError;
            }

            CreateDeleteQuery(model);
            var res = await _client.DeleteItemAsync(request);
            
            return (int)res.HttpStatusCode;
        }
        catch(Exception e)
        {
            Console.WriteLine($"Failed to delete dynamodb record at Delete(), " + e);
            return CommonResult.InternalServerError;
        }
    }

    /// <summary>
    /// 会議室情報 削除 リクエスト生成
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public void CreateDeleteQuery(DeleteMeetingRequestModel model){
        request.TableName = _table;
        request.Key["date_room"] = new AttributeValue{ S = model.CreatePartitionKey() };
        request.Key["time"] = new AttributeValue{ S = model.CreateSortKey() };
        request.ExpressionAttributeNames = new Dictionary<string, string>(){
                { "#v", "end_at" }};
        request.ExpressionAttributeValues = new Dictionary<string, AttributeValue>(){
                { ":end_at", new AttributeValue(){ S = model.EndAt }}};
        request.ConditionExpression = "#v = :end_at";
    }
}
