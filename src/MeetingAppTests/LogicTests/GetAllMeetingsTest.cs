using Moq;
using MeetingApp.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;


namespace MeetingApp.Tests.Infrastructure;

public class GetAllMeetingsTest
{
    private Mock<IGetConferenceRepository> _mock;
    private GetAllMeetings _func;
    private APIGatewayProxyRequest _req;
    private TestLambdaContext _context;
    private readonly JsonSerializerOptions _options = new()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

    public GetAllMeetingsTest()
    {
        _mock = new Mock<IGetConferenceRepository>();
        _func = new GetAllMeetings(_mock.Object);
        _req = new APIGatewayProxyRequest();
        _context = new TestLambdaContext();
    }

    [Fact]
    public async Task 正常終了時のテスト()
    {
        var table = new DynamoDBMeetingsTableItem();
        table.SortKey = "begins@1230";
        var r = new List<DynamoDBMeetingsTableItem>(){ table };
        _mock.Setup(x => x.GetAll("20240930_RoomA", new[]{ "begins@" })).ReturnsAsync(r);
        _req.PathParameters = new Dictionary<string, string>(){
            { "StartDate", "20240930" },
            { "Room", "RoomA" }
        };
        var resModel = new GetAllMeetingsResponseModel();
        resModel.Body.Add(new GetAllMeetingParts(table, "RoomA"));
        var body = JsonSerializer.Serialize(resModel, _options);

        var res = await _func.GetAllMeetingsHandler(_req, _context);

        Assert.Equal(CommonResult.OK, res.StatusCode);
        Assert.Equal(CommonResult.ResponseHeader, res.Headers);
        Assert.Equal(body, res.Body);
    }

    [Fact]
    public async Task バリデーションのテスト()
    {
        //必須パラメータ(Room)なし
        _req.PathParameters = new Dictionary<string, string>(){
            { "StartDate", "20240930" }
        };
        var res = await _func.GetAllMeetingsHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
        Assert.Equal(CommonResult.ResponseHeader, res.Headers);
        Assert.Equal(CommonResult.FromResult(400), res.Body);
        //必須パラメータ(StartDate)なし
        _req.PathParameters = new Dictionary<string, string>(){
            { "Room", "RoomA" }
        };
        res = await _func.GetAllMeetingsHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
        //文字数超過(StartDate)
        _req.PathParameters = new Dictionary<string, string>(){
            { "StartAt", "202409300" },
            { "Room", "RoomA" }
        };
        res = await _func.GetAllMeetingsHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
        //文字数足りない(StartDate)     
        _req.PathParameters = new Dictionary<string, string>(){
            { "StartAt", "2024093" },
            { "Room", "RoomA" }
        };
        res = await _func.GetAllMeetingsHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
        //数字以外が含まれる(StartDate) 
        _req.PathParameters = new Dictionary<string, string>(){
            { "StartAt", "2024v930" },
            { "Room", "RoomA" }
        };
        res = await _func.GetAllMeetingsHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
    }

    [Fact]
    public async Task DB取得なしの際DataNotFoundが返却されること()
    {
        _req.PathParameters = new Dictionary<string, string>(){
            { "StartDate", "20240930" },
            { "Room", "RoomA" }
        };
        var res = await _func.GetAllMeetingsHandler(_req, _context);

        Assert.Equal(CommonResult.DataNotFound, res.StatusCode);
        Assert.Equal(CommonResult.ResponseHeader, res.Headers);
        Assert.Equal(CommonResult.FromResult(404), res.Body);
    }

    [Fact]
    public async Task Exception発生時InternalServerErrorが返却されること()
    {
        var table = new DynamoDBMeetingsTableItem();
        table.SortKey = "1230";
        var r = new List<DynamoDBMeetingsTableItem>(){ table };
        _mock.Setup(x => x.GetAll("20240930_RoomA", new[]{ "begins@" })).ReturnsAsync(r);

        _req.PathParameters = new Dictionary<string, string>(){
            { "StartDate", "20240930" },
            { "Room", "RoomA" }
        };
        var res = await _func.GetAllMeetingsHandler(_req, _context);

        Assert.Equal(CommonResult.InternalServerError, res.StatusCode);
        Assert.Equal(CommonResult.ResponseHeader, res.Headers);
        Assert.Equal(CommonResult.FromResult(500), res.Body); 
    }
}
