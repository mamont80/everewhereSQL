using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore.Expr.Sql;
using ParserCore.Expr.Simple;

namespace ParserCore
{
    internal class FromParser
    {
        public List<TableClause> Tables = new List<TableClause>();

        private HashSet<string> joinStrings = new HashSet<string>() { "inner", "join", "left", "right", "full", "cross" };
        private HashSet<string> nextStrings = new HashSet<string>() { "where", "group", "having", "order", "limit", "offset", "union", "except", "intersect" };
        
        public void Parse(LexemCollection collection)
        {
            int num = 0;
            while (true)
            {
                var lex = collection.CurrentLexem();
                if (lex == null) return;
                var st = Read_table_reference(collection, num == 0);
                num++;
                Tables.Add(st);
                lex = collection.CurrentLexem();
                if (lex == null) return;
                if (lex.LexemType == LexType.Zpt)
                {
                    lex = collection.GotoNext();
                    if (lex == null) collection.Error("Error in table clause", collection.GetPrev());
                    continue;
                }
                if (nextStrings.Contains(lex.LexemText.ToLower())) return;
                if (joinStrings.Contains(lex.LexemText.ToLower())) continue;
                return;
                //collection.Error("Error parsing", lex);
            }
        }

        private TableClause Read_table_reference(LexemCollection collection, bool isFirst)
        {
            var lex = collection.CurrentLexem();
            if (lex == null) return null;
            //+left join
            //+left outer join
            //+inner join
            //+cross join
            //+join
            //+right join
            //+right outer join
            // full join
            JoinType jt = JoinType.Cross;
            if (!isFirst)
            {
                string s1 = collection.CurrentLexem().LexemText.ToLower();
                string s2 = null;
                string s3 = null;
                if (joinStrings.Contains(s1))
                {
                    lex = collection.GotoNext();
                    if (lex != null && joinStrings.Contains(lex.LexemText.ToLower()))
                    {
                        s2 = lex.LexemText.ToLower();
                        lex = collection.GotoNext();
                        if (lex != null && joinStrings.Contains(lex.LexemText.ToLower()))
                        {
                            s3 = lex.LexemText.ToLower();
                            lex = collection.GotoNext();
                        }
                    }
                }
                
                if (!joinStrings.Contains(s1))// случай ","
                {
                    jt = JoinType.Cross;
                }else
                if ((s3 == null && s2 == null && s1 == "join") ||
                    (s3 == null && s1 == "cross" && s2 == "join"))
                {
                    jt = JoinType.Cross;
                }
                else if ((s3 == null && s1 == "left" && s2 == "join") ||
                         (s1 == "left" && s2 == "outer" && s3 == "join"))
                    jt = JoinType.Left;
                else if ((s3 == null && s1 == "right" && s2 == "join") ||
                         (s1 == "right" && s2 == "outer" && s3 == "join"))
                    jt = JoinType.Left;
                else if (s3 == null && s1 == "inner" && s2 == "join")
                    jt = JoinType.Inner;
                else if (s3 == null && s1 == "full" && s2 == "join")
                    jt = JoinType.Full;
                else collection.Error("Unknow JOIN clause", collection.CurrentLexem());
            }
            TableClause st = null;
            
            if (lex == null) collection.Error("Error in table clause", collection.GetPrev());
            if (!lex.IsSkobraOpen())
            {
                string[] tableName = CommonParserFunc.ReadTableName(collection);
                // TODO: fixed! ok
                var v = collection.TableGetter.GetTableByName(tableName);
                st = TableClause.CreateByTable(tableName, v);
                lex = collection.GotoNext();
            }
            else
            {
                lex = collection.GotoNext();
                if (lex == null) collection.Error("Expression is not found", collection.GetLast());
                //подзапрос
                var idx = collection.IndexLexem;
                ExpressionParser tonode = new ExpressionParser(collection);
                tonode.Parse();
                if (tonode.Results.Count != 1) collection.Error("не верное число параметров", collection.Get(idx));
                lex = collection.CurrentLexem();
                if (lex == null || !lex.IsSkobraClose()) collection.Error("Expression is not closed", collection.CurrentLexem());
                lex = collection.GotoNext();
                
                //ITableDesc subselect = CommonUtils.FindParentSelect(tonode.Single()); Это штука работает не правильно
                ITableDesc subselect = (ITableDesc)tonode.Single();
                if (subselect == null) collection.Error("Subselect not found", collection.Get(idx));
                st = TableClause.CreateBySelect(subselect);
                
            }
            st.Join = jt;
            if (lex == null) return st;
            if (lex.LexemText.ToLower() == "as")
            {
                collection.GotoNext();
                if (lex == null) collection.Error("Alias not found", collection.GetPrev());
                st.Alias = CommonParserFunc.ReadAlias(collection);
                if (st.Alias == null) collection.Error("Alias not found", collection.GetPrev());
                lex = collection.GotoNext();
            }
            else
            {
                if (lex.LexemType == LexType.Text)
                {
                    st.Alias = CommonParserFunc.ReadAlias(collection);
                    if (st.Alias == null) collection.Error("Alias not found", collection.GetPrev());
                    lex = collection.GotoNext();
                }
                else
                    if (lex.LexemType == LexType.Command && 
                        !nextStrings.Contains(lex.LexemText.ToLower()) && 
                        !joinStrings.Contains(lex.LexemText.ToLower()) &&
                        lex.LexemText.ToLower() != "on")
                    {
                        st.Alias = CommonParserFunc.ReadAlias(collection);
                        if (st.Alias == null) collection.Error("Alias not found", collection.GetPrev());
                        lex = collection.GotoNext();
                    }
            }
            lex = collection.CurrentLexem();
            if (lex == null) return st;
            if (lex.LexemType == LexType.Command && lex.LexemText.ToLower() == "on")
            {
                lex = collection.GotoNext();
                var idx = collection.IndexLexem;
                ExpressionParser tonode = new ExpressionParser(collection);
                tonode.Parse();
                if (tonode.Results.Count != 1) collection.Error("не верное число параметров", collection.Get(idx));
                st.OnExpression = tonode.Single();
            }
            return st;
        }

    }
}
