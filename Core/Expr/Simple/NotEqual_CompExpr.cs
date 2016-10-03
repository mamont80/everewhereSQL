using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.Simple
{
    /// <summary>
    /// Опреация НЕ РАВНО
    /// </summary>
    public class NotEqual_CompExpr : CustomEqual
    {
        protected override bool CompareAsInt(object data) { return (Operand1.GetIntResultOut(data) != Operand2.GetIntResultOut(data)); }
        protected override bool CompareAsFloat(object data) { return (Operand1.GetFloatResultOut(data) != Operand2.GetFloatResultOut(data)); }
        protected override bool CompareAsDateTime(object data) { return (Operand1.GetDateTimeResultOut(data) != Operand2.GetDateTimeResultOut(data)); }
        protected override bool CompareAsTime(object data) { return (Operand1.GetTimeResultOut(data) != Operand2.GetTimeResultOut(data)); }
        protected override bool CompareAsBool(object data) { return (Operand1.GetBoolResultOut(data) != Operand2.GetBoolResultOut(data)); }
        //protected override bool CompareAsGeom(object data) { return !(Operand1.GetGeomResultOut(data).Equal(Operand2.GetGeomResultOut(data))); }
        protected override bool CompareAsStr(object data) { return (Operand1.GetStrResultOut(data) != Operand2.GetStrResultOut(data)); }
        public override string ToStr() { return "(" + Operand1.ToStr() + " <> " + Operand2.ToStr() + ")"; }
        public override string ToSql(ExpressionSqlBuilder builder) { return "(" + Operand1.ToSql(builder) + " <> " + Operand2.ToSql(builder) + ")"; }
    }
}
