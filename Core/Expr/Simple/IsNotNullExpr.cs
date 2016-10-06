using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.Simple
{
    public class IsNotNullExpr : Custom_OneOperand
    {
        public override int Priority()
        {
            return PriorityConst.Is;
        }
        
        public override bool IsLeftOperand() { return true; }

        public override void Prepare()
        {
            base.Prepare();
            GetBoolResultOut = GetResult;
            SetResultType(SimpleTypes.Boolean);
        }

        private bool GetResult(object data)
        {
            return !Operand.GetNullResultOut(data);
        }

        public override string ToStr()
        {
            return Operand.ToStr() + " is not null";
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            return Operand.ToSql(builder) + " is not null";
        }
    }
}
