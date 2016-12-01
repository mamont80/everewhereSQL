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

        public SelectParser(LexemCollection collection)
            : base(collection)
        {
        }
        public override void Parse()
        {
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
            var idx = Collection.IndexLexem;
            ColumnClauseParser colsParser = new ColumnClauseParser();
            colsParser.Parse(Collection);
            if (colsParser.Columns.Count == 0) Collection.Error("Columnn not found", Collection.Get(idx));
            Results.Add(SelectExpresion);
            SelectExpresion.Columns.Replace(colsParser.Columns);
            var lex = Collection.CurrentLexem();
            if (lex == null) return;
            if (lex.LexemText.ToLower() != "from") return;

            lex = Collection.GotoNext();
            FromParser fc = new FromParser();
            fc.Parse(Collection);
            SelectExpresion.Tables.Replace(fc.Tables);
            lex = Collection.CurrentLexem();
            if (lex == null) return;
            if (lex.LexemText.ToLower() == "where")
            {
                lex = Collection.GotoNext();
                if (lex == null) Collection.Error("Where clause not found", Collection.GetPrev());
                idx = Collection.IndexLexem;
                ExpressionParser tonode = new ExpressionParser(Collection);
                tonode.Parse();
                if (tonode.Results.Count != 1) Collection.Error("не верное число параметров", Collection.Get(idx));
                SelectExpresion.WhereExpr = tonode.Single();
            }
            lex = Collection.CurrentLexem();
            if (lex == null) return;
            if (lex.LexemText.ToLower() == "having")
            {
                lex = Collection.GotoNext();
                if (lex == null) Collection.Error("Having clause not found", Collection.GetPrev());
                idx = Collection.IndexLexem;
                ExpressionParser tonode = new ExpressionParser(Collection);
                tonode.Parse();
                if (tonode.Results.Count != 1) Collection.Error("не верное число параметров", Collection.Get(idx));
                SelectExpresion.Having = tonode.Single();
            }
            lex = Collection.CurrentLexem();
            if (lex == null) return;
            if (lex.LexemText.ToLower() == "group")
            {
                lex = Collection.GotoNext();
                if (lex == null) Collection.Error("Group by clause error", Collection.GetPrev());
                if (lex.LexemText.ToLower() != "by") Collection.Error("Group by clause error", Collection.GetPrev());
                lex = Collection.GotoNext();
                OrderByParser gb = new OrderByParser();
                gb.Parse(Collection, false);
                SelectExpresion.GroupBys.Replace(gb.Columns);
                if (SelectExpresion.GroupBys.Count == 0) Collection.Error("\"Group by\" columns not found", Collection.Get(idx));
                lex = Collection.CurrentLexem();
            }
            lex = Collection.CurrentLexem();
            if (lex == null) return;
            if (lex.LexemText.ToLower() == "order")
            {
                lex = Collection.GotoNext();
                if (lex == null) Collection.Error("Order by clause error", Collection.GetPrev());
                if (lex.LexemText.ToLower() != "by") Collection.ErrorWaitKeyWord("BY", Collection.GetPrev());
                lex = Collection.GotoNext();
                OrderByParser gb = new OrderByParser();
                gb.Parse(Collection, true);
                SelectExpresion.OrderBys.Replace(gb.Columns.Select(a => (OrderByClause)a).ToList());
                if (SelectExpresion.OrderBys.Count == 0) Collection.Error("\"Order by\" columns not found", Collection.Get(idx));
                lex = Collection.CurrentLexem();
            }
            lex = Collection.CurrentLexem();
            if (lex == null) return;
            if (lex.LexemText.ToLower() == "limit")
            {
                lex = Collection.GotoNext();
                if (lex == null) Collection.Error("Limit clause not found", Collection.GetPrev());
                ExpressionParser tonode = new ExpressionParser(Collection);
                tonode.Parse();
                SelectExpresion.LimitRecords = tonode.Single().GetIntResultOut(null);
            }
            lex = Collection.CurrentLexem();
            if (lex == null) return;
            if (lex.LexemText.ToLower() == "offset")
            {
                lex = Collection.GotoNext();
                if (lex == null) Collection.Error("Offset clause not found", Collection.GetPrev());
                ExpressionParser tonode = new ExpressionParser(Collection);
                tonode.Parse();
                SelectExpresion.SkipRecords = tonode.Single().GetIntResultOut(null);
            }
            while (true)
            {
                lex = Collection.CurrentLexem();
                if (lex == null) return;
                string s = lex.LexemText.ToLower();
                if (s == "union" || s == "intersect" || s == "except")
                {
                    lex = Collection.GotoNextMust();
                    ExtSelectClause extS = new ExtSelectClause();
                    extS.Select = new SelectExpresion();
                    if (s == "union" && lex.LexemText.ToLower() == "all")
                    {
                        extS.Operation = SelectOperation.UnionAll;
                        lex = Collection.GotoNextMust();
                    }
                    else if (s == "union") extS.Operation = SelectOperation.Union;
                    if (s == "intersect") extS.Operation = SelectOperation.Intersect;
                    if (s == "except") extS.Operation = SelectOperation.Except;
                    if (lex.LexemText.ToLower() != "select") Collection.Error("\"SELECT\" clouse is not found", lex);
                    SelectParser sp = new SelectParser(Collection);
                    sp.SelectExpresion = extS.Select;
                    sp.Parse();
                    SelectExpresion.ExtSelects.Add(extS);
                }
                else break;
            }
        }
    }
}
