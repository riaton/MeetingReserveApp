using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

public class DynamoDBAccess : IGetConferenceRepository {
    //Todo: こいつ環境変数
    private const string Table = "MeetingsTable";
    public IAmazonDynamoDB _dynamoDBClient = new AmazonDynamoDBClient();
    
    /// <summary>
    /// 会議室情報 全取得(DBアクセス)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<List<DynamoDBMeetingsTableItem>?> GetAll(string partitionKey, string sortKeyPrefix){
        try
        {
            IDynamoDBContext context = new DynamoDBContext(_dynamoDBClient);
            var sortKeys = new[]{ sortKeyPrefix };
            var items = await context.QueryAsync<DynamoDBMeetingsTableItem>
                (partitionKey, QueryOperator.BeginsWith, sortKeys)
                .GetRemainingAsync(); 
            if(items == null || items.Count == 0) {
                throw new ResourceNotFoundException("DynamoDB get count is 0");
            }
            return items;
        }
        catch(ResourceNotFoundException e)
        {
            Console.WriteLine($"Failed to get from dynamodb at GetAll(), " + e.Message);
            return null;
        }
    }
    /// <summary>
    /// 会議室情報 個別取得(DBアクセス)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<GetItemResponse?> GetOne(string partitionKey, string sortKey){
        try
        {
            var param = new GetItemRequest{
                Key = new Dictionary<string, AttributeValue>(){
                {"date_room", new AttributeValue{ S = partitionKey }},
                {"time", new AttributeValue{ S = sortKey }}},
                TableName = Table
            };
            var response = await _dynamoDBClient.GetItemAsync(param);
            if(response.Item.Count() == 0) {
                throw new ResourceNotFoundException("GetOne count is 0");
            }

            return response;
        }
        catch(ResourceNotFoundException e)
        {
            Console.WriteLine($"Failed to get from dynamodb, " + e.Message);
            return null;
        }
    }
}