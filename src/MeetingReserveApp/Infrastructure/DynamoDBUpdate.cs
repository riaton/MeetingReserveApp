using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using MeetingApp.Models;

namespace MeetingApp.Infrastructure;
public class DynamoDBUpdate : IUpdateConferenceRepository {
    private readonly IAmazonDynamoDB _client;
    private readonly string? _table;
    public UpdateItemRequest updateRequest = new();
    public BatchWriteItemRequest registerRequest = new();

    public DynamoDBUpdate(IAmazonDynamoDB client, string? tableName=null)
    {
        _client = client;
        if(tableName == null){
            _table = Environment.GetEnvironmentVariable("TABLE_NAME");
        } else {
            _table = tableName;
        }
    }
    
    /// <summary>
    /// 会議室情報 登録(DBアクセス)
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<int> Register(RegisterMeetingRequestModel model){
        try
        {
            if(_table == null) {
                Console.WriteLine("Environment variable of table name is null at Register()");
                return CommonResult.InternalServerError;
            }

            CreateRegisterQuery(model);
            
            BatchWriteItemResponse result;
            do
            {
                result = await _client.BatchWriteItemAsync(registerRequest);
                registerRequest.RequestItems = result.UnprocessedItems;
            } while (result.UnprocessedItems.Count > 0);

            return (int)result.HttpStatusCode;
        }
        catch(Exception e)
        {
            Console.WriteLine($"Failed to register to dynamodb at Register(), " + e);
            return CommonResult.InternalServerError;
        }
    }

    /// <summary>
    /// 会議室情報 更新(DBアクセス)
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<int> Update(UpdateMeetingRequestModel model){
        try
        {
            if(_table == null) {
                Console.WriteLine("Environment variable of table name is null at Update()");
                return CommonResult.InternalServerError;
            }
            CreateUpdateQuery(model);

            var res = await _client.UpdateItemAsync(updateRequest);
            
            return (int)res.HttpStatusCode;
        }
        catch(Exception e)
        {
            Console.WriteLine($"Failed to update dynamodb at Update(), " + e);
            return CommonResult.InternalServerError;
        }
    }

    /// <summary>
    /// 会議室情報 登録 リクエスト生成
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public void CreateRegisterQuery(RegisterMeetingRequestModel model){
        List<WriteRequest> requests = new();
        List<string> fillList = model.CreateFill();
        if(fillList.Count == 0) throw new Exception("Max batch count exceeded or you must specify time parameter in (0,15,30,45) minutes");
        //@begins
        Dictionary<string, AttributeValue> beginsItem = new();
        beginsItem["date_room"] = new AttributeValue { S = model.CreatePartitionKey() };
        beginsItem["time"] = new AttributeValue { S = model.CreateBeginsSortKey() };
        beginsItem["title"] = new AttributeValue { S = model.Title };
        beginsItem["email"] = new AttributeValue { S = model.Email };
        beginsItem["end_at"] = new AttributeValue { S = model.EndAt };
        if(model.Contents is not null){
            beginsItem["contents"] = new AttributeValue { S = model.Contents };
        }
        if(model.Participants is not null && model.Participants.Any()){
            beginsItem["members"] = new AttributeValue { SS = model.Participants };
        }
        requests.Add(new WriteRequest{
            PutRequest = new PutRequest { Item = beginsItem }
        });
        //@fill
        foreach(var fill in fillList){
            Dictionary<string, AttributeValue> item = new();
            item["date_room"] = new AttributeValue { S = model.CreatePartitionKey() };
            item["time"] = new AttributeValue { S = fill };
            item["end_at"] = new AttributeValue { S = model.EndAt };
            requests.Add(new WriteRequest{
                PutRequest = new PutRequest { Item = item }
            });
        }
        Dictionary<string, List<WriteRequest>> requestItem = new();
        requestItem[_table!] = requests;
        registerRequest.RequestItems = requestItem;
    }

    /// <summary>
    /// 会議室情報 更新 リクエスト生成
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public void CreateUpdateQuery(UpdateMeetingRequestModel model){
        model.Contents ??= string.Empty;
        model.Participants ??= new List<string>();

        updateRequest.TableName = _table;
        updateRequest.Key["date_room"] = new AttributeValue{ S = model.CreatePartitionKey() };
        updateRequest.Key["time"] = new AttributeValue{ S = model.CreateBeginsSortKey() };
        updateRequest.ExpressionAttributeNames = new Dictionary<string, string>(){
                { "#v1", "title" },
                { "#v2", "contents"},
                { "#v3", "members"},
                { "#v4", "email" }};
        updateRequest.ExpressionAttributeValues = new Dictionary<string, AttributeValue>(){
                { ":title", new AttributeValue(){ S = model.Title }},
                { ":contents", new AttributeValue(){ S = model.Contents }},
                { ":members", new AttributeValue(){ SS = model.Participants }},
                { ":requested_by", new AttributeValue(){ S = model.Email }}};
        updateRequest.UpdateExpression = "SET #v1 = :title, #v2 = :contents, #v3 = :members";
        updateRequest.ConditionExpression = "#v4 = :requested_by";
    }
}
