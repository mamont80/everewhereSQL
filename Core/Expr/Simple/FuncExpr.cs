using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.Simple
{
    /// <summary>
    /// Общий класс для всех сложных функций. К ним не относится Not()
    /// </summary>
    public class FuncExpr : Expression
    {
        public string FunctionName;
        //Функции идут как значения
        public override bool IsOperation() { return false; }
        public override bool IsFunction() { return true; }

        public override string ToStr()
        {
            string s = FunctionName + "(";
            for (int i = 0; i < Childs.Count; i++)
            {
                if (i != 0) s += ",";
                s += Childs[i].ToStr();
            }
            s += ")";
            return s;
        }

        public override void ParseInside(ExpressionParser parser)
        {
            var collection = parser.Collection;
            DoParseInside(this, collection);
            base.ParseInside(parser);
        }

        public static void DoParseInside(Expression exp, LexemCollection collection)
        {
            var idx = collection.IndexLexem;
            var le = collection.GotoNext();
            if (le == null) collection.Error("неожиданный конец", collection.GetPrev());
            if (!le.IsSkobraOpen()) collection.Error("Ожидалась открывающая скобка", collection.GetPrev());
            le = collection.GotoNext();
            while (true)
            {
                if (le.IsSkobraClose()) return;
                ExpressionParser tonode = new ExpressionParser(collection);
                tonode.Parse();
                if (collection.CurrentLexem() == null) collection.Error("нет закрывающейся скобки", collection.GetLast());
                if (tonode.Results.Count == 0) collection.Error("нет значений", collection.Get(idx));
                if (tonode.Results.Count > 1) collection.Error("несколько значений", collection.Get(idx));
                exp.AddChild(tonode.Results[0]);
                if (collection.CurrentLexem().LexemType == LexType.Zpt)
                {
                    collection.GotoNext();
                    continue;
                }
                if (collection.CurrentLexem().IsSkobraClose())
                {
                    return;
                }
                collection.Error("Unknow value", collection.CurrentLexem());
            }
        }
    }
}
