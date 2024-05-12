using Moq;
using MeetingApp.Models;
using System.Text.Json;
using System.Net;
using Amazon.Lambda.SQSEvents;
using Amazon.Lambda.TestUtilities;
using Amazon.SQS;
using Amazon.SQS.Model;


namespace MeetingApp.Tests.Infrastructure;

public class DeleteMeetingSQSReceiveTest
{
    private Mock<IAmazonSQS> _sqsMock;
    private Mock<IDeleteConferenceRepository> _deleteMock;
    private DeleteMeetingSQSReceive _func;
    private int _countDelete;
    private int _countDLQSend;
    private SQSEvent _req;
    private TestLambdaContext _context;
    private DeleteMeetingRequestModel _model;

    public DeleteMeetingSQSReceiveTest()
    {
        _sqsMock = new Mock<IAmazonSQS>();
        _deleteMock = new Mock<IDeleteConferenceRepository>();
        _func = new DeleteMeetingSQSReceive(_deleteMock.Object, _sqsMock.Object);
        _req = new SQSEvent();
        _context = new TestLambdaContext();
        _model = new();
        _model.Email = "aaa@gmail.com";
        _model.Room = "RoomA";
        _model.Date = "20240501";
        _model.StartAt = "1200";
        _model.EndAt = "1400";
        _countDelete = 0;
        _countDLQSend = 0;

        var r = new SendMessageResponse(){
            HttpStatusCode = HttpStatusCode.OK  
        };
        _deleteMock.Setup(x => x.Delete(It.IsAny<DeleteMeetingRequestModel>()))
            .Callback(() => {
                _countDelete++;
            }).ReturnsAsync(CommonResult.OK);
        _sqsMock.Setup(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), default))
            .Callback(() => {
                _countDLQSend++;
            }).ReturnsAsync(r);

        SQSEvent.SQSMessage a = new();
        a.Body = JsonSerializer.Serialize(_model);
        _req.Records = new(){a, a, a};
    }

    [Fact]
    public async Task 正常終了時のテスト()
    {
        await _func.DeleteMeetingSQSReceiveHandler(_req, _context);
        //Deleteが3回呼ばれること
        Assert.Equal(3, _countDelete);
        //DLQへのSendMessageが呼ばれないこと
        Assert.Equal(0, _countDLQSend);
    }

    [Fact]
    public async Task バリデーションエラーの際DLQへのMessageSendが実行されること()
    {
        SQSEvent.SQSMessage a = new();
        _model.Date = "111";
        a.Body = JsonSerializer.Serialize(_model);
        _req.Records = new(){a, a, a};

        await _func.DeleteMeetingSQSReceiveHandler(_req, _context);
        //Deleteが呼ばれないこと
        Assert.Equal(0, _countDelete);
        //DLQへのSendMessageが3回呼ばれること
        Assert.Equal(3, _countDLQSend);
    }

    [Fact]
    public async Task Delete失敗時DLQへのMessageSendが実行されること()
    {
        _deleteMock.Setup(x => x.Delete(It.IsAny<DeleteMeetingRequestModel>()))
            .Callback(() => {
                _countDelete++;
            }).ReturnsAsync(500);

        await _func.DeleteMeetingSQSReceiveHandler(_req, _context);
        //Deleteが3回呼ばれること
        Assert.Equal(3, _countDelete);
        //DLQへのSendMessageが3回呼ばれること
        Assert.Equal(3, _countDLQSend);
    }

    [Fact(Skip = "Exceptionを外部から発生させることが不可能なためスキップ")]
    public void Exception発生のテスト()
    {
    }
}
