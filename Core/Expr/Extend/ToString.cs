using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore;
using ParserCore.Expr.Simple;

namespace ParserCore.Expr.Extend
{
    public class ToString : FuncExpr_OneOperand
    {
        public override void Prepare()
        {
            base.Prepare();
            var tp = Operand.GetResultType();// 
            if (tp == SimpleTypes.Geometry) TypesException();
            SetResultType(SimpleTypes.String);
            GetStrResultOut = GetResult;
        }

        private string GetResult(object data)
        {
            var tp = Operand.GetResultType();
            var val = Operand.GetObjectResultOut(data);
            switch (tp)
            {
                case SimpleTypes.Date:
                    return ((DateTime)val).ToString("dd.MM.yyyy");
                case SimpleTypes.DateTime:
                    return ((DateTime)val).ToString("dd.MM.yyyy hh:mm:ss");
                case SimpleTypes.Float:
                    return ((double)Convert.ToDouble(val)).ToStr();
                case SimpleTypes.Integer:
                    return ((Int64)Convert.ToInt64(val)).ToString();
                case SimpleTypes.String:
                    return Convert.ToString(val);
                case SimpleTypes.Time:
                    return ((TimeSpan)val).ToString("c");
                case SimpleTypes.Geometry:
                    TypesException();
                    break;
                default:
                    TypesException();
                    break;
            }
            return "";
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            if (builder.DbType == DriverType.PostgreSQL)
                return "(" + Operand.ToSql(builder) + ")::text";
            if (builder.DbType == DriverType.SqlServer)
                return string.Format("CAST (({0}) as nvarchar(MAX))", Operand.ToSql(builder));
            return ToSqlException();
        }
        public override string ToStr() { return "ToStr(" + Operand.ToStr() + ")"; }
    }
}
