using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public class SelectToNode2 : CustomToNode
    {
        public SelectExpresion SelectExpresion;

        public override void Parse(LexemCollection collection)
        {
            base.Parse(collection);
            //текщая лексема SELECT
            var le = Collection.GotoNext();
            if (le == null) Collection.Error("неожиданный конец", Collection.GetPrev());
            if (le.Lexem.ToLower() == "distinct")
            {
                SelectExpresion.Query.Distinct = true;
                le = Collection.GotoNext();
                if (le == null) Collection.Error("неожиданный конец", Collection.GetPrev());
            }
            var idx = collection.IndexLexem;
            ColumnClauseToNode colsParser = new ColumnClauseToNode();
            colsParser.Parse(collection);
            if (colsParser.Columns.Count == 0) collection.Error("Columnn not found", collection.Get(idx));
            Results.Add(SelectExpresion);
            SelectExpresion.Query.Columns = colsParser.Columns;
            var lex = collection.CurrentLexem();
            if (lex == null) return;
            if (lex.Lexem.ToLower() != "from") return;

            lex = collection.GotoNext();
            FromClause fc = new FromClause();
            fc.Parse(collection);
            SelectExpresion.Query.Tables = fc.Tables;
            lex = collection.CurrentLexem();
            if (lex == null) return;
            if (lex.Lexem.ToLower() == "where")
            {
                lex = collection.GotoNext();
                if (lex == null) collection.Error("Where clause not found", collection.GetPrev());
                idx = collection.IndexLexem;
                ExpressionToNode2 tonode = new ExpressionToNode2();
                tonode.Parse(collection);
                if (tonode.Results.Count != 1) collection.Error("не верное число параметров", collection.Get(idx));
                SelectExpresion.Query.WhereExpr = tonode.Single();
            }
            lex = collection.CurrentLexem();
            if (lex == null) return;
            if (lex.Lexem.ToLower() == "having")
            {
                lex = collection.GotoNext();
                if (lex == null) collection.Error("Having clause not found", collection.GetPrev());
                idx = collection.IndexLexem;
                ExpressionToNode2 tonode = new ExpressionToNode2();
                tonode.Parse(collection);
                if (tonode.Results.Count != 1) collection.Error("не верное число параметров", collection.Get(idx));
                SelectExpresion.Query.Having = tonode.Single();
            }
            lex = collection.CurrentLexem();
            if (lex == null) return;
            if (lex.Lexem.ToLower() == "group")
            {
                lex = collection.GotoNext();
                if (lex == null) collection.Error("Group by clause error", collection.GetPrev());
                if (lex.Lexem.ToLower() != "by") collection.Error("Group by clause error", collection.GetPrev());
                lex = collection.GotoNext();
                OrderByToNode gb = new OrderByToNode();
                gb.Parse(collection, false);
                SelectExpresion.Query.GroupBys = gb.Columns;
                if (SelectExpresion.Query.GroupBys.Count == 0) collection.Error("\"Group by\" columns not found", collection.Get(idx));
                lex = collection.CurrentLexem();
            }
            lex = collection.CurrentLexem();
            if (lex == null) return;
            if (lex.Lexem.ToLower() == "order")
            {
                lex = collection.GotoNext();
                if (lex == null) collection.Error("Order by clause error", collection.GetPrev());
                if (lex.Lexem.ToLower() != "by") collection.Error("Order by clause error", collection.GetPrev());
                lex = collection.GotoNext();
                OrderByToNode gb = new OrderByToNode();
                gb.Parse(collection, true);
                SelectExpresion.Query.OrderBys = gb.Columns;
                if (SelectExpresion.Query.OrderBys.Count == 0) collection.Error("\"Order by\" columns not found", collection.Get(idx));
                lex = collection.CurrentLexem();
            }
            lex = collection.CurrentLexem();
            if (lex == null) return;
            if (lex.Lexem.ToLower() == "limit")
            {
                lex = collection.GotoNext();
                if (lex == null) collection.Error("Limit clause not found", collection.GetPrev());
                ExpressionToNode2 tonode = new ExpressionToNode2();
                tonode.Parse(collection);
                SelectExpresion.Query.LimitRecords = tonode.Single().GetIntResultOut(null);
            }
            lex = collection.CurrentLexem();
            if (lex == null) return;
            if (lex.Lexem.ToLower() == "offset")
            {
                lex = collection.GotoNext();
                if (lex == null) collection.Error("Offset clause not found", collection.GetPrev());
                ExpressionToNode2 tonode = new ExpressionToNode2();
                tonode.Parse(collection);
                SelectExpresion.Query.SkipRecords = tonode.Single().GetIntResultOut(null);
            }
        }
    }
}
