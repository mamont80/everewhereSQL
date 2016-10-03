using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    internal class OrderByParser
    {
        public List<GroupByClause> Columns = new List<GroupByClause>();
        public void Parse(LexemCollection collection, bool isOrderBy)
        {
            while (true)
            {
                var idx = collection.IndexLexem;
                ExpressionParser tonode = new ExpressionParser();
                tonode.Parse(collection);
                if (tonode.Results.Count != 1) collection.Error("не верное число параметров", collection.Get(idx));
                GroupByClause r = null;
                if (isOrderBy) r = new OrderByClause();
                else r = new GroupByClause();
                r.Expression = tonode.Single();
                Columns.Add(r);
                var lex = collection.CurrentLexem();
                if (lex == null) return;
                if (isOrderBy)
                {
                    if (lex.LexemText.ToLower() == "desc")
                    {
                        collection.GotoNext();
                        if (lex == null) return;
                        ((OrderByClause)r).Sort = SortType.DESC;
                        lex = collection.GotoNext();
                    }
                    else if (lex.LexemText.ToLower() == "asc")
                    {
                        collection.GotoNext();
                        if (lex == null) return;
                        ((OrderByClause)r).Sort = SortType.ASC;
                        lex = collection.GotoNext();
                    }
                }
                if (lex == null) return;
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
