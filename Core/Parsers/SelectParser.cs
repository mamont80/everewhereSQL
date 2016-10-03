using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore.Expr.Sql;

namespace ParserCore
{
    internal class SelectParser : CustomParser
    {
        public SelectExpresion SelectExpresion;

        public override void Parse(LexemCollection collection)
        {
            base.Parse(collection);
            //текщая лексема SELECT
            var le = Collection.GotoNext();
            if (le == null) Collection.Error("неожиданный конец", Collection.GetPrev());
            if (le.LexemText.ToLower() == "distinct")
            {
                SelectExpresion.Distinct = true;
                le = Collection.GotoNextMust();
            }
            else if (le.LexemText.ToLower() == "all")
            {
                le = Collection.GotoNextMust();
            }
            var idx = collection.IndexLexem;
            ColumnClauseParser colsParser = new ColumnClauseParser();
            colsParser.Parse(collection);
            if (colsParser.Columns.Count == 0) collection.Error("Columnn not found", collection.Get(idx));
            Results.Add(SelectExpresion);
            SelectExpresion.Columns.Replace(colsParser.Columns);
            var lex = collection.CurrentLexem();
            if (lex == null) return;
            if (lex.LexemText.ToLower() != "from") return;

            lex = collection.GotoNext();
            FromParser fc = new FromParser();
            fc.Parse(collection);
            SelectExpresion.Tables.Replace(fc.Tables);
            lex = collection.CurrentLexem();
            if (lex == null) return;
            if (lex.LexemText.ToLower() == "where")
            {
                lex = collection.GotoNext();
                if (lex == null) collection.Error("Where clause not found", collection.GetPrev());
                idx = collection.IndexLexem;
                ExpressionParser tonode = new ExpressionParser();
                tonode.Parse(collection);
                if (tonode.Results.Count != 1) collection.Error("не верное число параметров", collection.Get(idx));
                SelectExpresion.WhereExpr = tonode.Single();
            }
            lex = collection.CurrentLexem();
            if (lex == null) return;
            if (lex.LexemText.ToLower() == "having")
            {
                lex = collection.GotoNext();
                if (lex == null) collection.Error("Having clause not found", collection.GetPrev());
                idx = collection.IndexLexem;
                ExpressionParser tonode = new ExpressionParser();
                tonode.Parse(collection);
                if (tonode.Results.Count != 1) collection.Error("не верное число параметров", collection.Get(idx));
                SelectExpresion.Having = tonode.Single();
            }
            lex = collection.CurrentLexem();
            if (lex == null) return;
            if (lex.LexemText.ToLower() == "group")
            {
                lex = collection.GotoNext();
                if (lex == null) collection.Error("Group by clause error", collection.GetPrev());
                if (lex.LexemText.ToLower() != "by") collection.Error("Group by clause error", collection.GetPrev());
                lex = collection.GotoNext();
                OrderByParser gb = new OrderByParser();
                gb.Parse(collection, false);
                SelectExpresion.GroupBys.Replace(gb.Columns);
                if (SelectExpresion.GroupBys.Count == 0) collection.Error("\"Group by\" columns not found", collection.Get(idx));
                lex = collection.CurrentLexem();
            }
            lex = collection.CurrentLexem();
            if (lex == null) return;
            if (lex.LexemText.ToLower() == "order")
            {
                lex = collection.GotoNext();
                if (lex == null) collection.Error("Order by clause error", collection.GetPrev());
                if (lex.LexemText.ToLower() != "by") collection.Error("Order by clause error", collection.GetPrev());
                lex = collection.GotoNext();
                OrderByParser gb = new OrderByParser();
                gb.Parse(collection, true);
                SelectExpresion.OrderBys.Replace(gb.Columns.Select(a => (OrderByClause)a).ToList());
                if (SelectExpresion.OrderBys.Count == 0) collection.Error("\"Order by\" columns not found", collection.Get(idx));
                lex = collection.CurrentLexem();
            }
            lex = collection.CurrentLexem();
            if (lex == null) return;
            if (lex.LexemText.ToLower() == "limit")
            {
                lex = collection.GotoNext();
                if (lex == null) collection.Error("Limit clause not found", collection.GetPrev());
                ExpressionParser tonode = new ExpressionParser();
                tonode.Parse(collection);
                SelectExpresion.LimitRecords = tonode.Single().GetIntResultOut(null);
            }
            lex = collection.CurrentLexem();
            if (lex == null) return;
            if (lex.LexemText.ToLower() == "offset")
            {
                lex = collection.GotoNext();
                if (lex == null) collection.Error("Offset clause not found", collection.GetPrev());
                ExpressionParser tonode = new ExpressionParser();
                tonode.Parse(collection);
                SelectExpresion.SkipRecords = tonode.Single().GetIntResultOut(null);
            }
            while (true)
            {
                lex = collection.CurrentLexem();
                if (lex == null) return;
                string s = lex.LexemText.ToLower();
                if (s == "union" || s == "intersect" || s == "except")
                {
                    lex = collection.GotoNextMust();
                    ExtSelectClause extS = new ExtSelectClause();
                    extS.Select = new SelectExpresion();
                    if (s == "union" && lex.LexemText.ToLower() == "all")
                    {
                        extS.Operation = SelectOperation.UnionAll;
                        lex = collection.GotoNextMust();
                    }
                    else if (s == "union") extS.Operation = SelectOperation.Union;
                    if (s == "intersect") extS.Operation = SelectOperation.Intersect;
                    if (s == "except") extS.Operation = SelectOperation.Except;
                    if (lex.LexemText.ToLower() != "select") Collection.Error("\"SELECT\" clouse is not found", lex);
                    SelectParser sp = new SelectParser();
                    sp.SelectExpresion = extS.Select;
                    sp.Parse(collection);
                    SelectExpresion.ExtSelects.Add(extS);
                }
                else break;
            }
        }
    }
}
