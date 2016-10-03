using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.Simple
{
    public class NullConstExpr : ConstExpr
    {
        public NullConstExpr()
            : base()
        {
            Init("", SimpleTypes.String);
        }

        public override bool GetNullResultOut(object data)
        {
            return true;
        }

        public override string ToStr()
        {
            return "null";
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            return "null";
        }
    }
}
