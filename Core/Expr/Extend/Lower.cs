using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore;
using ParserCore.Expr.Simple;

namespace ParserCore.Expr.Extend
{
    public class Lower_operation : FuncExpr_OneOperand
    {

        public override void Prepare()
        {
            base.Prepare();
            if (!(Operand.GetResultType() == SimpleTypes.String)) TypesException();
            SetResultType(SimpleTypes.String);
            GetStrResultOut = CalcRes;
        }
        private string CalcRes(object data) { return Operand.GetStrResultOut(data).ToLower(); }

        public override string ToStr() { return "lower(" + Operand.ToStr() + ")"; }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            return "lower(" + Operand.ToSql(builder) + ")";
        }
    }
}
