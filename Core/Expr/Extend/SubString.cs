using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore;
using ParserCore.Expr.Simple;

namespace ParserCore.Expr.Extend
{
    public class SubString: FuncExpr
    {
        public override void Prepare()
        {
            base.Prepare();
            if (Childs == null || Childs.Count < 2 || Childs.Count > 3) throw new Exception("Wrong number operands");
            if (!(Childs[0].GetResultType() == SimpleTypes.String)) TypesException();//substring
            if (!(Childs[1].GetResultType() == SimpleTypes.Integer)) TypesException();//start
            if ((Childs.Count == 3) && !(Childs[2].GetResultType() == SimpleTypes.Integer)) TypesException();//count

            SetResultType(SimpleTypes.String);
            GetStrResultOut = CalcRes;
        }

        private string CalcRes(object data)
        {
            string s;
            if (Childs.Count == 3)
                s = Childs[0].GetStrResultOut(data).Substring((int)Childs[1].GetIntResultOut(data) + 1, (int)Childs[2].GetIntResultOut(data));
            else s = Childs[0].GetStrResultOut(data).Substring((int)Childs[1].GetIntResultOut(data) + 1);
            return s;
        }

        public override string ToStr() { return "SubString(" + Childs[0].ToStr() + ", " + Childs[1].ToStr() + ", " + Childs[2].ToStr() + ")"; }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            if (builder.DbType == DriverType.SqlServer)
            {
                if (Childs.Count == 3)
                    return "SUBSTRING(" + Childs[0].ToSql(builder) + ", " + Childs[1].ToSql(builder) + ", " + Childs[2].ToSql(builder) + ")";
                else return "SUBSTRING(" + Childs[0].ToSql(builder) + ", " + Childs[1].ToSql(builder) + ", len(" + Childs[0].ToSql(builder) + ")-" + Childs[1].ToSql(builder) + "+1)";
            }
            else if (builder.DbType == DriverType.PostgreSQL)
            {
                if (Childs.Count == 3)
                    return "substr(" + Childs[0].ToSql(builder) + ", " + Childs[1].ToSql(builder) + ", " + Childs[2].ToSql(builder) + ")";
                else return "substr(" + Childs[0].ToSql(builder) + ", " + Childs[1].ToSql(builder) + ")";
            }
            else return ToSqlException();
        }

    }
}
