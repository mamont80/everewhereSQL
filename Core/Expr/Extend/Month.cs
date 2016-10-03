using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore;
using ParserCore.Expr.Simple;

namespace ParserCore.Expr.Extend
{
    public class Month : FuncExpr_OneOperand
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
            return dt.Month;
        }
        public override string ToStr()
        {
            return "Month(" + Operand.ToStr() + ")";
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            if (builder.DbType == DriverType.SqlServer)
            {
                return "(MONTH(" + Operand.ToSql(builder) + "))";
            }
            if (builder.DbType == DriverType.PostgreSQL)
            {
                return "(date_part('month'," + Operand.ToSql(builder) + "))";
            }
            return ToSqlException();

        }
    }
}
