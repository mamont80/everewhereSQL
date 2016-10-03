using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore;
using ParserCore.Expr.Simple;

namespace ParserCore.Expr.Extend
{
    public class StrToDateTime : FuncExpr_OneOperand
    {
        public override void Prepare()
        {
            base.Prepare();
            if (Operand.GetResultType() != SimpleTypes.String) TypesException();
            SetResultType(SimpleTypes.DateTime);
            GetDateTimeResultOut = GetResult;
        }

        private DateTime GetResult(object data)
        {
            string s = Operand.GetStrResultOut(data);
            DateTime dt;
            ParserDateTimeStatus st = CommonUtils.ParseDateTime(s, out dt);
            if (st == ParserDateTimeStatus.Date || st == ParserDateTimeStatus.DateTime) return dt;
            throw new Exception("String value is not date type");
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            if (builder.DbType == DriverType.SqlServer)
            {
                return string.Format("CAST ({0} as datetime(7))", Operand.ToSql(builder));
            }
            if (builder.DbType == DriverType.PostgreSQL)
            {
                return string.Format("CAST ({0} as timestamp)", Operand.ToSql(builder));
            }
            return ToSqlException();
        }
        public override string ToStr() { return "StrToDateTime(" + Operand.ToStr() + ")"; }
    }
}
