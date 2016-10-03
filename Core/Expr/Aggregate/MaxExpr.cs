using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore;
using ParserCore.Expr.Simple;

namespace ParserCore.Expr.Aggregate
{
    public class MaxExpr : FuncExpr_OneOperand
    {
        public override void Prepare()
        {
            base.Prepare();
            if (Operand.GetResultType() == SimpleTypes.Boolean ||
                Operand.GetResultType() == SimpleTypes.Geometry) TypesException();
            SetResultType(Operand.GetResultType());
            GetFloatResultOut = GetResult;
        }
        protected override bool CanCalcOnline() { return false; }

        private double GetResult(object data)
        {
            throw new Exception("Can not on fly calculation");
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            return " max(" + Operand.ToSql(builder) + ")";
        }
        public override string ToStr() { return "max(" + Operand.ToStr() + ")"; }
    }
}
