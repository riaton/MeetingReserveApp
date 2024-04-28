using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using MeetingApp.Models;

namespace MeetingApp.Infrastructure;

public class DynamoDBAccess : IGetConferenceRepository {
    private readonly IAmazonDynamoDB _client;
    private readonly IDynamoDBContext _context;
    private readonly string? Table = Environment.GetEnvironmentVariable("TABLE_NAME");
    public DynamoDBAccess(IAmazonDynamoDB client, IDynamoDBContext context)
    {
        _client = client;
        _context = context;
    }
    
    /// <summary>
    /// 会議室情報 全取得(DBアクセス)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<List<DynamoDBMeetingsTableItem>?> GetAll(string partitionKey, string sortKeyPrefix){
        try
        {
            if(Table == null){
                Console.WriteLine("Environment variable of table name is null at GetAll()");
                return null;
            }
            var sortKeys = new[]{ sortKeyPrefix };
            var items = await _context.QueryAsync<DynamoDBMeetingsTableItem>
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
            if(Table == null){
                Console.WriteLine("Environment variable of table name is null at GetOne()");
                return null;
            }
            var param = new GetItemRequest{
                Key = new Dictionary<string, AttributeValue>(){
                {"date_room", new AttributeValue{ S = partitionKey }},
                {"time", new AttributeValue{ S = sortKey }}},
                TableName = Table
            };
            var response = await _client.GetItemAsync(param);
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