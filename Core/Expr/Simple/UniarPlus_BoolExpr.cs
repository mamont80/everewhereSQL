using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.Simple
{
    public class UniarPlus_BoolExpr : UniarMinus_BoolExpr
    {
        protected override long DoMinusInt(object data) { return Operand.GetIntResultOut(data); }
        protected override double DoMinusFloat(object data) { return Operand.GetFloatResultOut(data); }
        public override string ToStr() { return "+" + Operand.ToStr(); }
        public override string ToSql(ExpressionSqlBuilder builder) { return " +(" + Operand.ToSql(builder) + ")"; }
    }
}
