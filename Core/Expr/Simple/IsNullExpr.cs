using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.Simple
{
    public class IsNullExpr : IsNotNullExpr
    {
        private bool GetResult(object data)
        {
            return Operand.GetNullResultOut(data);
        }

        public override string ToStr()
        {
            return Operand.ToStr() + " is null";
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            return Operand.ToSql(builder) + " is null";
        }
    }
}
