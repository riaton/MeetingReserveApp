using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using MeetingApp.Models;

namespace MeetingApp.Infrastructure;

public class DynamoDBAccess : IGetConferenceRepository {
    private readonly IAmazonDynamoDB _client;
    private readonly IDynamoDBContext _context;
    private readonly string? _table;
    public GetItemRequest request = new();
    public DynamoDBAccess(IAmazonDynamoDB client, IDynamoDBContext context, string? tableName=null)
    {
        _client = client;
        _context = context;
        if(tableName == null){
            _table = Environment.GetEnvironmentVariable("TABLE_NAME");
        } else {
            _table = tableName;
        }
    }
    
    /// <summary>
    /// 会議室情報 全取得(DBアクセス)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<List<DynamoDBMeetingsTableItem>?> GetAll(string partitionKey, string[] sortKeyPrefix){
        if(_table == null){
            Console.WriteLine("Environment variable of table name is null at GetAll()");
            return null;
        }

        var items = await _context.QueryAsync<DynamoDBMeetingsTableItem>
            (partitionKey, QueryOperator.BeginsWith, sortKeyPrefix)
            .GetRemainingAsync();
            
        if(items.Count == 0) {
            Console.WriteLine("Failed to get from dynamodb at GetAll()"); 
            return null;
        }

        return items;
    }
    
    /// <summary>
    /// 会議室情報 個別取得(DBアクセス)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<GetItemResponse?> GetOne(string partitionKey, string sortKey){
        if(_table == null){
            Console.WriteLine("Environment variable of table name is null at GetOne()");
            return null;
        }

        CreateGetOneQuery(partitionKey, sortKey);

        var response = await _client.GetItemAsync(request);
        if(response == null || response.Item.Count == 0) {
            if(response == null) Console.WriteLine("response is null");
            Console.WriteLine("Failed to get from dynamodb at GetOne()"); 
            return null;
        }

        return response;
    }

    /// <summary>
    /// 会議室情報 個別取得 リクエスト生成
    /// </summary>
    /// <param name="partitionKey"></param>
    /// <param name="sortKey"></param>
    /// <returns></returns>
    public void CreateGetOneQuery(string partitionKey, string sortKey){
        request.Key = new Dictionary<string, AttributeValue>(){
                {"date_room", new AttributeValue{ S = partitionKey }},
                {"time", new AttributeValue{ S = sortKey }}};
        request.TableName = _table;
    }
}