using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

public class DynamoDBDelete : IDeleteConferenceRepository {
    //Todo: こいつ環境変数
    private const string Table = "MeetingsTable";
    public AmazonDynamoDBClient client = new AmazonDynamoDBClient();

    /// <summary>
    /// 会議室情報 削除(DBアクセス)
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<int> Delete(DeleteMeetingRequestModel model){
        try
        {
            var param = new DeleteItemRequest{
                TableName = Table,
                Key = new Dictionary<string, AttributeValue>(){
                {"date_room", new AttributeValue{ S = model.CreatePartitionKey() }}},
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
