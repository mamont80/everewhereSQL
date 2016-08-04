using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    // Фунции работающие только для экспорта в SQL
    public class AllColumnExpr : Expression
    {
        public override bool IsOperation()
        {
            return false;
        }
        public string Prefix;
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            //if (Operand.GetResultType() != ColumnSimpleTypes.String) TypesException();
            SetResultType(SimpleTypes.String);
            GetStrResultOut = GetResult;
        }
        public override int NumChilds() { return 0; }
        protected override bool CanCalcOnline() { return false; }

        private string GetResult(object data)
        {
            throw new Exception("Can not on fly calculation");
        }

        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return "*";
        }
        public override string ToStr() { return "*"; }
    }

    public class CountExpr : FuncExpr_OneOperand
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            //if (Operand.GetResultType() != ColumnSimpleTypes.String) TypesException();
            SetResultType(SimpleTypes.Integer);
            GetIntResultOut = GetResult;
        }
        protected override bool CanCalcOnline() { return false; }

        private long GetResult(object data)
        {
            throw new Exception("Can not on fly calculation");
        }

        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return " count(" + Operand.ToSQL(builder) + ")"; 
        }
        public override string ToStr() { return "count(" + Operand.ToStr() + ")"; }
    }

    public class SumExpr : FuncExpr_OneOperand
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (Operand.GetResultType() != SimpleTypes.Float && Operand.GetResultType() != SimpleTypes.Integer && Operand.GetResultType() != SimpleTypes.Time) TypesException();
            SetResultType(Operand.GetResultType());
            GetFloatResultOut = GetResult;
        }
        protected override bool CanCalcOnline() { return false; }

        private double GetResult(object data)
        {
            throw new Exception("Can not on fly calculation");
        }

        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return " sum(" + Operand.ToSQL(builder) + ")";
        }
        public override string ToStr() { return "sum(" + Operand.ToStr() + ")"; }
    }

    public class MinExpr : FuncExpr_OneOperand
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
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

        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return " min(" + Operand.ToSQL(builder) + ")";
        }
        public override string ToStr() { return "min(" + Operand.ToStr() + ")"; }
    }

    public class MaxExpr : FuncExpr_OneOperand
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
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

        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return " max(" + Operand.ToSQL(builder) + ")";
        }
        public override string ToStr() { return "max(" + Operand.ToStr() + ")"; }
    }

    public class AvgExpr : FuncExpr_OneOperand
    {
        protected override void BeforePrepare()
        {
            base.BeforePrepare();
            if (Operand.GetResultType() != SimpleTypes.Integer &&
                Operand.GetResultType() != SimpleTypes.Float
                ) TypesException();
            SimpleTypes st = Operand.GetResultType();
            if (st == SimpleTypes.Integer) st = SimpleTypes.Float;
            SetResultType(st);
            GetFloatResultOut = GetResult;
        }
        protected override bool CanCalcOnline() { return false; }

        private double GetResult(object data)
        {
            throw new Exception("Can not on fly calculation");
        }

        public override string ToSQL(ExpressionSqlBuilder builder)
        {
            return " avg(" + Operand.ToSQL(builder) + ")";
        }

        public override string ToStr() { return "avg(" + Operand.ToStr() + ")"; }
    }
}
