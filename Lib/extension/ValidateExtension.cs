using Lib.helper;

namespace Lib.extension
{
    public static class ValidateExtension
    {
        /// <summary>
        /// 是手机号
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsMobilePhone(this string data)
        {
            return ValidateHelper.IsMobilePhone(data);
        }

        /// <summary>
        /// 是否是域名
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsDomain(this string data)
        {
            return ValidateHelper.IsDomain(data);
        }

        /// <summary>
        /// 是否是中文
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsChinese(this string data)
        {
            return ValidateHelper.IsChinese(data);
        }

        /// <summary>
        /// 判断是否是中文字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsChineaseStr(this string data)
        {
            return ValidateHelper.IsChineaseStr(data);
        }

        /// <summary>
        /// 是否是身份证号
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsIDCardNo(this string data)
        {
            return ValidateHelper.IsIDCardNo(data);
        }

        /// <summary>
        /// 是否是IP
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsIP(this string data)
        {
            return ValidateHelper.IsIP(data);
        }

        /// <summary>
        /// 数字或者字母
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNUMBER_OR_CHAR(this string data)
        {
            return ValidateHelper.IsNUMBER_OR_CHAR(data);
        }

        /// <summary>
        /// 是否是数字
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNumber(this string data)
        {
            return ValidateHelper.IsNumber(data);
        }

        /// <summary>
        /// 是否是邮箱
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsEmail(this string data)
        {
            return ValidateHelper.IsEmail(data);
        }
        /// <summary>
        /// 是否是电话
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsPhone(this string data)
        {
            return ValidateHelper.IsPhone(data);
        }
        /// <summary>
        /// 是否是URL
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsURL(this string data)
        {
            return ValidateHelper.IsURL(data);
        }
        /// <summary>
        /// 是否是日期
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsDate(this string data)
        {
            return ValidateHelper.IsDate(data);
        }
        /// <summary>
        /// 是否是时间
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsTime(this string data)
        {
            return ValidateHelper.IsTime(data);
        }
        /// <summary>
        /// 是否是float
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsFloat(this string data)
        {
            return ValidateHelper.IsFloat(data);
        }
        /// <summary>
        /// 是否是int
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsInt(this string data)
        {
            return ValidateHelper.IsInt(data);
        }
        /// <summary>
        /// 长度是否在范围内
        /// </summary>
        /// <param name="data"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool LenInRange(this string data, int min, int max)
        {
            return ValidateHelper.IsLenInRange(data, min, max);
        }
    }
}
