using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPC.Core.Entity;
using Hiwjcn.Core.Domain.User;

namespace EPC.Core.Model
{
    public class IssueGroupBy
    {
        public int Year { get; set; }

        public int Month { get; set; }

        public int Day { get; set; }

        public int IsClosed { get; set; }

        public string UserUID { get; set; }

        public string DeviceUID { get; set; }

        public int Count { get; set; }

        public int AvgSecondsToTakeToClose { get; set; }

        public DeviceEntity DeviceModel { get; set; }
    }

    public class CheckLogGroupBy
    {
        public string DeviceUID { get; set; }

        public string UserUID { get; set; }

        public UserEntity UserModel { get; set; }

        public string UserName { get => this.UserModel?.NickName; }

        public DateTime LastCheckTime { get; set; }

        public int Count { get; set; }
    }

    public class LogTimeGroupByDevice
    {
        public string DeviceUID { get; set; }

        public DateTime LogTime { get; set; }
    }

}
