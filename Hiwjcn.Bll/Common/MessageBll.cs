using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Threading.Tasks;
using Lib;
using Model;
using Model.Sys;
using Lib.core;
using Lib.helper;
using Lib.http;
using Dal.Sys;
using Model.User;
using Lib.infrastructure;

namespace Bll.Sys
{
    /// <summary>
    /// 站内信
    /// </summary>
    public class MessageBll : ServiceBase<MessageModel>
    {
        public MessageBll()
        {
            //
        }

        public override string CheckModel(MessageModel model)
        {
            if (model == null) { return "消息对象为空"; }
            if (!ValidateHelper.IsPlumpString(model.MsgContent))
            {
                return "消息内容为空";
            }
            if (model.SenderUserID == model.ReceiverUserID)
            {
                return "无法给自己发送消息";
            }
            return string.Empty;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string SendMessage(MessageModel model)
        {
            string err = CheckModel(model);
            if (ValidateHelper.IsPlumpString(err)) { return err; }
            var dal = new MessageDal();
            return dal.Add(model) > 0 ? SUCCESS : "发送失败";
        }

        /// <summary>
        /// 获取最新的站内信
        /// </summary>
        /// <param name="receiverID"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<MessageModel> GetTopMessage(int receiverID, int count = 100)
        {
            string key = "topmessage,uid:" + receiverID + "count:" + count;
            return Cache(key, () =>
            {
                var mdal = new MessageDal();
                var list = mdal.QueryList(where: x => x.ReceiverUserID == receiverID, 
                    orderby: x => x.UpdateTime, Desc: true, start: 0, count: count);
                return list;
            });
        }

        /// <summary>
        /// 获取站内信数量
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        public int GetSenderMessageCount(int user_id, DateTime start, DateTime end)
        {
            string key = Com.GetCacheKey("messagecount", user_id.ToString(), start.ToString(), end.ToString());
            return Cache(key, () =>
            {
                var pdal = new MessageDal();
                return pdal.GetCount(x => x.SenderUserID == user_id
                    && x.UpdateTime >= start && x.UpdateTime < end);
            });
        }

    }
}
