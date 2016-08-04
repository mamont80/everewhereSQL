using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ParserCore
{
    /*
    public class SelectLexemToExpression
    {
        public ExpressionFactoryTable Factory = new ExpressionFactoryTable();
        public ExpressionLexemToNode LexemToNode;
        public SelectExpresion SelectResult;
        public User CurrentUser;
        public void Run(LexExprCollection lexems)
        {
            LexemToNode = new ExpressionLexemToNode();
            LexemToNode.NodeFactory = Factory;
            LexemToNode.Init(lexems);
            Run();
        }

        public void Run()
        {
            if (LexemToNode.CurrentLexem().Lexem.ToLower() == "select")
            {
                SelectResult = new SelectExpresion();
                SelectResult.Query = new GmSqlQuery();
                ParseSelect(SelectResult);
                SelectResult.Query.Prepare();
                return;
            }
        }


        private void ParseSelect(SelectExpresion selectExpr)
        {
            LexemToNode.IndexLexem++;//пропускаем select
            //считываем колонки для выборки
            ReadColumns(selectExpr);
            if (LexemToNode.CurrentLexem() == null) return;
            if (LexemToNode.CurrentLexem().Lexem.ToLower() == "from")
            {
                LexemToNode.IndexLexem++;
                ReadTableReferences(selectExpr);
            }
            TablesFromQueryToFactory(selectExpr);
            CapFieldToFieldExpr(selectExpr);
            if (LexemToNode.CurrentLexem() == null) return;
            if (LexemToNode.CurrentLexem().Lexem.ToLower() == "where")
            {
                LexemToNode.IndexLexem++;
                var copy = LexemToNode.Clone();

                copy.StopOnSecondValue = true;
                copy.StopWords = StopWhere;
                var whereExpr = copy.GetExpr();
                LexemToNode.IndexLexem = copy.IndexLexem;
                selectExpr.Query.SetWhere(whereExpr);
            }
            if (LexemToNode.CurrentLexem() == null) return;
            if (LexemToNode.CurrentLexem().Lexem.ToLower() == "group")
            {
                LexemToNode.IndexLexem++;
                if (LexemToNode.CurrentLexem() == null) throw new Exception("Не правильно записан GROUP BY");
                if (LexemToNode.CurrentLexem().Lexem.ToLower() != "by") throw new Exception("Пропущено \"BY\" в конструкции GROUP BY");
                LexemToNode.IndexLexem++;
                ReadGroupByColumns(selectExpr);
            }
            if (LexemToNode.CurrentLexem() == null) return;
            if (LexemToNode.CurrentLexem().Lexem.ToLower() == "having")
            {
                LexemToNode.IndexLexem++;
                var copy = LexemToNode.Clone();

                copy.StopOnSecondValue = true;
                copy.StopWords = StopWhere;
                var whereExpr = copy.GetExpr();
                LexemToNode.IndexLexem = copy.IndexLexem;
                selectExpr.Query.Having = whereExpr;
            }
            if (LexemToNode.CurrentLexem() == null) return;
            if (LexemToNode.CurrentLexem().Lexem.ToLower() == "order")
            {
                LexemToNode.IndexLexem++;
                if (LexemToNode.CurrentLexem() == null) throw new Exception("Не правильно записан GROUP BY");
                if (LexemToNode.CurrentLexem().Lexem.ToLower() != "by") throw new Exception("Пропущено \"BY\" в конструкции GROUP BY");
                LexemToNode.IndexLexem++;
                ReadOrderByColumns(selectExpr);
            }
            if (LexemToNode.CurrentLexem() == null) return;

            while (ReadNextPart(selectExpr)){}
        }

        private bool ReadNextPart(SelectExpresion selectExpr)
        {
            if (LexemToNode.CurrentLexem() == null) return false;
            if (LexemToNode.CurrentLexem().Lexem.ToLower() == "limit")
            {
                LexemToNode.IndexLexem++;
                var copy = LexemToNode.Clone();

                copy.StopWords = StopWhere;
                var whereExpr = copy.GetExpr();
                LexemToNode.IndexLexem = copy.IndexLexem;
                whereExpr = whereExpr.PrepareAndOptimize();
                if (!(whereExpr is ConstExpr)) throw new Exception("Limit is not constant");
                selectExpr.Query.LimitRecords = whereExpr.GetIntResultOut(null);
                return true;
            }
            if (LexemToNode.CurrentLexem().Lexem.ToLower() == "offset")
            {
                LexemToNode.IndexLexem++;
                var copy = LexemToNode.Clone();

                copy.StopWords = StopWhere;
                var whereExpr = copy.GetExpr();
                LexemToNode.IndexLexem = copy.IndexLexem;
                whereExpr = whereExpr.PrepareAndOptimize();
                if (!(whereExpr is ConstExpr)) throw new Exception("Offset is not constant");
                selectExpr.Query.SkipRecords = whereExpr.GetIntResultOut(null);
                return true;
            }
            return false;
        }

        private HashSet<string> WaitFrom = new HashSet<string>() { "from", "as" };
        private HashSet<string> StopFrom = new HashSet<string>() { "where", "group", "order", "having", "limit", "offset" };
        private HashSet<string> joinStrings = new HashSet<string>() { "inner", "join", "left", "right", "full", "cross"};
        private HashSet<string> StopWhere = new HashSet<string>() { "group", "order", "having", "limit", "offset" };
        private HashSet<string> StopGroup = new HashSet<string>() { "order", "having", "limit", "offset", "desc", "asc" };

        private void ReadTableReferences(SelectExpresion selectExpr)
        {
            if (selectExpr.Query.Tables == null) selectExpr.Query.Tables = new List<SelectTable>();
            SelectTable sel = null;
            while (true)
            {
                if (LexemToNode.CurrentLexem() == null) break;
                if (StopFrom.Contains(LexemToNode.CurrentLexem().Lexem.ToLower())) break;
                string tableName = ReadAlias(LexemToNode);
                VectorLayer vl = LayersCache.GetLayer(tableName) as VectorLayer;
                if (vl != null)
                {
                    if (!UserAccess.CanUserViewLayer(CurrentUser, vl)) vl = null;
                }
                if (vl == null) throw new Exception("Layer "+tableName+" is not found");
                if (sel == null) sel = SelectTable.Create(vl);
                selectExpr.Query.Tables.Add(sel);
                string t;
                //t = LexemToNode.CurrentLexem().Lexem.ToLower();
                //if (StopFrom.Contains(t)) break;
                
                //LexemToNode.IndexLexem++;
                if (LexemToNode.CurrentLexem() == null) break;
                if (LexemToNode.CurrentLexem().Lexem.ToLower() == "as")
                {
                    LexemToNode.IndexLexem++;
                    if (LexemToNode.CurrentLexem() == null) throw new Exception("Alias not found");
                    sel.Alias = ReadAlias(LexemToNode);
                }
                else
                {
                    t = LexemToNode.CurrentLexem().Lexem.ToLower();
                    if (!StopFrom.Contains(t) && !joinStrings.Contains(t) &&
                        (LexemToNode.CurrentLexem().LexemType == LexType.Command || LexemToNode.CurrentLexem().LexemType == LexType.Text))
                        sel.Alias = ReadAlias(LexemToNode);
                }
                if (LexemToNode.CurrentLexem() == null) break;
                t = LexemToNode.CurrentLexem().Lexem.ToLower();
                if (StopFrom.Contains(t)) break;
                if (LexemToNode.CurrentLexem().LexemType == LexType.Zpt)
                {
                    LexemToNode.IndexLexem++;
                    sel = null;
                    continue;
                }
                if (joinStrings.Contains(t))
                {
                    sel = new SelectTable();
                    ReadJoin(sel, LexemToNode);
                }
                else break;
                selectExpr.Query.Tables.Add(sel);
            }
        }

        private void TablesFromQueryToFactory(SelectExpresion selectExpr)
        {
            Factory.Tables = selectExpr.Query.Tables;
        }

        private void CapFieldToFieldExpr(SelectExpresion selectExpr)
        {
            for (int i = 0; i < selectExpr.Query.Columns.Count; i++)
            {
                var col = selectExpr.Query.Columns[i];
                RecursiveFieldCap(col.ColumnExpression);
            }
        }

        private void RecursiveFieldCap(Expression expr)
        {
            if (expr.Childs != null)
            {
                foreach (var e in expr.Childs)
                {
                    RecursiveFieldCap(e);
                }
            }
            if (expr is FieldCapExpr)
            {
                FieldCapExpr fc = expr as FieldCapExpr;
                //expr.AddChild(Factory.AddFiled(fc.FieldAlias, fc.TableAlias));
            }
        }

        private void ReadJoin(SelectTable sel, ExpressionLexemToNode LexemToNode)
        {
            //+left join
            //+left outer join
            //+inner join
            //+cross join
            //+join
            //+right join
            //+right outer join
            // full join
            string s1 = LexemToNode.CurrentLexem().Lexem.ToLower();
            string s2 = null;
            string s3 = null;
            if (LexemToNode.SeekLexem(1) != null && joinStrings.Contains(LexemToNode.SeekLexem(1).Lexem))
            {
                s2 = LexemToNode.SeekLexem(1).Lexem;
                LexemToNode.IndexLexem++;
                if (LexemToNode.SeekLexem(1) != null && joinStrings.Contains(LexemToNode.SeekLexem(1).Lexem))
                {
                    s3 = LexemToNode.SeekLexem(1).Lexem;
                    LexemToNode.IndexLexem++;
                }
            }
            if ((s3 == null && s2 == null && s1 == "join") ||
                (s3 == null && s1 == "cross" && s2 == "join"))
            {
                sel.Join = JoinType.Cross;
            }else
            if ((s3 == null && s1 == "left" && s2 == "join") ||
                (s1 == "left" && s2 == "outer" && s3 == "join"))
                sel.Join = JoinType.Left;
            else
                if ((s3 == null && s1 == "right" && s2 == "join") ||
                    (s1 == "right" && s2 == "outer" && s3 == "join"))
                    sel.Join = JoinType.Left;
                else if (s3 == null && s1 == "inner" && s2 == "join")
                    sel.Join = JoinType.Inner;
                else if (s3 == null && s1 == "full" && s2 == "join")
                    sel.Join = JoinType.Full;
                else throw new Exception("Unknow JOIN description");
        }

        private void ReadColumns(SelectExpresion selectExpr)
        {
            //колнка может заканчиваться:
            // select "Column" from
            // select "Column")
            // select "Column" as "aa"
            // select "Column" as aa
            // select "Column" "aa"
            // select "Column" aa
            // select "Column",
            Factory.FieldCap = true;
            ExpressionLexemToNode copy = null;
            try
            {
                if (selectExpr.Query.Columns == null) selectExpr.Query.Columns = new List<ColumnClause>();
                while (true)
                {
                    copy = LexemToNode.Clone();

                    copy.StopOnSecondValue = true;
                    copy.StopOnZpt = true;
                    copy.StopWords = WaitFrom;
                    var colExpr = copy.GetExpr();
                    LexemToNode.IndexLexem = copy.IndexLexem;
                    ColumnClause cs = new ColumnClause();
                    cs.ColumnExpression = colExpr;
                    selectExpr.Query.Columns.Add(cs);

                    if (copy.HasStopOnWord)
                    {
                        if (LexemToNode.CurrentLexem().Lexem.ToLower() == "as")
                        {
                            LexemToNode.IndexLexem++;
                            cs.Alias = ReadAlias(LexemToNode);
                        }
                    }else
                    if (copy.HasStopOnSecondValue)
                    {
                        cs.Alias = ReadAlias(LexemToNode);
                    }
                    if (LexemToNode.CurrentLexem().Lexem.ToLower() == "from") break;
                    LexemToNode.IndexLexem++;
                }
            }
            finally
            {
                Factory.FieldCap = false;
            }
        }

        private string ReadAlias(ExpressionLexemToNode ltn)
        {
            if (ltn.CurrentLexem() == null) throw new Exception("Alias not found");
            if (ltn.CurrentLexem().LexemType == LexType.Command)
            {
                var r = ltn.CurrentLexem().Lexem;
                ltn.IndexLexem++;
                return r;
            }
            if (ltn.CurrentLexem().LexemType == LexType.Text)
            {
                var strs = ExpressionFactory.ParseStringQuote(ltn.CurrentLexem().Lexem);
                if (strs.Length != 1) throw new Exception("Составной алиас");
                ltn.IndexLexem++;
                return strs[0];
            }
            throw new Exception("Алиас клонки не найден");
        }

        private void ReadOrderByColumns(SelectExpresion selectExpr)
        {
            var lst = ReadCustomOrderColumns();
            selectExpr.Query.OrderBys = lst;
        }
        private void ReadGroupByColumns(SelectExpresion selectExpr)
        {
            var lst = ReadCustomOrderColumns();
            selectExpr.Query.GroupBys = lst;
        }

        private List<GroupBy> ReadCustomOrderColumns()
        {
            //колнка может заканчиваться:
            // select "Column",
            List<GroupBy> res = new List<GroupBy>();
            ExpressionLexemToNode copy = null;
            while (true)
            {
                copy = LexemToNode.Clone();

                copy.StopOnZpt = true;
                copy.StopWords = StopGroup;
                var colExpr = copy.GetExpr();
                LexemToNode.IndexLexem = copy.IndexLexem;
                GroupBy gb = new GroupBy();
                gb.Expression = colExpr;
                res.Add(gb);

                if (copy.HasStopOnWord)
                {
                    string s = LexemToNode.CurrentLexem().Lexem.ToLower();
                    switch (s)
                    {
                        case "asc":
                            LexemToNode.IndexLexem++;
                            break;
                        case "desc":
                            gb.Sort = SortType.DESC;
                            LexemToNode.IndexLexem++;
                            break;
                    }
                }
                if (LexemToNode.CurrentLexem() == null) return res;
                if (LexemToNode.CurrentLexem().LexemType == LexType.Zpt)
                {
                    LexemToNode.IndexLexem++;
                    continue;
                }

                string s1 = LexemToNode.CurrentLexem().Lexem.ToLower();
                if (copy.StopWords.Contains(s1)) return res;
            }
        }

    }*/
}
