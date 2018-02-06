using Lib.extension;
using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hiwjcn.Service.Epc.InputsType
{
    public class ValidResult
    {
        public ValidResult()
        {
            this.Tips = new List<string>();
            this.ValidErrors = new List<string>();
        }

        public virtual bool OK { get => !ValidateHelper.IsPlumpList(this.Tips); }

        public virtual List<string> Tips { get; set; }

        public virtual List<string> ValidErrors { get; set; }

        public virtual void ThrowIfException()
        { }
    }

    public interface InputExpressionValidateable
    {
        /// <summary>
        /// 验证这个表达式是否正常
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool PrepareAndValid(out string msg);
    }

    [Serializable]
    public abstract class InputExpression<InputType>
    {
        /// <summary>
        /// 正常值
        /// </summary>
        public virtual InputType Normal { get; set; }

        public abstract ValidResult ValidInputs(InputType value);

        protected List<string> PrepareTips(IEnumerable<string> list) =>
            ConvertHelper.NotNullList(list).NotEmptyAndDistinct(x => x).ToList();
    }
}
