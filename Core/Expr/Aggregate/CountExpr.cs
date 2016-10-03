using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore;
using ParserCore.Expr.Simple;
using ParserCore.Expr.Sql;

namespace ParserCore.Expr.Aggregate
{
    public class CountExpr : FuncExpr_OneOperand
    {
        public bool AllColumns = false;

        public override void Prepare()
        {
            if (ChildsCount() != 1) throw new Exception("Function COUNT() must have one argument");
            if (!(Operand is AllColumnExpr)) base.Prepare();

            SetResultType(SimpleTypes.Integer);
            GetIntResultOut = GetResult;
        }
        protected override bool CanCalcOnline() { return false; }

        private long GetResult(object data)
        {
            throw new Exception("Can not on fly calculation");
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            if (AllColumns) return "count(*)";
            return " count(" + Operand.ToSql(builder) + ")";
        }
        public override string ToStr()
        {
            if (AllColumns) return "count(*)";
            return "count(" + Operand.ToStr() + ")";
        }
    }
}
