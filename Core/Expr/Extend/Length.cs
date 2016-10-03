using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore;
using ParserCore.Expr.Simple;

namespace ParserCore.Expr.Extend
{
    public class Length : FuncExpr_OneOperand
    {

        public override void Prepare()
        {
            base.Prepare();
            if (!(Operand.GetResultType() == SimpleTypes.String)) TypesException();
            SetResultType(SimpleTypes.Integer);
            GetIntResultOut = CalcRes;
        }
        private Int64 CalcRes(object data) { return Operand.GetStrResultOut(data).Length; }

        public override string ToStr() { return "length(" + Operand.ToStr() + ")"; }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            if (builder.DbType == DriverType.SqlServer)
            {
                return "(len(" + Operand.ToSql(builder) + "))";
            }
            if (builder.DbType == DriverType.PostgreSQL)
            {
                return "(length(" + Operand.ToSql(builder) + "))";
            }
            return ToSqlException();
        }
    }
}
