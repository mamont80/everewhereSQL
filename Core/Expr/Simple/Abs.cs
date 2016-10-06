using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore;

namespace ParserCore.Expr.Simple
{
    public class Abs: FuncExpr_OneOperand
    {
        public override void Prepare()
        {
            base.Prepare();
            if (!(Operand.GetResultType() == SimpleTypes.Integer || Operand.GetResultType() == SimpleTypes.Float)) TypesException();
            SetResultType(SimpleTypes.Integer);
            GetIntResultOut = CalcRes;
        }
        private Int64 CalcRes(object data) { return (Int64)Math.Abs(Operand.GetFloatResultOut(data)); }

        public override string ToStr() { return "abs(" + Operand.ToStr() + ")"; }
        public override string ToSql(ExpressionSqlBuilder builder)
        {
            return "abs(" + Operand.ToSql(builder) + ")";
        }
    }
}
