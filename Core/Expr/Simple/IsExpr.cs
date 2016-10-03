using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.Simple
{
    public class IsExpr : Custom_TwoOperand
    {
        public override int Priority()
        {
            return PriorityConst.Is;
        }

        public override void Prepare()
        {
            base.Prepare();
            if (!(Operand2 is NullConstExpr)) throw new Exception("allowed only the expression \"is null\"");
            GetBoolResultOut = GetResult;
            SetResultType(SimpleTypes.Boolean);
        }

        private bool GetResult(object data)
        {
            return Operand1.GetNullResultOut(data);
        }

        public override string ToStr()
        {
            return Operand1.ToStr() + " is null";
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            return Operand1.ToSql(builder) + " is " + Operand2.ToSql(builder);
        }
    }
}
