using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore;
using ParserCore.Expr.Simple;


namespace ParserCore.Expr.Sql
{
    public class AnyExpr: FuncExpr_OneOperand
    {
        public override void Prepare()
        {
            base.Prepare();
            if (!(Operand is SelectExpresion)) TypesException();
            SelectExpresion sel = (SelectExpresion) Operand;
            if (sel.TableColumns.Count != 1) throw new Exception("Subselct must return 1 column only");
            var tp = sel.TableColumns[0].SimpleType;
            SetResultType(tp);
            GetBoolResultOut = GetBoolRes;
        }

        public bool GetBoolRes(object data)
        {
            throw new NotImplementedException();
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            return " ANY(" + Operand.ToSql(builder) + ")";
        }

        public override string ToStr() { return " ANY(" + Operand.ToStr() + ")"; }
    }
}
