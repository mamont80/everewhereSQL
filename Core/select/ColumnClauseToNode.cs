using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LayerData;

namespace ParserCore
{
    public class ColumnClauseToNode
    {
        public List<ColumnClause> Columns = new List<ColumnClause>();
        public void Parse(LexemCollection collection)
        {
            while (true)
            {
                var idx = collection.IndexLexem;
                ExpressionToNode2 tonode = new ExpressionToNode2();
                tonode.Parse(collection);
                if (tonode.Results.Count != 1) collection.Error("не верное число параметров", collection.Get(idx));
                ColumnClause r = new ColumnClause();
                r.ColumnExpression = tonode.Single();
                Columns.Add(r);
                var lex = collection.CurrentLexem();
                if (lex == null) return;
                if (lex.Lexem.ToLower() == "as")
                {
                    collection.GotoNext();
                    if (lex == null) return;
                    r.Alias = CommonFunc.ReadAlias(collection);
                    lex = collection.GotoNext();
                }
                else
                if (lex.Lexem.ToLower() == "from") return;
                else
                {
                    if (lex.LexemType == LexType.Command || lex.LexemType == LexType.Text)
                    {
                        r.Alias = CommonFunc.ReadAlias(collection);
                        lex = collection.GotoNext();
                    }
                }
                if (lex == null) return;
                if (lex.Lexem.ToLower() == "from") return;
                if (lex.LexemType == LexType.Zpt)
                {
                    collection.GotoNext();
                    continue;
                }
                return;
            }
        }
    }
}
