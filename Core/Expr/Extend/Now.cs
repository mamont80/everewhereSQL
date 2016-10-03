using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore;
using ParserCore.Expr.Simple;

namespace ParserCore.Expr.Extend
{
    public class Now : FuncExpr_WithoutOperand
    {
        public override void Prepare()
        {
            base.Prepare();
            SetResultType(SimpleTypes.DateTime);
            GetDateTimeResultOut = GetResult;
        }

        private DateTime GetResult(object data)
        {
            return DateTime.Now;
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            if (builder.DbType == DriverType.SqlServer)
            {
                return "GetDate()";
            }
            if (builder.DbType == DriverType.PostgreSQL)
            {
                return "Now()";
            }
            return ToSqlException();
        }
        public override string ToStr() { return "Now()"; }
    }

}
