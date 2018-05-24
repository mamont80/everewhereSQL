using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore.Expr.Simple;

namespace ParserCore.Expr.Extend.Math
{
    public class Tan : FuncExpr_OneOperand
    {
        public override void Prepare()
        {
            base.Prepare();
            var t1 = Operand.GetResultType();
            if (!(t1 == SimpleTypes.Float || t1 == SimpleTypes.Integer)) TypesException();
            SetResultType(SimpleTypes.Float);
            GetFloatResultOut = CalcRes;
        }
        private double CalcRes(object data) { return System.Math.Tan(Operand.GetFloatResultOut(data)); }

        public override string ToStr() { return "tan(" + Operand.ToStr() + ")"; }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            return "tan(" + Operand.ToSql(builder) + ")";
        }
    }
}
