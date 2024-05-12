using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Moq;
using MeetingApp.Infrastructure;
using MeetingApp.Models;
using System.Net;

namespace MeetingApp.Tests.Infrastructure;
//Todo: Register()のテスト
public class DynamoDBUpdateTest
{
    private Mock<IAmazonDynamoDB> _mock;
    private UpdateMeetingRequestModel _updateModel;
    private RegisterMeetingRequestModel _registerModel;

    public DynamoDBUpdateTest()
    {
        _mock = new Mock<IAmazonDynamoDB>();
        _updateModel = new UpdateMeetingRequestModel();
        _registerModel = new RegisterMeetingRequestModel();
    }

    [Fact]
    public async Task 環境変数が設定されていない時に500エラーが返却されること()
    {
        DynamoDBUpdate update = new DynamoDBUpdate(_mock.Object);

        var result = await update.Update(_updateModel);
        Assert.Equal(CommonResult.InternalServerError, result);

        result = await update.Register(_registerModel);
        Assert.Equal(CommonResult.InternalServerError, result);
    }

    [Fact]
    public async Task Update関数のクエリ実行が成功した場合実行結果のステータスコードが返却されること(){
        DynamoDBUpdate update = new DynamoDBUpdate(_mock.Object, "table");
        var response = new UpdateItemResponse{
            HttpStatusCode = HttpStatusCode.OK
        };

        _mock.Setup(x => x.UpdateItemAsync(update.updateRequest, default)).ReturnsAsync(response);
        var result = await update.Update(_updateModel);
        
        Assert.Equal((int)HttpStatusCode.OK, result);
    }

    [Fact]
    public async Task Update関数で例外が発生した時500エラーが返却されること(){
        DynamoDBUpdate update = new DynamoDBUpdate(_mock.Object, "table");

        var result = await update.Update(_updateModel);
        
        Assert.Equal(CommonResult.InternalServerError, result);
    }

    [Fact]
    public void Updateリクエスト生成メソッドのテスト(){
        DynamoDBUpdate update = new DynamoDBUpdate(_mock.Object, "table");

        UpdateMeetingRequestModel model = new();
        model.Date = "20240501";
        model.Email = "aaa@gmail.com";
        model.StartAt = "1200";
        model.Room = "RoomA";
        model.Title = "title";
        model.Contents = "the contents";
        model.Participants = new List<string>{"aaa", "bbb"};
        update.CreateUpdateQuery(model);

        Assert.Equal("table", update.updateRequest.TableName);
        Assert.Equal("20240501_RoomA", update.updateRequest.Key["date_room"].S);
        Assert.Equal("begins@1200", update.updateRequest.Key["time"].S);
        Assert.Equal("title", update.updateRequest.ExpressionAttributeValues[":title"].S);
        Assert.Equal("the contents", update.updateRequest.ExpressionAttributeValues[":contents"].S);
        Assert.Equal(model.Participants, update.updateRequest.ExpressionAttributeValues[":members"].SS);
        Assert.Equal("aaa@gmail.com", update.updateRequest.ExpressionAttributeValues[":requested_by"].S);
        Assert.Equal("SET #v1 = :title, #v2 = :contents, #v3 = :members", update.updateRequest.UpdateExpression);
        Assert.Equal("#v4 = :requested_by", update.updateRequest.ConditionExpression);
    }

    [Fact]
    public void 正しいインターフェースが継承されていること(){
        DynamoDBUpdate update = new DynamoDBUpdate(_mock.Object);

        Assert.IsAssignableFrom<IUpdateConferenceRepository>(update);
    }
}