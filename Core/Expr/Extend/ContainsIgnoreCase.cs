using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore.Expr.Simple;

namespace ParserCore.Expr.Extend
{
    public class ContainsIgnoreCase : Custom_TwoOperand
    {
        public override int Priority()
        {
            return PriorityConst.Like;
        }

        public override void Prepare()
        {
            base.Prepare();
            if (!(Operand1.GetResultType() == SimpleTypes.String && Operand2.GetResultType() == SimpleTypes.String)) TypesException();
            if (!Operand2.OnlyOnline()) OperandOnlyConstException(2);
            SetResultType(SimpleTypes.Boolean);
            GetBoolResultOut = CalcAsBool;
        }
        private bool CalcAsBool(object data) { return Operand1.GetStrResultOut(data).IndexOf(Operand2.GetStrResultOut(data), StringComparison.OrdinalIgnoreCase) >= 0; }

        public override string ToStr() { return "(" + Operand1.ToStr() + " ContainsCase " + Operand2.ToStr() + ")"; }
        public override string ToSql(ExpressionSqlBuilder builder)
        {
            if (builder.DbType == DriverType.SqlServer)
            {
                return string.Format("(CHARINDEX(upper({0}),upper({1}))>0)", Operand2.ToSql(builder), Operand1.ToSql(builder));
            }
            if (builder.DbType == DriverType.PostgreSQL)
            {
                return string.Format("(position(upper({0}) in upper({1}))>0)", Operand2.ToSql(builder), Operand1.ToSql(builder));
            }
            return ToSqlException();
        }
    }

}
