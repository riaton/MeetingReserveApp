using Moq;
using MeetingApp.Models;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;


namespace MeetingApp.Tests.Infrastructure;

public class RegisterMeetingTest
{
    private Mock<IUpdateConferenceRepository> _mock;
    private RegisterMeeting _func;
    private APIGatewayProxyRequest _req;
    private TestLambdaContext _context;
    private RegisterMeetingRequestModel _model;
    private const string max50String = "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwx";

    public RegisterMeetingTest()
    {
        _mock = new Mock<IUpdateConferenceRepository>();
        _func = new RegisterMeeting(_mock.Object);
        _req = new APIGatewayProxyRequest();
        _context = new TestLambdaContext();
        _model = new RegisterMeetingRequestModel();
        _model.Email = "aaa@gmail.com";
        _model.Title = "aaa";
        _model.Room = "RoomA";
        _model.Date = "20240501";
        _model.StartAt = "1200";
        _model.EndAt = "1400";
    }

    [Fact]
    public async Task 正常終了時のテスト()
    {
        _req.Body = JsonSerializer.Serialize(_model);
        _mock.Setup(x => x.Register(It.IsAny<RegisterMeetingRequestModel>()))
            .ReturnsAsync(CommonResult.OK);

        var res = await _func.RegisterMeetingHandler(_req, _context);

        Assert.Equal(CommonResult.OK, res.StatusCode);
        Assert.Equal(CommonResult.ResponseHeader, res.Headers);
        Assert.Null(res.Body);
    }

    [Fact]
    public async Task バリデーションのテスト()
    {
        _mock.Setup(x => x.Register(It.IsAny<RegisterMeetingRequestModel>()))
            .ReturnsAsync(CommonResult.OK);
        //必須パラメータ(Email)なし
        _model.Email = null;
        _req.Body = JsonSerializer.Serialize(_model);
        var res = await _func.RegisterMeetingHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
        Assert.Equal(CommonResult.ResponseHeader, res.Headers);
        Assert.Null(res.Body);
        //必須パラメータ(Title)なし
        _model.Email = "aaa@gmail.com";
        _model.Title = null;
        _req.Body = JsonSerializer.Serialize(_model);
        res = await _func.RegisterMeetingHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
        //必須パラメータ(Room)なし
        _model.Title = "aaa";
        _model.Room = null;
        _req.Body = JsonSerializer.Serialize(_model);
        res = await _func.RegisterMeetingHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
        //必須パラメータ(Date)なし
        _model.Room = "RoomA";
        _model.Date = null;
        _req.Body = JsonSerializer.Serialize(_model);
        res = await _func.RegisterMeetingHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
        //必須パラメータ(StartAt)なし
        _model.Date = "20240501";
        _model.StartAt = null;
        _req.Body = JsonSerializer.Serialize(_model);
        res = await _func.RegisterMeetingHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
        //文字数50(Email)
        _model.StartAt = "1200";
        _model.Email = max50String;
        _req.Body = JsonSerializer.Serialize(_model);
        res = await _func.RegisterMeetingHandler(_req, _context);
        Assert.Equal(CommonResult.OK, res.StatusCode);
        //文字数51(Email)
        _model.Email = max50String + "y";
        _req.Body = JsonSerializer.Serialize(_model);
        res = await _func.RegisterMeetingHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
        //文字数50(Title)
        _model.Email = "aaa@gmail.com";
        _model.Title = max50String;
        _req.Body = JsonSerializer.Serialize(_model);
        res = await _func.RegisterMeetingHandler(_req, _context);
        Assert.Equal(CommonResult.OK, res.StatusCode);
        //文字数51(Title)
        _model.Title = max50String + "y";
        _req.Body = JsonSerializer.Serialize(_model);
        res = await _func.RegisterMeetingHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
        //文字数7(Date)
        _model.Title = "aaa";
        _model.Date = "2024050";
        _req.Body = JsonSerializer.Serialize(_model);
        res = await _func.RegisterMeetingHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
        //文字数9(Date)
        _model.Date = "202405010";
        _req.Body = JsonSerializer.Serialize(_model);
        res = await _func.RegisterMeetingHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
        //文字数3(StartAt)
        _model.Date = "20240501";
        _model.StartAt = "120";
        _req.Body = JsonSerializer.Serialize(_model);
        res = await _func.RegisterMeetingHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
        //文字数5(StartAt)
        _model.StartAt = "12000";
        _req.Body = JsonSerializer.Serialize(_model);
        res = await _func.RegisterMeetingHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
        //数字以外が含まれる(StartAt)
        _model.StartAt = "12a0";
        _req.Body = JsonSerializer.Serialize(_model);
        res = await _func.RegisterMeetingHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
        //数字以外が含まれる(Date)
        _model.StartAt = "1200";
        _model.Date = "202b0501";
        _req.Body = JsonSerializer.Serialize(_model);
        res = await _func.RegisterMeetingHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
        //必須パラメータ(EndAt)なし
        _model.Date = "20240501";
        _model.EndAt = null;
        _req.Body = JsonSerializer.Serialize(_model);
        res = await _func.RegisterMeetingHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
        //文字数3(EndAt)
        _model.EndAt = "202";
        _req.Body = JsonSerializer.Serialize(_model);
        res = await _func.RegisterMeetingHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
        //文字数5(EndAt)
        _model.EndAt = "20240";
        _req.Body = JsonSerializer.Serialize(_model);
        res = await _func.RegisterMeetingHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
        //数字以外が含まれる(EndAt)
        _model.EndAt = "20c4";
        _req.Body = JsonSerializer.Serialize(_model);
        res = await _func.RegisterMeetingHandler(_req, _context);
        Assert.Equal(CommonResult.ValidateError, res.StatusCode);
    }

    [Fact]
    public async Task DB更新失敗時InternalServerErrorが返却されること()
    {
        _req.Body = JsonSerializer.Serialize(_model);

        var res = await _func.RegisterMeetingHandler(_req, _context);

        Assert.Equal(CommonResult.InternalServerError, res.StatusCode);
        Assert.Equal(CommonResult.ResponseHeader, res.Headers);
        Assert.Null(res.Body);
    }

    [Fact(Skip = "Exceptionを外部から発生させることが不可能なためスキップ")]
    public void Exception発生時InternalServerErrorが返却されること()
    {
    }
}
