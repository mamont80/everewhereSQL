using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.Simple
{
    public class UniarMinus_BoolExpr : Custom_OneOperand
    {
        public override void Prepare()
        {
            base.Prepare();
            if (Operand == null) OperandNotFoundException();
            if (Operand.GetResultType() == SimpleTypes.Integer)
            {
                GetIntResultOut = DoMinusInt;
                SetResultType(SimpleTypes.Integer);
                return;
            }
            if (Operand.GetResultType() == SimpleTypes.Float)
            {
                GetFloatResultOut = DoMinusFloat;
                SetResultType(SimpleTypes.Float);
                return;
            }/*
            if (Operand.GetResultType() == ColumnSimpleTypes.Time)
            {
                GetTimeResultOut = DoMinusTime;
                SimpleType = ColumnSimpleTypes.Time;
                SetConvertors(ColumnSimpleTypes.Time);
                return;
            }*/
            TypesException();
        }
        protected virtual long DoMinusInt(object data) { return -Operand.GetIntResultOut(data); }
        protected virtual double DoMinusFloat(object data) { return -Operand.GetFloatResultOut(data); }
        //private TimeSpan DoMinusTime() { return -Operand.GetTimeResultOut(); }

        public override bool IsLeftOperand() { return true; }
        public override int Priority() { return PriorityConst.UnarMinus; }
        public override string ToStr() { return "-" + Operand.ToStr(); }
        public override string ToSql(ExpressionSqlBuilder builder) { return " -(" + Operand.ToSql(builder) + ")"; }
    }
}
