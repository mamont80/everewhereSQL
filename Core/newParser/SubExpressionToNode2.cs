using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableQuery
{
    // (val1)
    public class SubExpressionToNode2 : CustomToNode
    {
        public override void Parse(LexemCollection collection)
        {
            base.Parse(collection);
            var idx = Collection.IndexLexem;
            var le = Collection.GotoNext();
            if (le == null) Collection.Error("неожиданный конец", Collection.GetPrev());
            if (le.IsSkobraClose()) Collection.Error("пустое выражение", Collection.CurrentLexem());
            ExpressionToNode2 tonode = new ExpressionToNode2();
            tonode.Parse(Collection);
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
