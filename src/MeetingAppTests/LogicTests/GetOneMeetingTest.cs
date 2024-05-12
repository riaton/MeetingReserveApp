using Amazon.DynamoDBv2.Model;
using Moq;
using MeetingApp.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;


namespace MeetingApp.Tests.Infrastructure;

public class GetOneMeetingest
{
    private Mock<IGetConferenceRepository> _mock;
    private GetOneMeeting _func;
    private APIGatewayProxyRequest _req;
    private TestLambdaContext _context;
    private readonly JsonSerializerOptions _options = new()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

    public GetOneMeetingest()
    {
        _mock = new Mock<IGetConferenceRepository>();
        _func = new GetOneMeeting(_mock.Object);
        _req = new APIGatewayProxyRequest();
        _context = new TestLambdaContext();
    }

    [Fact]
    public async Task 正常終了時のテスト()
    {
        var r = new GetItemResponse();
        r.Item.Add("end_at", new AttributeValue("1400"));
        r.Item.Add("time", new AttributeValue("begins@1230"));
        r.Item.Add("date_room", new AttributeValue("20240930_RoomA"));
        _mock.Setup(x => x.GetOne("20240930_RoomA", "begins@1230")).ReturnsAsync(r);
        _req.PathParameters = new Dictionary<string, string>(){
            { "StartAt", "202409301230" },
            { "Room", "RoomA" }
        };
        var body = JsonSerializer.Serialize(new GetOneResponseModel(r), _options);

        var res = await _func.GetOneMeetingHandler(_req, _context);

        Assert.Equal(CommonResult.OK, res.StatusCode);
        Assert.Equal(CommonResult.ResponseHeader, res.Headers);
        Assert.Equal(body, res.Body);
    }

    [Fact]
    public async Task バリデーションのテスト()
    {
        //必須パラメータ(Room)なし
        _req.PathParameters = new Dictionary<string, string>(){
            { "StartAt", "202409301230" }
        };
        var res = await _func.GetOneMeetingHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
        Assert.Equal(CommonResult.ResponseHeader, res.Headers);
        Assert.Equal(CommonResult.FromResult(400), res.Body);
        //必須パラメータ(StartAt)なし
        _req.PathParameters = new Dictionary<string, string>(){
            { "Room", "RoomA" }
        };
        res = await _func.GetOneMeetingHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
        //文字数超過(StartAt)
        _req.PathParameters = new Dictionary<string, string>(){
            { "StartAt", "2024093012300" },
            { "Room", "RoomA" }
        };
        res = await _func.GetOneMeetingHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
        //文字数足りない(StartAt)     
        _req.PathParameters = new Dictionary<string, string>(){
            { "StartAt", "20240930123" },
            { "Room", "RoomA" }
        };
        res = await _func.GetOneMeetingHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
        //数字以外が含まれる(StartAt) 
        _req.PathParameters = new Dictionary<string, string>(){
            { "StartAt", "20240930a230" },
            { "Room", "RoomA" }
        };
        res = await _func.GetOneMeetingHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
    }

    [Fact]
    public async Task DB取得なしの際DataNotFoundが返却されること()
    {
        _req.PathParameters = new Dictionary<string, string>(){
            { "StartAt", "202409301230" },
            { "Room", "RoomA" }
        };
        var res = await _func.GetOneMeetingHandler(_req, _context);

        Assert.Equal(CommonResult.DataNotFound, res.StatusCode);
        Assert.Equal(CommonResult.ResponseHeader, res.Headers);
        Assert.Equal(CommonResult.FromResult(404), res.Body);
    }

    [Fact]
    public async Task Exception発生時InternalServerErrorが返却されること()
    {
        var r = new GetItemResponse();
        r.Item.Add("end_at", new AttributeValue("1400"));
        r.Item.Add("time", new AttributeValue("begins@1230"));
        _mock.Setup(x => x.GetOne("20240930_RoomA", "begins@1230")).ReturnsAsync(r);
        _req.PathParameters = new Dictionary<string, string>(){
            { "StartAt", "202409301230" },
            { "Room", "RoomA" }
        };
        var res = await _func.GetOneMeetingHandler(_req, _context);

        Assert.Equal(CommonResult.InternalServerError, res.StatusCode);
        Assert.Equal(CommonResult.ResponseHeader, res.Headers);
        Assert.Equal(CommonResult.FromResult(500), res.Body);   
    }
}
