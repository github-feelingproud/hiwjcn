using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.events;
using Model.User;
using Lib.helper;
using Lib.extension;

namespace Hiwjcn.Bll.User
{
    public class UserEventHandler :
        IConsumer<EntityDeleted<UserModel>>,
        IConsumer<EntityInserted<UserModel>>,
        IConsumer<EntityUpdated<UserModel>>,
        IConsumer<string>
    {
        public void HandleEvent(EntityInserted<UserModel> eventMessage)
        {
            var json = eventMessage.Entity.ToJson();
            $"{json}".AddInfoLog("添加");
        }

        public void HandleEvent(string eventMessage)
        {
            ConvertHelper.GetString(eventMessage).AddBusinessInfoLog();
        }

        public void HandleEvent(EntityUpdated<UserModel> eventMessage)
        {
            var json = eventMessage.Entity.ToJson();
            $"{json}".AddInfoLog("更新");
        }

        public void HandleEvent(EntityDeleted<UserModel> eventMessage)
        {
            var json = eventMessage.Entity.ToJson();
            $"{json}".AddInfoLog("删除");
        }
    }
}
