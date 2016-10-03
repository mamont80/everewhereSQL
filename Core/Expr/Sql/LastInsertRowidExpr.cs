using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore.Expr.Simple;

namespace ParserCore.Expr.Sql
{
    public class LastInsertRowidExpr : FuncExpr_WithoutOperand
    {
        public override void Prepare()
        {
            base.Prepare();
            SetResultType(SimpleTypes.Integer);
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            if (builder.DbType == DriverType.SqlServer)
            {
                return "SCOPE_IDENTITY()";
            }
            if (builder.DbType == DriverType.PostgreSQL)
            {
                return "lastval()";
            }
            return ToSqlException();
        }
        public override string ToStr() { return "LastInsertRowid()"; }
    }
}
