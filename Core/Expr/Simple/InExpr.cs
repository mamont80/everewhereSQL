using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore.Expr.Simple;
using ParserCore.Expr.Sql;

namespace ParserCore.Expr.Simple
{
    public class InExpr : Custom_TwoOperand
    {
        public override bool IsOperation() { return true; }
        public override bool IsFunction() { return false; }
        public override int NumChilds() { return 2; }
        public override int Priority()
        {
            return PriorityConst.In;
        }

        public delegate bool BoolItemResult(Expression Operand1, Expression Operand2, object data);

        protected BoolItemResult CompareItem;

        public override void Prepare()
        {
            if (Childs.Count != 2) throw new Exception("invalid IN operation");
            if (!(Childs[1] is SubExpression)) throw new Exception("After IN operation wait '()'");
            ((SubExpression)(Childs[1])).MultiValues = true;
            base.Prepare();

            SetResultType(SimpleTypes.Boolean);
            if (Childs[1].ChildsCount() == 1 && Childs[1].Childs[0] is SelectExpresion) return;

            List<SimpleTypes> types = new List<SimpleTypes>();
            for (int i = 0; i < Childs[1].Childs.Count; i++)
            {
                var tp = CustomEqual.GetCompareType(Childs[0], Childs[1].Childs[i]);
                if (tp == null) TypesException();
                types.Add(tp.Value);
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
                    CompareItem = CompareAsBool;
                    break;
                case SimpleTypes.Date:
                case SimpleTypes.DateTime:
                    CompareItem = CompareAsDateTime;
                    break;
                case SimpleTypes.Float:
                    CompareItem = CompareAsFloat;
                    break;
                case SimpleTypes.Geometry:
                    throw new Exception("Can not compare geometries");
                //    CompareItem = CompareAsGeom;
                //    break;
                case SimpleTypes.Integer:
                    CompareItem = CompareAsInt;
                    break;
                case SimpleTypes.String:
                    CompareItem = CompareAsStr;
                    break;
                case SimpleTypes.Time:
                    CompareItem = CompareAsTime;
                    break;
            }

            GetBoolResultOut = GetResult;
        }

        protected virtual bool GetResult(object data)
        {
            for (int i = 0; i < Operand2.Childs.Count; i++)
            {
                if (CompareItem(Operand1, Operand2.Childs[i], data)) return true;
            }
            return false;
        }

        protected static bool CompareAsInt(Expression Operand1, Expression Operand2, object data) { return (Operand1.GetIntResultOut(data) == Operand2.GetIntResultOut(data)); }
        protected static bool CompareAsFloat(Expression Operand1, Expression Operand2, object data) { return (Operand1.GetFloatResultOut(data) == Operand2.GetFloatResultOut(data)); }
        protected static bool CompareAsDateTime(Expression Operand1, Expression Operand2, object data) { return (Operand1.GetDateTimeResultOut(data) == Operand2.GetDateTimeResultOut(data)); }
        protected static bool CompareAsTime(Expression Operand1, Expression Operand2, object data) { return (Operand1.GetTimeResultOut(data) == Operand2.GetTimeResultOut(data)); }
        protected static bool CompareAsBool(Expression Operand1, Expression Operand2, object data) { return (Operand1.GetBoolResultOut(data) == Operand2.GetBoolResultOut(data)); }
        //protected static bool CompareAsGeom(Expression Operand1, Expression Operand2, object data) { return (Operand1.GetGeomResultOut(data).Equal(Operand2.GetGeomResultOut(data))); }
        protected static bool CompareAsStr(Expression Operand1, Expression Operand2, object data) { return (Operand1.GetStrResultOut(data) == Operand2.GetStrResultOut(data)); }

        public override string ToStr()
        {
            string s = Operand1.ToStr() + " in " + Operand2.ToStr();
            return s;
        }
        public override string ToSql(ExpressionSqlBuilder builder)
        {
            string s = Operand1.ToSql(builder) + " in " + Operand2.ToSql(builder);
            return s;
        }

        public override void ParseInside(ExpressionParser parser)
        {
            var collection = parser.Collection;

            var q = collection.GotoNext();
            if (q == null || !q.IsSkobraOpen())
                collection.Error("Function arguments not found", collection.GetPrev());
            FuncArgsParser tt = new FuncArgsParser(collection);
            tt.Parse();
            SubExpression sub = new SubExpression();
            tt.Results.ForEach(a => sub.AddChild(a));
            q.Expr = sub;
            parser.OutExp.Add(q);
            base.ParseInside(parser);
            parser.waitValue = false;
        }
    }

}
