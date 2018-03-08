using System;
using System.Collections.Generic;
using Lib.extension;

namespace Hiwjcn.Service.Epc.InputsType
{
    /// <summary>
    /// 布尔类型
    /// </summary>
    [Serializable]
    public class BoolInputExpression : InputExpression<bool>, InputExpressionValidateable
    {
        public virtual List<string> EqualTips { get; set; }

        public virtual List<string> NotEqualTips { get; set; }

        public bool PrepareAndValid(out string msg)
        {
            msg = string.Empty;

            this.EqualTips = this.PrepareTips(this.EqualTips);
            this.NotEqualTips = this.PrepareTips(this.NotEqualTips);

            return true;
        }

        public override ValidResult ValidInputs(bool value)
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
