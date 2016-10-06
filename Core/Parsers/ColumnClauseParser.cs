using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    internal class ColumnClauseParser
    {
        public List<ColumnClause> Columns = new List<ColumnClause>();
        private HashSet<string> nextStrings = new HashSet<string>() { "from", "where", "limit", "offset", "union", "except", "intersect" };
        
        public void Parse(LexemCollection collection)
        {
            while (true)
            {
                var idx = collection.IndexLexem;
                ExpressionParser tonode = new ExpressionParser(collection);
                tonode.Parse();
                if (tonode.Results.Count != 1) collection.Error("не верное число параметров", collection.Get(idx));
                ColumnClause r = new ColumnClause();
                r.ColumnExpression = tonode.Single();
                Columns.Add(r);
                var lex = collection.CurrentLexem();
                if (lex == null) return;
                if (lex.LexemText.ToLower() == "as")
                {
                    collection.GotoNext();
                    if (lex == null) return;
                    r.Alias = CommonParserFunc.ReadAlias(collection);
                    lex = collection.GotoNext();
                }
                else
                if (nextStrings.Contains(lex.LexemText.ToLower())) return;
                else
                {
                    if (lex.LexemType == LexType.Command || lex.LexemType == LexType.Text)
                    {
                        r.Alias = CommonParserFunc.ReadAlias(collection);
                        lex = collection.GotoNext();
                    }
                }
                if (lex == null) return;
                if (nextStrings.Contains(lex.LexemText.ToLower())) return;
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
