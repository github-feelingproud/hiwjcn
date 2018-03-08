using Lib.extension;
using Lib.helper;
using System;
using System.Collections.Generic;

namespace Hiwjcn.Service.Epc.InputsType
{
    /// <summary>
    /// 字符串类型
    /// </summary>
    [Serializable]
    public class StringInputExpression : InputExpression<string>, InputExpressionValidateable
    {
        public virtual List<string> EqualTips { get; set; }

        public virtual List<string> NotEqualTips { get; set; }

        public bool PrepareAndValid(out string msg)
        {
            msg = string.Empty;

            this.EqualTips = this.PrepareTips(this.EqualTips);
            this.NotEqualTips = this.PrepareTips(this.NotEqualTips);

            if (!ValidateHelper.IsPlumpString(this.Normal))
            {
                msg = "正常数值没有设定";
                return false;
            }

            return true;
        }

        public override ValidResult ValidInputs(string value)
        {
            var data = new ValidResult();
            if (value == this.Normal)
            {
                data.Tips.AddWhenNotEmpty(this.EqualTips ?? new List<string>());
            }
            else
            {
                data.Tips.AddWhenNotEmpty(this.NotEqualTips ?? new List<string>());
            }

            return data;
        }
    }
}
