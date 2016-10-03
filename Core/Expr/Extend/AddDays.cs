using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore;

namespace ParserCore.Expr.Extend
{
    public class AddDays : AddMinutes
    {
        public override DateTime GetResultDate(object data)
        {
            return Operand1.GetDateTimeResultOut(data).AddDays(Operand2.GetFloatResultOut(data));
        }

        public override TimeSpan GetResultTime(object data)
        {
            return Operand1.GetTimeResultOut(data).Add(TimeSpan.FromDays(Operand2.GetFloatResultOut(data)));
        }

        public override string ToStr() { return "AddDays(" + Operand1.ToStr() + "," + Operand2.ToStr() + ")"; }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            if (builder.DbType == DriverType.SqlServer)
            {
                return "DATEADD(day," + Operand2.ToSql(builder) + "," + Operand1.ToSql(builder) + ")";
            }
            if (builder.DbType == DriverType.PostgreSQL)
            {
                return "(" + Operand1.ToSql(builder) + "+" + Operand2.ToSql(builder) + " * interval '1 day')";
            }
            return ToSqlException();
        }
    }
}
