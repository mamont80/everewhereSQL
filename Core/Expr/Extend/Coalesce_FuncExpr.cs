using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore;

namespace ParserCore.Expr.Extend
{
    public class Coalesce_FuncExpr : Expression
    {
        public override bool IsOperation() { return false; }
        public override bool IsFunction() { return true; }
        public override int NumChilds() { return -1; }

        public override void Prepare()
        {
            base.Prepare();

            if (ChildsCount() < 1) throw new Exception("Arguments is not found");

            List<SimpleTypes> types = new List<SimpleTypes>();
            for (int i = 0; i < Childs.Count; i++)
            {
                types.Add(Childs[i].GetResultType());
            }
            types = types.Distinct().ToList();
            if (types.Count == 0 || types.Count > 2) TypesException();
            SimpleTypes t = types[0];
            if (types.Count == 2)
            {
                if ((types[0] == SimpleTypes.Float && types[1] == SimpleTypes.Integer) || (types[1] == SimpleTypes.Float && types[0] == SimpleTypes.Integer))
                {
                    t = SimpleTypes.Float;
                }
                else TypesException();
            }
            //CompareItem
            switch (t)
            {
                case SimpleTypes.Boolean:
                    SetResultType(SimpleTypes.Boolean);
                    GetStrResultOut = StrRes;
                    break;
                case SimpleTypes.Date:
                case SimpleTypes.DateTime:
                    SetResultType(SimpleTypes.DateTime);
                    GetDateTimeResultOut = DateTimeRes;
                    break;
                case SimpleTypes.Float:
                    SetResultType(SimpleTypes.Float);
                    GetFloatResultOut = FloatRes;
                    break;
                case SimpleTypes.Geometry:
                    SetResultType(SimpleTypes.Geometry);
                    GetGeomResultOut = GeomRes;
                    break;
                case SimpleTypes.Integer:
                    SetResultType(SimpleTypes.Integer);
                    GetIntResultOut = IntRes;
                    break;
                case SimpleTypes.String:
                    SetResultType(SimpleTypes.String);
                    GetStrResultOut = StrRes;
                    break;
                case SimpleTypes.Time:
                    SetResultType(SimpleTypes.Time);
                    GetTimeResultOut = TimeRes;
                    break;
            }
        }

        private string StrRes(object data)
        {
            for (int i = 0; i < Childs.Count; i++)
            {
                if (!Childs[i].GetNullResultOut(data)) return Childs[i].GetStrResultOut(data);
            }
            return Childs.Last().GetStrResultOut(data);
        }

        private Int64 IntRes(object data)
        {
            for (int i = 0; i < Childs.Count; i++)
            {
                if (!Childs[i].GetNullResultOut(data)) return Childs[i].GetIntResultOut(data);
            }
            return Childs.Last().GetIntResultOut(data);
        }

        private double FloatRes(object data)
        {
            for (int i = 0; i < Childs.Count; i++)
            {
                if (!Childs[i].GetNullResultOut(data)) return Childs[i].GetFloatResultOut(data);
            }
            return Childs.Last().GetFloatResultOut(data);
        }

        private DateTime DateTimeRes(object data)
        {
            for (int i = 0; i < Childs.Count; i++)
            {
                if (!Childs[i].GetNullResultOut(data)) return Childs[i].GetDateTimeResultOut(data);
            }
            return Childs.Last().GetDateTimeResultOut(data);
        }

        private TimeSpan TimeRes(object data)
        {
            for (int i = 0; i < Childs.Count; i++)
            {
                if (!Childs[i].GetNullResultOut(data)) return Childs[i].GetTimeResultOut(data);
            }
            return Childs.Last().GetTimeResultOut(data);
        }

        private object GeomRes(object data)
        {
            for (int i = 0; i < Childs.Count; i++)
            {
                if (!Childs[i].GetNullResultOut(data)) return Childs[i].GetGeomResultOut(data);
            }
            return Childs.Last().GetGeomResultOut(data);
        }

        private bool BoolRes(object data)
        {
            for (int i = 0; i < Childs.Count; i++)
            {
                if (!Childs[i].GetNullResultOut(data)) return Childs[i].GetBoolResultOut(data);
            }
            return Childs.Last().GetBoolResultOut(data);
        }


        public override string ToStr()
        {
            string s = "Coalesce(";
            for (int i = 0; i < Childs.Count; i++)
            {
                if (i >= 1) s += ", ";
                s += Childs[i].ToStr();
            }
            s += ")";
            return s;
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            string s = "coalesce(";
            for (int i = 0; i < Childs.Count; i++)
            {
                if (i >= 1) s += ", ";
                s += Childs[i].ToSql(builder);
            }
            s += ")";
            return s;
        }
    }

}
