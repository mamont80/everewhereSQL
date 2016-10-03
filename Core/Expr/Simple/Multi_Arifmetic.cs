using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.Simple
{
    public abstract class Other_Arifmetic : Custom_Arifmetic
    {
        public override void Prepare()
        {
            base.Prepare();
            SimpleTypes t1 = Operand1.GetResultType();
            SimpleTypes t2 = Operand2.GetResultType();
            //public enum ColumnSimpleTypes { Integer = 1, Float = 2, String = 3, Geometry = 4, Date = 5, DateTime = 6, Time = 7, Boolean = 8 }
            if (t1 == SimpleTypes.Integer && t2 == SimpleTypes.Integer)
            {
                GetIntResultOut = CalcAsInt;
                SetResultType(SimpleTypes.Integer);
                return;
            }
            if ((t1 == SimpleTypes.Integer || t1 == SimpleTypes.Float)
                && (t2 == SimpleTypes.Integer || t2 == SimpleTypes.Float))
            {
                GetFloatResultOut = CalcAsFloat;
                SetResultType(SimpleTypes.Float);
                return;
            }
            TypesException();
        }
        protected abstract long CalcAsInt(object data);
        protected abstract double CalcAsFloat(object data);
    }

    public class Multi_Arifmetic : Other_Arifmetic
    {
        protected override long CalcAsInt(object data) { return Operand1.GetIntResultOut(data) * Operand2.GetIntResultOut(data); }
        protected override double CalcAsFloat(object data) { return Operand1.GetFloatResultOut(data) * Operand2.GetFloatResultOut(data); }
        public override string ToStr() { return "(" + Operand1.ToStr() + " * " + Operand2.ToStr() + ")"; }
        public override string ToSql(ExpressionSqlBuilder builder) { return "(" + Operand1.ToSql(builder) + " * " + Operand2.ToSql(builder) + ")"; }
        public override int Priority() { return PriorityConst.MultiDiv; }
    }
}
