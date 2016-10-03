using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore;
using ParserCore.Expr.Simple;

namespace ParserCore.Expr.Extend
{
    public class Right : FuncExpr_TwoOperand
    {
        public override void Prepare()
        {
            base.Prepare();
            if (!(Operand1.GetResultType() == SimpleTypes.String && Operand2.GetResultType() == SimpleTypes.Integer)) TypesException();
            SetResultType(SimpleTypes.String);
            GetStrResultOut = CalcAsStr;
        }
        private string CalcAsStr(object data)
        {
            string s = Operand1.GetStrResultOut(data);
            return s.Substring(s.Length - (int)Operand2.GetIntResultOut(data));
        }

        public override string ToStr() { return "Right(" + Operand1.ToStr() + ", " + Operand2.ToStr() + ")"; }
        public override string ToSql(ExpressionSqlBuilder builder)
        {
            return "right(" + Operand1.ToSql(builder) + "," + Operand2.ToSql(builder) + ")";
        }
    }
}
