using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore.Expr.Simple;

namespace ParserCore
{
    // (val1)
    internal class SubExpressionParser : CustomParser
    {
        public SubExpressionParser(LexemCollection collection)
            : base(collection)
        {
        }
        public override void Parse()
        {
            var idx = Collection.IndexLexem;
            var le = Collection.GotoNext();
            if (le == null) Collection.Error("неожиданный конец", Collection.GetPrev());
            if (le.IsSkobraClose()) Collection.Error("пустое выражение", Collection.CurrentLexem());
            ExpressionParser tonode = new ExpressionParser(Collection);
            tonode.Parse();
            if (Collection.CurrentLexem() == null) Collection.Error("нет закрывающейся скобки", Collection.GetLast());
            if (tonode.Results.Count == 0) Collection.Error("нет значений", Collection.Get(idx));
            if (tonode.Results.Count > 1) Collection.Error("несколько значений", Collection.Get(idx));
            var t = new SubExpression();
            t.AddChild(tonode.Single());
            Results.Add(t);
            if (!Collection.CurrentLexem().IsSkobraClose()) Collection.Error("нет закрывающейся скобки", Collection.CurrentLexem());
        }
    }
}
