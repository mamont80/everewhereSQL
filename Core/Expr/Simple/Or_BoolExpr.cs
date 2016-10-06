using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.Simple
{
    /// <summary>
    /// Опреация логического OR
    /// </summary>
    public class Or_BoolExpr : Custom_BoolExpr
    {
        protected override bool AsBool(object data) { return Operand1.GetBoolResultOut(data) || Operand2.GetBoolResultOut(data); }
        public override string ToStr() { return Operand1.ToStr() + " or " + Operand2.ToStr(); }
        public override string ToSql(ExpressionSqlBuilder builder) { return "(" + Operand1.ToSql(builder) + " or " + Operand2.ToSql(builder) + ")"; }
        public override int Priority() { return PriorityConst.Or; }
    }
}
