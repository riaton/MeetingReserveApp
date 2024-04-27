using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using MeetingApp.Models;

namespace MeetingApp.Infrastructure;
public class DynamoDBDelete : IDeleteConferenceRepository {
    private string? Table = Environment.GetEnvironmentVariable("TABLE_NAME");
    //private string? Table = "MeetingsTable";
    public AmazonDynamoDBClient client = new AmazonDynamoDBClient();

    /// <summary>
    /// 会議室情報 削除(DBアクセス)
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<int> Delete(DeleteMeetingRequestModel model){
        try
        {
            if(Table == null){
                Console.WriteLine("Environment variable of table name is null at Delete()");
                return CommonResult.InternalServerError;
            }
            var param = new DeleteItemRequest{
                TableName = Table,
                Key = new Dictionary<string, AttributeValue>(){
                {"date_room", new AttributeValue{ S = model.CreatePartitionKey() }},
                {"time", new AttributeValue{ S = model.CreateSortKey() }}},
                ExpressionAttributeNames = new Dictionary<string, string>(){
                    { "#v", "end_at" },
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>(){
                    { ":end_at", new AttributeValue(){ S = model.EndAt }},
                },
                ConditionExpression = "#v = :end_at"
            };
            var res = await client.DeleteItemAsync(param);
            
            return (int)res.HttpStatusCode;
        }
        catch(Exception e)
        {
            Console.WriteLine($"Failed to delete dynamodb record at Delete(), " + e);
            return CommonResult.InternalServerError;
        }
    }
}
