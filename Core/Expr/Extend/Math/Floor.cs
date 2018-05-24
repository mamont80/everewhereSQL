using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore.Expr.Simple;

namespace ParserCore.Expr.Extend.Math
{
    public class Floor : FuncExpr_OneOperand
    {
        public override void Prepare()
        {
            base.Prepare();
            var t1 = Operand.GetResultType();
            if (!(t1 == SimpleTypes.Float || t1 == SimpleTypes.Integer)) TypesException();
            SetResultType(t1);
            GetFloatResultOut = CalcFloat;
            GetIntResultOut = CalcInt;
        }

        private double CalcFloat(object data) { return System.Math.Floor(Operand.GetFloatResultOut(data)); }

        private long CalcInt(object data) { return (long)System.Math.Floor((decimal)Operand.GetIntResultOut(data)); }

        public override string ToStr() { return "floor(" + Operand.ToStr() + ")"; }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            return "floor(" + Operand.ToSql(builder) + ")";
        }
    }
}
