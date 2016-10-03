using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore;
using ParserCore.Expr.Simple;

namespace ParserCore.Expr.Aggregate
{
    public class AvgExpr : FuncExpr_OneOperand
    {
        public override void Prepare()
        {
            base.Prepare();
            if (Operand.GetResultType() != SimpleTypes.Integer &&
                Operand.GetResultType() != SimpleTypes.Float
                ) TypesException();
            SimpleTypes st = Operand.GetResultType();
            if (st == SimpleTypes.Integer) st = SimpleTypes.Float;
            SetResultType(st);
            GetFloatResultOut = GetResult;
        }

        protected override bool CanCalcOnline()
        {
            return false;
        }

        private double GetResult(object data)
        {
            throw new Exception("Can not on fly calculation");
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            return " avg(" + Operand.ToSql(builder) + ")";
        }

        public override string ToStr()
        {
            return "avg(" + Operand.ToStr() + ")";
        }
    }
}
