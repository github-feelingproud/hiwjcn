using Lib.extension;
using Lib.helper;
using System;
using System.Collections.Generic;

namespace Hiwjcn.Service.Epc.InputsType
{
    /// <summary>
    /// 选项类型
    /// </summary>
    [Serializable]
    public class SelectInputExpression : InputExpression<string[]>, InputExpressionValidateable
    {

        public virtual List<string> EqualTips { get; set; }

        public virtual List<string> AnyEqualTips { get; set; }

        public virtual List<string> NotEqualTips { get; set; }

        /// <summary>
        /// 是不是多选
        /// </summary>
        public virtual bool Multi { get; set; }

        public bool PrepareAndValid(out string msg)
        {
            msg = string.Empty;

            this.EqualTips = this.PrepareTips(this.EqualTips);
            this.NotEqualTips = this.PrepareTips(this.NotEqualTips);
            this.AnyEqualTips = this.PrepareTips(this.AnyEqualTips);
            this.Normal = this.PrepareTips(this.Normal).ToArray();

            if (!ValidateHelper.IsPlumpList(this.Normal))
            {
                msg = "正常数值没有设定";
                return false;
            }

            return true;
        }

        public override ValidResult ValidInputs(string[] value)
        {
            var data = new ValidResult();
            if (!ValidateHelper.IsPlumpList(this.Normal))
            {
                data.ValidErrors.Add("没有设置正常选项，无法验证输入");
                return data;
            }
            var list = ConvertHelper.NotNullList(value);
            if (list.AllEqual(this.Normal))
            {
                data.Tips.AddWhenNotEmpty(this.EqualTips ?? new List<string>());
            }
            else
            {
                if (list.AnyEqual(this.Normal))
                {
                    data.Tips.AddWhenNotEmpty(this.AnyEqualTips ?? new List<string>());
                }
                else
                {
                    data.Tips.AddWhenNotEmpty(this.NotEqualTips ?? new List<string>());
                }
            }

            return data;
        }
    }
}
