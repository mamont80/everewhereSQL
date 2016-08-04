using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableQuery
{
    // (val1, val2)
    public class FuncArgsToNode2 : CustomToNode
    {
        public override void Parse(LexemCollection collection)
        {
            base.Parse(collection);
            var idx = Collection.IndexLexem;
            var le = Collection.GotoNext();
            if (le == null) Collection.Error("неожиданный конец", Collection.GetPrev());
            while (true)
            {
                if (le.IsSkobraClose()) return;
                ExpressionToNode2 tonode = new ExpressionToNode2();
                tonode.Parse(Collection);
                if (Collection.CurrentLexem() == null) Collection.Error("нет закрывающейся скобки", Collection.GetLast());
                if (tonode.Results.Count == 0) Collection.Error("нет значений", Collection.Get(idx));
                if (tonode.Results.Count > 1) Collection.Error("несколько значений", Collection.Get(idx));
                Results.Add(tonode.Results[0]);
                if (Collection.CurrentLexem().LexemType == LexType.Zpt)
                {
                    Collection.GotoNext();
                    continue;
                }
                if (Collection.CurrentLexem().IsSkobraClose())
                {
                    return;
                }
                Collection.Error("Неизвесное значение", Collection.CurrentLexem());
            }
        }
    }
}
