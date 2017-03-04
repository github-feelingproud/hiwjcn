using Hiwjcn.Core.Infrastructure.Common;
using Lib.helper;
using Lib.ioc;
using Lib.mvc.user;
using System;
using System.Linq;
using System.Web;

/// <summary>
/// FileManager 的摘要说明
/// </summary>
public class ListFileManager : Handler
{
    private int Start;
    private int Size;
    private int Total;
    private string State;
    private String[] FileList;

    public ListFileManager(HttpContext context)
        : base(context)
    {
    }

    public override void Process()
    {
        var loginuser = AccountHelper.User.GetLoginUser(Context);
        if (loginuser == null)
        {
            State = "没有登陆";
            WriteResult();
            return;
        }
        Start = ConvertHelper.GetInt(Request["start"], -1);
        Size = ConvertHelper.GetInt(Request["size"], -1);
        if (Start < 0 || Size <= 0)
        {
            State = "参数错误";
            WriteResult();
            return;
        }
        try
        {
            var upfileservice = AppContext.GetObject<IUpFileService>();
            upfileservice.FindFiles(loginuser.IID, Start, Size, ref FileList, ref Total);
            State = "SUCCESS";
        }
        catch (Exception e)
        {
            State = e.Message;
        }
        WriteResult();
    }

    private void WriteResult()
    {
        WriteJson(new
        {
            state = State,
            list = FileList == null ? null : FileList.Select(x => new { url = x }),
            start = Start,
            size = Size,
            total = Total
        });
    }
}