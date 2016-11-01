using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.Simple
{
    public class SubExpression : Expression
    {
        public override bool IsOperation() { return false; }
        public override bool IsFunction() { return false; }
        /// <summary>
        /// Эти скобки для операции: arg in (arg1, arg2, ...)
        /// или для insert values (a,b,c),(a,b,c)
        /// </summary>
        public bool MultiValues = false;

        protected override bool CanCalcOnline() //запрещаем оптимизировать если это для операции IN (1,2)
        {
            if (MultiValues) return false;
            return true;
        }

        public override void Prepare()
        {
            base.Prepare();
            if (Childs.Count == 0 && !MultiValues) throw new Exception("Empty subexpression");
            if (Childs.Count > 1 && !MultiValues) throw new Exception("Invalid subexpression");
            if (Childs.Count == 1)
            {
                var tp = Childs[0].GetResultType();
                SetResultType(Childs[0].GetResultType());
                switch (tp)
                {
                    case SimpleTypes.Boolean:
                        GetBoolResultOut = CalcAsBool;
                        break;
                    case SimpleTypes.Date:
                    case SimpleTypes.DateTime:
                        GetDateTimeResultOut = CalcDateTimeResult;
                        break;
                    case SimpleTypes.Float:
                        GetFloatResultOut = CalcFloatResult;
                        break;
                    case SimpleTypes.Geometry:
                        GetGeomResultOut = CalcGeomResult;
                        break;
                    case SimpleTypes.Integer:
                        GetIntResultOut = CalcIntResult;
                        break;
                    case SimpleTypes.String:
                        GetStrResultOut = CalcStrResult;
                        break;
                    case SimpleTypes.Time:
                        GetTimeResultOut = CalcTimeResult;
                        break;
                }
            }
        }
        public override bool GetNullResultOut(object data) { return Childs[0].GetNullResultOut(data); }

        public bool CalcAsBool(object data) { return Childs[0].GetBoolResultOut(data); }
        public long CalcIntResult(object data) { return Childs[0].GetIntResultOut(data); }
        public string CalcStrResult(object data) { return Childs[0].GetStrResultOut(data); }
        public double CalcFloatResult(object data) { return Childs[0].GetFloatResultOut(data); }
        public DateTime CalcDateTimeResult(object data) { return Childs[0].GetDateTimeResultOut(data); }
        public TimeSpan CalcTimeResult(object data) { return Childs[0].GetTimeResultOut(data); }
        public object CalcGeomResult(object data) { return Childs[0].GetGeomResultOut(data); }

        public override int NumChilds() { return -1; }

        public override string ToStr()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            for (int i = 0; i < Childs.Count; i++)
            {
                if (i != 0) sb.Append(", ");
                sb.Append(Childs[i].ToStr());
            }
            sb.Append(")");
            return sb.ToString();
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            for (int i = 0; i < Childs.Count; i++)
            {
                if (i != 0) sb.Append(", ");
                sb.Append(Childs[i].ToSql(builder));
            }
            sb.Append(")");
            return sb.ToString();
        }

    }
}
