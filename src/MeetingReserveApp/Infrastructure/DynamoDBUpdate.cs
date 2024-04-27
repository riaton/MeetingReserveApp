using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using MeetingApp.Models;

namespace MeetingApp.Infrastructure;
public class DynamoDBUpdate : IUpdateConferenceRepository {
    private string? Table = Environment.GetEnvironmentVariable("TABLE_NAME");
    public AmazonDynamoDBClient client = new AmazonDynamoDBClient();
    
    /// <summary>
    /// 会議室情報 登録(DBアクセス)
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<int> Register(RegisterMeetingRequestModel model){
        try
        {
            if(Table == null) {
                Console.WriteLine("Environment variable of table name is null at Register()");
                return CommonResult.InternalServerError;
            }

            Dictionary<string, List<WriteRequest>> requestItem = new();
            requestItem[Table] = CreateRegisterRequest(model);
            BatchWriteItemRequest request = 
                new BatchWriteItemRequest { RequestItems = requestItem };
            BatchWriteItemResponse result;
            do
            {
                result = await client.BatchWriteItemAsync(request);
                request.RequestItems = result.UnprocessedItems;
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
            if(Table == null) {
                Console.WriteLine("Environment variable of table name is null at Update()");
                return CommonResult.InternalServerError;
            }

            model.Contents ??= string.Empty;
            model.Participants ??= new List<string>();
            var param = new UpdateItemRequest{
                TableName = Table,
                Key = new Dictionary<string, AttributeValue>(){
                {"date_room", new AttributeValue{ S = model.CreatePartitionKey() }},
                {"time", new AttributeValue{ S = model.CreateBeginsSortKey() }}},
                ExpressionAttributeNames = new Dictionary<string, string>(){
                    { "#v1", "title" },
                    { "#v2", "contents"},
                    { "#v3", "members"},
                    { "#v4", "email" }
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>(){
                    { ":title", new AttributeValue(){ S = model.Title }},
                    { ":contents", new AttributeValue(){ S = model.Contents }},
                    { ":members", new AttributeValue(){ SS = model.Participants }},
                    { ":requested_by", new AttributeValue(){ S = model.Email }}
                },
                UpdateExpression = "SET #v1 = :title, #v2 = :contents, #v3 = :members",
                ConditionExpression = "#v4 = :requested_by"
            };
            var res = await client.UpdateItemAsync(param);
            
            return (int)res.HttpStatusCode;
        }
        catch(Exception e)
        {
            Console.WriteLine($"Failed to update dynamodb at Update(), " + e);
            return CommonResult.InternalServerError;
        }
    }

    public List<WriteRequest> CreateRegisterRequest(RegisterMeetingRequestModel model){
        List<WriteRequest> requests = new();
        List<string> fillList = model.CreateFill();
        if(fillList.Count() == 0) return requests;
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

        return requests;
    }
}
