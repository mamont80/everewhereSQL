using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore.Expr.Simple;

namespace ParserCore.Expr.Extend.Math
{
    public class Log : Func_VariousOperand
    {
        public override void Prepare()
        {
            base.Prepare();
            if (Childs == null || Childs.Count < 1 || Childs.Count > 2) throw new Exception("Wrong number operands");
            var t = Childs[0].GetResultType();
            if (t != SimpleTypes.Float && t != SimpleTypes.Integer) TypesException();
            if (Childs.Count == 1)
            {
                SetResultType(SimpleTypes.Float);
                GetFloatResultOut = GetFloatResult;
            }
            else
            {
                var t2 = Childs[1].GetResultType();
                if (t2 != SimpleTypes.Integer)
                    ThrowException("The second parameter of the function LOG must be integer");
                SetResultType(SimpleTypes.Float);
                GetFloatResultOut = GetFloatResult;
            }
        }

        private double GetFloatResult(object data)
        {
            double r1 = Childs[0].GetFloatResultOut(data);
            if (Childs.Count > 1)
            {
                int r2 = (int)Childs[1].GetIntResultOut(data);
                return System.Math.Log(r1, r2);
            }
            return System.Math.Log(r1);
        }

        public override string ToStr()
        {
            string s = "LOG(" + Childs[0].ToStr();
            if (Childs.Count > 1) s = s + ", " + Childs[1].ToStr();
            s += ")";
            return s;
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            string s = "";
            if (builder.DbType == DriverType.SqlServer)
            {
                s += "LOG(" + Childs[0].ToSql(builder);
            }
            if (builder.DbType == DriverType.PostgreSQL)
            {
                s += "log(" + Childs[0].ToSql(builder);
            }
            if (Childs.Count > 1)
            {
                s += ", " + Childs[1].ToSql(builder);
            }
            s += ")";
            return s;
        }

    }
}
