using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore;
using ParserCore.Expr.Simple;

namespace ParserCore.Expr.Sql
{
    public class ExistsExpr: FuncExpr_OneOperand
    {
        public override void Prepare()
        {
            base.Prepare();
            if (!(Operand is SelectExpresion)) TypesException();
            SetResultType(SimpleTypes.Boolean);
            GetBoolResultOut = GetBoolRes;
        }

        public bool GetBoolRes(object data)
        {
            throw new NotImplementedException();
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            return " EXISTS(" +Operand.ToSql(builder) + ")";
        }

        public override string ToStr() { return " EXISTS(" + Operand.ToStr() + ")"; }

    }
}
