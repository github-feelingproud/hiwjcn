using Model.User;

namespace Hiwjcn.Web.Models.User
{
    public class MeViewModel : ViewModelBase
    {
        public virtual bool IsMe { get; set; }

        public virtual UserModel User { get; set; }
    }
}