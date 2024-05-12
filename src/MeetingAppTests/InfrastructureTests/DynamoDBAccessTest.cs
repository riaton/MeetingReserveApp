using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Moq;
using MeetingApp.Infrastructure;
using MeetingApp.Models;

namespace MeetingApp.Tests.Infrastructure;

public class DynamoDBAccessTest
{
    private Mock<IAmazonDynamoDB> _mock1;
    private Mock<IDynamoDBContext> _mock2;

    public DynamoDBAccessTest()
    {
        _mock1 = new Mock<IAmazonDynamoDB>();
        _mock2 = new Mock<IDynamoDBContext>();
    }

    [Fact]
    public async Task 環境変数が設定されていない時にNullが返却されること()
    {
        DynamoDBAccess access = new DynamoDBAccess(_mock1.Object, _mock2.Object);

        var result1 = await access.GetAll("partitionKey", new[]{"sortKey"});
        Assert.Null(result1);
        var result2 = await access.GetOne("partitionKey", "sortKey");
        Assert.Null(result2);
    }

    [Fact]
    public async Task GetOne関数のクエリ実行時に取得件数が0の場合_または_取得結果がNullの場合Nullが返却されること(){
        DynamoDBAccess access = new DynamoDBAccess(_mock1.Object, _mock2.Object, "table");
        var result = await access.GetOne("partitionKey", "sortKey");
        Assert.Null(result);

        _mock1.Setup(x => x.GetItemAsync(access.request, default)).ReturnsAsync(new GetItemResponse());
        result = await access.GetOne("partitionKey", "sortKey");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetOne関数のクエリ実行時にデータが取得できた場合そのデータが戻り値として返却されること(){
        DynamoDBAccess access = new DynamoDBAccess(_mock1.Object, _mock2.Object, "table");
        var response = new GetItemResponse();
        var att = new AttributeValue{S = "bbb"};
        response.Item.Add("aaa", att);

        _mock1.Setup(x => x.GetItemAsync(access.request, default)).ReturnsAsync(response);
        var result = await access.GetOne("partitionKey", "sortKey");
        
        Assert.NotNull(result);
        Assert.Equal(att, result.Item["aaa"]);
    }

    [Fact]
    public void GetOneリクエスト生成メソッドのテスト(){
        DynamoDBAccess access = new DynamoDBAccess(_mock1.Object, _mock2.Object, "table");
        Assert.Equal(0, access.request.Key.Count);
        Assert.Null(access.request.TableName);

        access.CreateGetOneQuery("aaa", "bbb");

        Assert.Equal("aaa", access.request.Key["date_room"].S);
        Assert.Equal("bbb", access.request.Key["time"].S);
        Assert.Equal("table", access.request.TableName);
    }

    [Fact]
    public async Task GetAll関数のクエリ実行時に取得件数が0の場合Nullが返却されること(){
        DynamoDBAccess access = new DynamoDBAccess(_mock1.Object, _mock2.Object, "table");

        _mock2.Setup(x => 
            x.QueryAsync<DynamoDBMeetingsTableItem>("partitionKey", QueryOperator.BeginsWith, 
            new[]{"sortKey"}, default)).Returns(new GetAllHelper(0));
        var result = await access.GetAll("partitionKey", new[]{"sortKey"});
        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetAll関数のクエリ実行時にデータが取得できた場合そのデータが戻り値として返却されること(){
        DynamoDBAccess access = new DynamoDBAccess(_mock1.Object, _mock2.Object, "table");

        _mock2.Setup(x => 
            x.QueryAsync<DynamoDBMeetingsTableItem>("partitionKey", QueryOperator.BeginsWith, 
            new[]{"sortKey"}, default)).Returns(new GetAllHelper(1));
        var result = await access.GetAll("partitionKey", new[]{"sortKey"});

        Assert.NotNull(result);
        Assert.Equal(1, result.Count);
        Assert.Contains(new DynamoDBMeetingsTableItem(), result);
    }

    [Fact]
    public void 正しいインターフェースが継承されていること(){
        DynamoDBAccess access = new DynamoDBAccess(_mock1.Object, _mock2.Object);

        Assert.IsAssignableFrom<IGetConferenceRepository>(access);
    }
}

public class GetAllHelper : AsyncSearch<DynamoDBMeetingsTableItem>{
    private int _option;
    public GetAllHelper(int option)
    {
        _option = option;
    }
    public override Task<List<DynamoDBMeetingsTableItem>> GetRemainingAsync(CancellationToken cancellationToken = default)
    {
        switch(_option){
            case 0:
                return Task.FromResult(new List<DynamoDBMeetingsTableItem>());
            default:
                var result = new List<DynamoDBMeetingsTableItem>();
                result.Add(new DynamoDBMeetingsTableItem());
                return Task.FromResult(result);
        }
    }
}