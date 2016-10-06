using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore;
using ParserCore.Expr.Simple;

namespace ParserCore.Expr.Extend
{
    public class Position: FuncExpr_TwoOperand
    {
        public override void Prepare()
        {
            base.Prepare();
            if (!(Operand1.GetResultType() == SimpleTypes.String)) TypesException();//substring
            if (!(Operand2.GetResultType() == SimpleTypes.String)) TypesException();//string
            SetResultType(SimpleTypes.Integer);
            GetIntResultOut = CalcRes;
        }
        private Int64 CalcRes(object data)
        {
            var idx = Operand2.GetStrResultOut(data).IndexOf(Operand1.GetStrResultOut(data));
            return idx + 1;
        }

        public override string ToStr() { return "position(" + Operand1.ToStr()+", "+Operand2.ToStr() + ")"; }
        public override string ToSql(ExpressionSqlBuilder builder)
        {
            if (builder.DbType == DriverType.SqlServer)
            {
                return "CHARINDEX(" + Operand1.ToSql(builder) + ", " + Operand2.ToSql(builder) + ")";
            }
            else if (builder.DbType == DriverType.PostgreSQL)
            {
                return "position("+Operand1.ToSql(builder)+" in "+Operand2.ToSql(builder)+")";
            }
            else return ToSqlException();
        }
    }
}
