using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LayerData;

namespace TableQuery
{
    public class OrderByToNode
    {
        public List<GroupBy> Columns = new List<GroupBy>();
        public void Parse(LexemCollection collection, bool isOrderBy)
        {
            while (true)
            {
                var idx = collection.IndexLexem;
                ExpressionToNode2 tonode = new ExpressionToNode2();
                tonode.Parse(collection);
                if (tonode.Results.Count != 1) collection.Error("не верное число параметров", collection.Get(idx));
                GroupBy r = new GroupBy();
                r.Expression = tonode.Single();
                Columns.Add(r);
                var lex = collection.CurrentLexem();
                if (lex == null) return;
                if (isOrderBy)
                {
                    if (lex.Lexem.ToLower() == "desc")
                    {
                        collection.GotoNext();
                        if (lex == null) return;
                        r.Sort = SortType.DESC;
                        lex = collection.GotoNext();
                    }
                    else if (lex.Lexem.ToLower() == "asc")
                    {
                        collection.GotoNext();
                        if (lex == null) return;
                        r.Sort = SortType.ASC;
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
