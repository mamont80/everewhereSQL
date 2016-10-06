using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    // (val1, val2)
    internal class FuncArgsParser : CustomParser
    {
        public FuncArgsParser(LexemCollection collection)
            : base(collection)
        {
        }
        public override void Parse()
        {
            var idx = Collection.IndexLexem;
            var le = Collection.GotoNext();
            if (le == null) Collection.Error("неожиданный конец", Collection.GetPrev());
            while (true)
            {
                if (le.IsSkobraClose()) return;
                ExpressionParser tonode = new ExpressionParser(Collection);
                tonode.Parse();
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
