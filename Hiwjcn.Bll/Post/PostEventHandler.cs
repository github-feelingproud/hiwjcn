using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Post;
using Lib.events;

namespace Hiwjcn.Bll.Post
{
    public class PostEventHandler :
        IConsumer<EntityDeleted<PostModel>>,
        IConsumer<string>
    {
        public void HandleEvent(string eventMessage)
        {
            Console.WriteLine(eventMessage);
        }

        public void HandleEvent(EntityDeleted<PostModel> eventMessage)
        {
            //
        }
    }
}
