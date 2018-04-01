using Lib.extension;
using Lib.helper;
using System;
using System.Collections.Generic;

namespace Hiwjcn.Service.Epc.InputsType
{
    /// <summary>
    /// 数值类型参数
    /// </summary>
    [Serializable]
    public class NumberInputExpression : InputExpression<double>, InputExpressionValidateable
    {
        /// <summary>
        /// 误差率
        /// </summary>
        public virtual double[] Range { get; set; }

        public virtual string Unit { get; set; }

        public virtual List<string> LowerTips { get; set; }

        public virtual List<string> UpperTips { get; set; }

        public bool PrepareAndValid(out string msg)
        {
            msg = string.Empty;

            this.LowerTips = this.PrepareTips(this.LowerTips);
            this.UpperTips = this.PrepareTips(this.UpperTips);
            this.Unit = ConvertHelper.GetString(this.Unit);

            if (!ValidateHelper.IsPlumpList(this.Range) || this.Range.Length != 2)
            {
                msg = "误差范围参数错误";
                return false;
            }

            return true;
        }

        public override ValidResult ValidInputs(double value)
        {
            var data = new ValidResult();
            if (this.Range?.Length != 2)
            {
                data.ValidErrors.Add("设备没有设置误差范围，无法验证提交数据");
                return data;
            }

            if (value < this.Normal * (1.00 - (Range[0] / 100.00)))
            {
                data.Tips.AddWhenNotEmpty(this.LowerTips ?? new List<string>());
            }
            if (value > this.Normal * (1.00 + (Range[1] / 100.00)))
            {
                data.Tips.AddWhenNotEmpty(this.UpperTips ?? new List<string>());
            }

            return data;
        }
    }
}
