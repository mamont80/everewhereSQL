using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.Simple
{
    public abstract class Custom_BoolExpr : Custom_TwoOperand
    {
        public override void Prepare()
        {
            base.Prepare();
            if (Operand1.GetResultType() != SimpleTypes.Boolean) this.TypesException();
            if (Operand2.GetResultType() != SimpleTypes.Boolean) this.TypesException();
            GetBoolResultOut = AsBool;
            SetResultType(SimpleTypes.Boolean);
        }
        protected abstract bool AsBool(object data);
    }
}
