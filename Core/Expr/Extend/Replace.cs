using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore;
using ParserCore.Expr.Simple;

namespace ParserCore.Expr.Extend
{
    public class Replace : FuncExpr
    {
        public override void Prepare()
        {
            base.Prepare();
            if (Childs == null || Childs.Count != 3) throw new Exception("Wrong number operands");
            if (!(Childs[0].GetResultType() == SimpleTypes.String)) TypesException();
            if (!(Childs[1].GetResultType() == SimpleTypes.String)) TypesException();
            if (!(Childs[2].GetResultType() == SimpleTypes.String)) TypesException();
            
            SetResultType(SimpleTypes.String);
            GetStrResultOut = CalcRes;
        }

        private string CalcRes(object data)
        {
            string s;
            s = Childs[0].GetStrResultOut(data).Replace(Childs[1].GetStrResultOut(data), Childs[2].GetStrResultOut(data));
            return s;
        }

        public override string ToStr() { return "Replace(" + Childs[0].ToStr() + ", " + Childs[1].ToStr() + ", " + Childs[2].ToStr() + ")"; }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            if (builder.DbType == DriverType.SqlServer)
            {
                return "replace(" + Childs[0].ToSql(builder) + ", " + Childs[1].ToSql(builder) + ", " + Childs[2].ToSql(builder) + ")";
            }
            else if (builder.DbType == DriverType.PostgreSQL)
            {
                return "replace(" + Childs[0].ToSql(builder) + ", " + Childs[1].ToSql(builder) + ", " + Childs[2].ToSql(builder) + ")";
            }
            else return ToSqlException();
        }

    }
}
