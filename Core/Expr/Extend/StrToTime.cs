using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore;
using ParserCore.Expr.Simple;

namespace ParserCore.Expr.Extend
{
    public class StrToTime : FuncExpr_OneOperand
    {
        public override void Prepare()
        {
            base.Prepare();
            if (Operand.GetResultType() != SimpleTypes.String) TypesException();
            SetResultType(SimpleTypes.Time);
            GetTimeResultOut = GetResult;
        }

        private TimeSpan GetResult(object data)
        {
            string s = Operand.GetStrResultOut(data);
            DateTime dt;
            if (CommonUtils.ParseDateTime(s, out dt) != ParserDateTimeStatus.Time) throw new Exception("String value is not time type");
            return dt.TimeOfDay;
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            if (builder.DbType == DriverType.SqlServer)
            {
                return string.Format("CAST ({0} as time(7))", Operand.ToSql(builder));
            }
            if (builder.DbType == DriverType.PostgreSQL)
            {
                return string.Format("CAST ({0} as time)", Operand.ToSql(builder));
            }
            return ToSqlException();
        }
        public override string ToStr() { return "StrToTime(" + Operand.ToStr() + ")"; }
    }
}
