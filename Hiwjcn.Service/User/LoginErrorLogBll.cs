using Hiwjcn.Core.Infrastructure.User;
using Lib.helper;
using Lib.infrastructure;
using System;
using WebLogic.Dal.User;
using WebLogic.Model.User;

namespace WebLogic.Bll.User
{
    public class LoginErrorLogBll : ServiceBase<LoginErrorLogModel>, ILoginLogService
    {
        private LoginErrorLogDal _LoginErrorLogDal { get; set; }

        public LoginErrorLogBll()
        {
            this._LoginErrorLogDal = new LoginErrorLogDal();
        }

        public override string CheckModel(LoginErrorLogModel model)
        {
            if (model == null) { return "model对象为空"; }
            if (!ValidateHelper.IsAllPlumpString(model.LoginKey))
            {
                return "登录名为空";
            }
            return string.Empty;
        }

        /// <summary>
        /// 添加登录错误
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string AddLoginErrorLog(LoginErrorLogModel model)
        {
            string check = this.CheckModel(model);
            if (ValidateHelper.IsPlumpString(check)) { return check; }
            return _LoginErrorLogDal.Add(model) > 0 ? SUCCESS : "添加登录错误记录失败";
        }

        /// <summary>
        /// 获取登录错误，并清除旧数据(时间比较使用秒)
        /// </summary>
        /// <param name="LoginKey"></param>
        /// <param name="ExpireTime"></param>
        /// <returns></returns>
        public int GetRecentLoginErrorTimes(string LoginKey)
        {
            if (!ValidateHelper.IsPlumpString(LoginKey)) { throw new Exception("loginkey为空"); }
            var end = DateTime.Now;
            var start = end.AddMinutes(-10);

            int count = _LoginErrorLogDal.GetCount(x => x.LoginKey == LoginKey && x.CreateTime >= start && x.CreateTime <= end);

            {
                //清理更早的数据
                var list = _LoginErrorLogDal.GetList(x => x.LoginKey == LoginKey && x.CreateTime < start);
                if (ValidateHelper.IsPlumpList(list))
                {
                    _LoginErrorLogDal.Delete(list.ToArray());
                }
            }
            return count;
        }

    }
}
