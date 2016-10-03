using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.Simple
{
    /// <summary>
    /// Операция логического NOT
    /// </summary>
    public class Not_BoolExpr : Custom_OneOperand
    {
        public override void Prepare()
        {
            base.Prepare();
            if (Operand == null) OperandNotFoundException();
            if (!(Operand is NullConstExpr) && (Operand.GetResultType() != SimpleTypes.Boolean)) this.TypesException();
            GetBoolResultOut = DoNot;
            SetResultType(SimpleTypes.Boolean);
        }
        public override bool IsRightAssociate() { return true; }

        private bool DoNot(object data) { return !Operand.GetBoolResultOut(data); }
        public override int Priority() { return PriorityConst.Not; }
        public override string ToStr() { return "not(" + Operand.ToStr() + ")"; }
        public override string ToSql(ExpressionSqlBuilder builder) { return " not(" + Operand.ToSql(builder) + ")"; }
    }
}
