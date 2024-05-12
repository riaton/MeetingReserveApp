using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Moq;
using MeetingApp.Infrastructure;
using MeetingApp.Models;
using System.Net;

namespace MeetingApp.Tests.Infrastructure;

public class DynamoDBDeleteTest
{
    private Mock<IAmazonDynamoDB> _mock;
    private DeleteMeetingRequestModel _model;

    public DynamoDBDeleteTest()
    {
        _mock = new Mock<IAmazonDynamoDB>();
        _model = new DeleteMeetingRequestModel();
    }

    [Fact]
    public async Task 環境変数が設定されていない時に500エラーが返却されること()
    {
        DynamoDBDelete delete = new DynamoDBDelete(_mock.Object);

        var result = await delete.Delete(_model);
        Assert.Equal(CommonResult.InternalServerError, result);
    }

    [Fact]
    public async Task Delete関数のクエリ実行が成功した場合実行結果のステータスコードが返却されること(){
        DynamoDBDelete delete = new DynamoDBDelete(_mock.Object, "table");
        var response = new DeleteItemResponse{
            HttpStatusCode = HttpStatusCode.OK
        };

        _mock.Setup(x => x.DeleteItemAsync(delete.request, default)).ReturnsAsync(response);
        var result = await delete.Delete(_model);
        
        Assert.Equal((int)HttpStatusCode.OK, result);
    }

    [Fact]
    public async Task Delete関数で例外が発生した時500エラーが返却されること(){
        DynamoDBDelete delete = new DynamoDBDelete(_mock.Object, "table");

        var result = await delete.Delete(_model);
        
        Assert.Equal(CommonResult.InternalServerError, result);
    }
 
    [Fact]
    public void Deleteリクエスト生成メソッドのテスト(){
        DynamoDBDelete delete = new DynamoDBDelete(_mock.Object, "table");

        DeleteMeetingRequestModel model = new();
        model.Date = "20240501";
        model.Email = "aaa@gmail.com";
        model.EndAt = "1400";
        model.StartAt = "1200";
        model.Room = "RoomA";
        delete.CreateDeleteQuery(model);

        Assert.Equal("table", delete.request.TableName);
        Assert.Equal("20240501_RoomA", delete.request.Key["date_room"].S);
        Assert.Equal("begins@1200", delete.request.Key["time"].S);
        Assert.Equal("end_at", delete.request.ExpressionAttributeNames["#v"]);
        Assert.Equal("1400", delete.request.ExpressionAttributeValues[":end_at"].S);
        Assert.Equal("#v = :end_at", delete.request.ConditionExpression);
    }

    [Fact]
    public void 正しいインターフェースが継承されていること(){
        DynamoDBDelete delete = new DynamoDBDelete(_mock.Object);

        Assert.IsAssignableFrom<IDeleteConferenceRepository>(delete);
    }
}