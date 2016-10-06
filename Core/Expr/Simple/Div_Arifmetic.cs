using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.Simple
{
    public class Div_Arifmetic : Other_Arifmetic
    {
        public override void Prepare()
        {
            base.Prepare();
            //Всегда Float
            GetFloatResultOut = CalcAsFloat;
            SetResultType(SimpleTypes.Float);
        }

        protected override long CalcAsInt(object data) { return (int)(Operand1.GetIntResultOut(data) / Operand2.GetIntResultOut(data)); }
        protected override double CalcAsFloat(object data) { return Operand1.GetFloatResultOut(data) / Operand2.GetFloatResultOut(data); }
        public override string ToStr() { return  Operand1.ToStr() + " / " + Operand2.ToStr(); }
        public override string ToSql(ExpressionSqlBuilder builder) { return "(" + Operand1.ToSql(builder) + " / " + Operand2.ToSql(builder) + ")"; }
        public override int Priority() { return PriorityConst.MultiDiv; }
    }
}
