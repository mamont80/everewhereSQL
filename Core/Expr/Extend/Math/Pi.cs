using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore.Expr.Simple;

namespace ParserCore.Expr.Extend.Math
{
    public class Pi : FuncExpr_WithoutOperand
    {
        public override void Prepare()
        {
            base.Prepare();
            SetResultType(SimpleTypes.Float);
            GetFloatResultOut = CalcRes;
        }
        private double CalcRes(object data) { return System.Math.PI; }

        public override string ToStr() { return "pi()"; }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            return "pi()";
        }
    }
}
