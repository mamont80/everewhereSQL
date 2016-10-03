using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore;
using ParserCore.Expr.Simple;

namespace ParserCore.Expr.Extend
{
    public class Day : FuncExpr_OneOperand
    {
        public override void Prepare()
        {
            base.Prepare();
            var t = Operand.GetResultType();
            if (t != SimpleTypes.DateTime && t != SimpleTypes.Date) TypesException();
            SetResultType(SimpleTypes.Integer);
            GetIntResultOut = GetResult;
        }

        private long GetResult(object data)
        {
            DateTime dt = Operand.GetDateTimeResultOut(data);
            return dt.Day;
        }

        public override string ToStr()
        {
            return "Day(" + Operand.ToStr() + ")";
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            if (builder.DbType == DriverType.SqlServer)
            {
                return "(DAY(" + Operand.ToSql(builder) + "))";
            }
            if (builder.DbType == DriverType.PostgreSQL)
            {
                return "(date_part('day'," + Operand.ToSql(builder) + "))";
            }
            return ToSqlException();
        }
    }
}
