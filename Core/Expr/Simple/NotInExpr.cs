using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.Simple
{
    public class NotInExpr : InExpr
    {
        protected override bool GetResult(object data)
        {
            return !base.GetResult(data);
        }

        public override string ToStr()
        {
            string s = Operand1.ToStr() + " not in " + Operand2.ToStr();
            return s;
        }
        public override string ToSql(ExpressionSqlBuilder builder)
        {
            string s = Operand1.ToSql(builder) + " not in " + Operand2.ToSql(builder);
            return s;
        }
    }
}
