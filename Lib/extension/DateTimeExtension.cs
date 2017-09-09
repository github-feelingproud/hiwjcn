using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.extension
{
    public static class DateTimeExtension
    {
        /// <summary>
        /// 计算当月的总天数
        /// </summary>
        public static int DaysOfThisMonth(this DateTime time)
        {
            var start = time.Date;
            var end = start.AddMonths(1);
            var days = (end - start).TotalDays;
            return (int)Math.Ceiling(days);
        }

        /// <summary>
        /// 明天凌晨
        /// </summary>
        public static DateTime ToTomorrowMorning(this DateTime time)
        {
            return time.Date.AddDays(1);
        }

        /// <summary>
        /// 今天凌晨
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static DateTime ToToDayMorning(this DateTime time)
        {
            return time.Date;
        }

        /// <summary>
        /// 获取年龄
        /// </summary>
        /// <param name="birthday"></param>
        /// <returns></returns>
        public static int GetAge(this DateTime birthday)
        {
            var today = DateTime.Now;

            int age = today.Year - birthday.Year;

            if (birthday > today.AddYears(-age))
            {
                --age;
            }

            return age;
        }

        /// <summary>
        /// 获取格式化字符串，带时分秒，格式："yyyy-MM-dd HH:mm:ss"
        /// </summary>
        /// <param name="dateTime">日期</param>
        /// <param name="isRemoveSecond">是否移除秒</param>
        public static string ToDateTimeString(this DateTime dateTime, bool isRemoveSecond = false)
        {
            if (isRemoveSecond)
                return dateTime.ToString("yyyy-MM-dd HH:mm");
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 获取格式化字符串，不带时分秒，格式："yyyy-MM-dd"
        /// </summary>
        /// <param name="dateTime">日期</param>
        public static string ToDateString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// 获取格式化字符串，不带年月日，格式："HH:mm:ss"
        /// </summary>
        /// <param name="dateTime">日期</param>
        public static string ToTimeString(this DateTime dateTime)
        {
            return dateTime.ToString("HH:mm:ss");
        }

        /// <summary>
        /// 获取格式化字符串，带毫秒，格式："yyyy-MM-dd HH:mm:ss.fff"
        /// </summary>
        /// <param name="dateTime">日期</param>
        public static string ToMillisecondString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        /// <summary>
        /// 获取格式化字符串，不带时分秒，格式："yyyy年MM月dd日"
        /// </summary>
        /// <param name="dateTime">日期</param>
        public static string ToChineseDateString(this DateTime dateTime)
        {
            return string.Format("{0}年{1}月{2}日", dateTime.Year, dateTime.Month, dateTime.Day);
        }

        /// <summary>
        /// 转为n秒前，n分钟前 etc
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToFriendlyDateTime(this DateTime dateTime)
        {
            return DateTimeHelper.GetFriendlyDateTime(dateTime);
        }

        /// <summary>
        /// 转为n秒前，n分钟前 etc
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string GetSimpleFriendlyDateTime(this DateTime dateTime)
        {
            return DateTimeHelper.GetSimpleFriendlyDateTime(dateTime);
        }

        /// <summary>
        /// 获取一天的开始和第二天的开始
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static (DateTime start, DateTime end) GetDateBorder(this DateTime dateTime)
        {
            var date = dateTime.Date;
            return (date, date.AddDays(1));
        }

        /// <summary>
        /// 判断是否是同一天
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static bool IsSameDay(this DateTime dateTime, DateTime time)
        {
            var border = dateTime.GetDateBorder();
            return border.start <= time && time < border.end;
        }

        /// <summary>
        /// 是今天
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static bool IsToday(this DateTime time)
        {
            return time.IsSameDay(DateTime.Now);
        }

        /// <summary>
        /// 是昨天
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static bool IsYesterday(this DateTime time)
        {
            return time.IsSameDay(DateTime.Now.AddDays(-1));
        }

    }
}
