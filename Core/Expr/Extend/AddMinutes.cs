using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore.Expr.Simple;


namespace ParserCore.Expr.Extend
{
    public class AddMinutes : FuncExpr_TwoOperand
    {
        public override void Prepare()
        {
            base.Prepare();
            SimpleTypes st = SimpleTypes.DateTime;
            if (Operand1.GetResultType() == SimpleTypes.DateTime)
            {
                st = SimpleTypes.DateTime;
                GetDateTimeResultOut = GetResultDate;
            }
            if (Operand1.GetResultType() == SimpleTypes.Date)
            {
                st = SimpleTypes.Date;
                GetDateTimeResultOut = GetResultDate;
            }
            if (Operand1.GetResultType() == SimpleTypes.Time)
            {
                st = SimpleTypes.Time;
                GetTimeResultOut = GetResultTime;
            }
            SetResultType(st);
        }

        public virtual DateTime GetResultDate(object data)
        {
            return Operand1.GetDateTimeResultOut(data).AddMinutes(Operand2.GetFloatResultOut(data));
        }
        public virtual TimeSpan GetResultTime(object data)
        {
            return Operand1.GetTimeResultOut(data).Add(TimeSpan.FromMinutes(Operand2.GetFloatResultOut(data)));
        }
        public override string ToSql(ExpressionSqlBuilder builder)
        {
            if (builder.DbType == DriverType.SqlServer)
            {
                return "DATEADD(minute," + Operand2.ToSql(builder) + "," + Operand1.ToSql(builder) + ")";
            }
            if (builder.DbType == DriverType.PostgreSQL)
            {
                return "(" + Operand1.ToSql(builder) + "+" + Operand2.ToSql(builder) + " * interval '1 minute')";
            }
            return ToSqlException();

        }
        public override string ToStr() { return "AddMinutes(" + Operand1.ToStr() + "," + Operand2.ToStr() + ")"; }
    }
}
