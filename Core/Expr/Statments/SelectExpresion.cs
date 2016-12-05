using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.Sql
{
    public class SelectExpresion : CustomStatement, ITableDesc, ITableSource, ISelect
    {
        public long LimitRecords = -1;

        public long SkipRecords = 0;

        public bool Distinct = false;

        public TokenList<TableClause> Tables { get; private set; }
        public TokenList<ColumnClause> Columns { get; private set; }
        public TokenList<GroupByClause> GroupBys { get; private set; }
        public TokenList<OrderByClause> OrderBys { get; private set; }

        private Expression _Having;

        public Expression Having
        {
            get { return _Having; }
            set
            {
                _Having = value;
                if (value != null) value.ParentToken = this;
            }
        }

        private Expression _WhereExpr;
        public Expression WhereExpr
        {
            get { return _WhereExpr; }
            set
            {
                _WhereExpr = value;
                if (value != null) value.ParentToken = this;
            }
        }

        /*
        private SelectClause _Query;

        public SelectClause Query
        {
            get { return _Query; }
            set
            {
                _Query = value;
                if (value != null) value.ParentToken = this;
            }
        }*/

        public TokenList<ExtSelectClause> ExtSelects;

        public SelectExpresion():base()
        {
            Tables = new TokenList<TableClause>(this);
            Columns = new TokenList<ColumnClause>(this);
            GroupBys = new TokenList<GroupByClause>(this);
            OrderBys = new TokenList<OrderByClause>(this);

            //Query = new SelectClause();
            TableColumns = new List<Column>();
            ExtSelects = new TokenList<ExtSelectClause>(this);
        }

        public TableClause[] GetTables()
        {
            return Tables.ToArray();
        }

        private void MakeColumnsAsLocalSubSelect()
        {
            TableColumns.Clear();

            List<ColumnClause> lst = GetAllColumns().ToList();
            HashSet<string> str = new HashSet<string>();
            foreach (var c in lst)
            {
                string alias = c.ExtractAlias();
                if (alias == null) alias = "";
                var tp = c.ColumnExpression.GetResultType();
                if (tp == SimpleTypes.Geometry)
                {
                    CoordinateSystem = c.ColumnExpression.GetCoordinateSystem();
                }
                TableColumns.Add(new ColumnSubSelect() { ColumnClause = c, Name = alias, SimpleType = tp });
            }
        }

        public override void Prepare()
        {
            base.Prepare();
            TableColumns.Clear();
            PrepareTables();
            PrepareColumns();
            MakeColumnsAsLocalSubSelect();
            PrepareInner();

            if (ExtSelects != null && ExtSelects.Count > 0)
            {
                foreach (var e in ExtSelects)
                {
                    e.Prepare();
                }
            }
            var lst = GetAllColumns();
            if (lst.Count == 1)
            {
                var tp = lst[0].ColumnExpression.GetResultType();
                SetResultType(tp);
            }
        }

        /// <summary>
        /// All columns, with extract '*' clause
        /// </summary>
        /// <returns></returns>
        public override List<ColumnClause> GetAllColumns()
        {
            List<ColumnClause> ColumnsLocal = new List<ColumnClause>();
            foreach (var cc in Columns)
            {
                if (cc.ColumnExpression is AllColumnExpr)
                {
                    //AllColumnExpr all = (AllColumnExpr) cc.ColumnExpression;
                    //all.Prepare();
                    ColumnsLocal.AddRange(((AllColumnExpr)cc.ColumnExpression).Columns);
                }
                else ColumnsLocal.Add(cc);
            }
            return ColumnsLocal;
        }

        private void PrepareTables()
        {
            //подготовка таблиц
            for (int i = 0; i < Tables.Count; i++)
            {
                TableClause st = Tables[i];
                st.Prepare();
            }
        }

        private void PrepareColumns()
        {
            if (Columns == null || Columns.Count == 0)
            {
                List<ColumnClause> lst = new List<ColumnClause>();
                for (int i = 0; i < Tables.Count; i++)
                {
                    TableClause st = Tables[i];
                    var tlist = st.Table.TableColumns;
                    foreach (var c in tlist)
                    {
                        ColumnClause cs = new ColumnClause();
                        FieldExpr fe = new FieldExpr();
                        fe.Bind(st, c.Name);
                        cs.ColumnExpression = fe;
                        lst.Add(cs);
                    }
                }
                Columns.Replace(lst);
            }

            for (int i = 0; i < Columns.Count; i++)
            {
                ColumnClause cs = Columns[i];
                cs.Prepare();
            }
        }

        private void PrepareInner()
        {

            List<ColumnClause> ColumnsLocal = GetAllColumns();
            HashSet<string> uniqCols = new HashSet<string>();
            int i2 = 0;
            foreach (var cs in ColumnsLocal)
            {
                string s = cs.ExtractAlias();
                if (!string.IsNullOrEmpty(s) && !uniqCols.Contains(s))
                {
                    cs.InternalDbAlias = s;
                }
                if (string.IsNullOrEmpty(cs.InternalDbAlias))
                {
                    while (true)
                    {
                        string nm = "col" + i2.ToString();
                        if (uniqCols.Contains(nm))
                        {
                            i2++;
                            continue;
                        }
                        cs.InternalDbAlias = nm;
                        uniqCols.Add(cs.InternalDbAlias);
                        break;
                    }
                }
            }


            if (GroupBys != null && GroupBys.Count > 0)
            {
                //подготовка group by. В колонках пока не поддерживаются агрегатные функции
                for (int i = 0; i < GroupBys.Count; i++)
                {
                    GroupByClause gb = GroupBys[i];
                    if (gb.Expression != null) gb.Expression.Prepare();
                }
                //сверяем выражения в groupby и колонках. Всё что в колонках должно быть groupby
                /*
                for (int i = 0; i < Columns.Length; i++)
                {
                    ColumnSelect cs = Columns[i];
                    string s1 = cs.ColumnExpressionStr.ToLower().Trim();
                    var arr = GroupBys.Where(a => a.ExpressionStr.ToLower().Trim() == s1).ToArray();
                    if (arr.Length == 0) throw new Exception(string.Format("Column {0} not found in groupby expression"));
                }*/
            }
            if (WhereExpr != null) WhereExpr.Prepare();
            if (Having != null) Having.Prepare();
            if (OrderBys != null && OrderBys.Count > 0)
            {
                //подготовка group by. В колонках пока не поддерживаются агрегатные функции
                for (int i = 0; i < OrderBys.Count; i++)
                {
                    OrderByClause gb = OrderBys[i];
                    if (gb.Expression != null) gb.Expression.Prepare();
                }
            }
        }


        public override string ToStr()
        {
            StringBuilder sql = new StringBuilder(DoToStr());

            if (ExtSelects != null && ExtSelects.Count > 0)
            {
                foreach (var e in ExtSelects)
                {
                    sql.Append(" " + e.ToStr());
                    
                }
            }
            return sql.ToString();
        }

        private string DoToStr()
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("select ");
            if (Distinct) sql.Append("DISTINCT ");
            for (int i = 0; i < Columns.Count; i++)
            {
                var cs = Columns[i];
                if (i != 0) sql.Append(", ");
                sql.Append(cs.ToStr());
            }
            if (Tables.Count > 0)
            {
                sql.Append(" from " + Tables[0].ToStr());
                //if (!string.IsNullOrEmpty(Tables[0].Alias)) sql.Append(" as \"").Append(Tables[0].Alias).Append("\"");
                for (int i = 1; i < Tables.Count; i++)
                {
                    var t = Tables[i];
                    if (t.Join == JoinType.Cross) sql.Append(" cross join ");
                    if (t.Join == JoinType.Inner) sql.Append(" inner join ");
                    if (t.Join == JoinType.Full) sql.Append(" full join ");
                    if (t.Join == JoinType.Left) sql.Append(" left join ");
                    if (t.Join == JoinType.Right) sql.Append(" right join ");
                    sql.Append(" ").Append(t.ToStr());
                    //if (!string.IsNullOrEmpty(t.Alias)) sql.Append(" as \"").Append(t.Alias).Append("\"");
                    if (t.OnExpression != null)
                    {
                        sql.Append(" on (").Append(t.OnExpression.ToStr()).Append(")");
                    }
                }
            }
            if (WhereExpr != null)
            {
                sql.Append(" where ").Append(WhereExpr.ToStr());
            }
            if (GroupBys != null && GroupBys.Count > 0)
            {
                sql.Append(" group by");
                for (int i = 0; i < GroupBys.Count; i++)
                {
                    var g = GroupBys[i];
                    if (i != 0) sql.Append(", ");
                    sql.Append(g.ToStr());
                }
            }
            if (Having != null)
            {
                sql.Append(" having ");
                sql.Append(Having.ToStr());
            }
            if (OrderBys != null && OrderBys.Count > 0)
            {
                sql.Append(" order by ");
                for (int i = 0; i < OrderBys.Count; i++)
                {
                    var g = OrderBys[i];
                    if (i != 0) sql.Append(", ");
                    sql.Append(g.ToStr());
                }
            }
            if (LimitRecords >= 0) sql.Append(" limit " + LimitRecords.ToString());
            if (SkipRecords > 0) sql.Append(" offset " + SkipRecords.ToString());
            return sql.ToString();
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            string s = DoToSql(builder);
            if (ExtSelects != null && ExtSelects.Count > 0)
            {
                foreach (var e in ExtSelects)
                {
                    s += e.ToSql(builder);
                }
            }
            return s;
        }

        private string DoToSql(ExpressionSqlBuilder builder)
        {

            //добавляем колонки
            StringBuilder sqlColumns = new StringBuilder();
            List<ColumnClause> ColumnsLocal = GetAllColumns();
            int i = 0;
            foreach (var cs in ColumnsLocal)
            {
                if (i != 0) sqlColumns.Append(", ");
                sqlColumns.Append(cs.ToSql(builder));
                //if (!string.IsNullOrEmpty(cs.InternalDbAlias)) sqlColumns.Append(" as ").Append(BaseExpressionFactory.TableSqlCodeEscape(cs.InternalDbAlias)).Append(" ");
                i++;
            }

            StringBuilder sqlTables = new StringBuilder();
            if (Tables.Count > 0) sqlTables.Append("from ");
            i = 0;
            foreach (TableClause st in Tables)
            {
                if (i != 0)
                {
                    switch (st.Join)
                    {
                        case JoinType.Cross:
                            sqlTables.Append(" CROSS JOIN ");
                            break;
                        case JoinType.Full:
                            sqlTables.Append(" FULL JOIN ");
                            break;
                        case JoinType.Left:
                            sqlTables.Append(" LEFT JOIN ");
                            break;
                        case JoinType.Right:
                            sqlTables.Append(" RIGHT JOIN ");
                            break;
                        case JoinType.Inner:
                            sqlTables.Append(" INNER JOIN ");
                            break;
                    }
                }

                sqlTables.Append(st.ToSql(builder));
                //if (!string.IsNullOrEmpty(st.Alias)) sqlTables.Append(" as ").Append(BaseExpressionFactory.TableSqlCodeEscape(st.Alias)).Append(" ");


                if (i != 0 && st.Join != JoinType.Cross)
                {
                    sqlTables.Append(" ON ").Append(st.OnExpression.ToSql(builder));
                }
                sqlTables.Append(" ");
                i++;
            }
            //sqlSb.Append(" ");
            string sqlWhere = "";
            if (WhereExpr != null)
            {
                sqlWhere = " where ";
                sqlWhere += WhereExpr.ToSql(builder);
            }
            StringBuilder sqlGroups = new StringBuilder();
            if (GroupBys != null && GroupBys.Count > 0)
            {
                sqlGroups.Append(" group by ");
                i = 0;
                foreach (GroupByClause groupBy in GroupBys)
                {
                    if (i != 0) sqlGroups.Append(", ");
                    sqlGroups.Append(groupBy.ToSql(builder));
                    i++;
                }
            }
            string orderby = "";
            if (OrderBys != null && OrderBys.Count > 0)
            {
                foreach (var ob in OrderBys)
                {
                    if (!string.IsNullOrEmpty(orderby)) orderby += ", ";
                    orderby += ob.ToSql(builder);
                }
                if (!string.IsNullOrEmpty(orderby)) orderby = " ORDER BY " + orderby;
            }

            string sql = "";
            //вместе с ограничениями

            if (builder.DbType == DriverType.SqlServer)
            {
                if (SkipRecords == 0)
                {
                    return string.Format("select {6}{5}{0} {1} {2} {3} {4}", sqlColumns.ToString(), sqlTables.ToString(), sqlWhere, sqlGroups.ToString(), orderby,
                        Distinct ? "distinct " : "",
                        LimitRecords > 0 ? "top " + LimitRecords.ToString() + " " : "");
                }

                if (string.IsNullOrEmpty(orderby)) orderby = "ORDER BY (SELECT null)";

                StringBuilder aliasesCol = new StringBuilder();
                for (int i2 = 0; i2 < ColumnsLocal.Count; i2++)
                {
                    if (i2 != 0) aliasesCol.Append(", ");
                    aliasesCol.Append(builder.EncodeTable(ColumnsLocal[i2].InternalDbAlias));
                }

                sql = string.Format(@"
select {0} from
(
   SELECT {6} {2}, Row_Number()  OVER ({1}) as ""num19376194i9"" {3} {4} {5}
)as t2 where ", aliasesCol.ToString(), orderby, sqlColumns.ToString(), sqlTables.ToString(), sqlWhere, sqlGroups.ToString(), Distinct ? " distinct " : "");
                sql += "\"num19376194i9\" > " + SkipRecords.ToString();
                if (LimitRecords >= 0)
                    sql += " and \"num19376194i9\" <= " + (LimitRecords + SkipRecords).ToString();
            }
            if (builder.DbType == DriverType.PostgreSQL)
            {

                sql = string.Format("select {5} {0} {1} {2} {3} {4}", sqlColumns.ToString(), sqlTables.ToString(), sqlWhere, sqlGroups.ToString(), orderby, Distinct ? " distinct " : "");
                if (SkipRecords > 0)
                    sql += "  OFFSET " + SkipRecords.ToString();
                if (LimitRecords >= 0)
                    sql += " LIMIT " + LimitRecords.ToString();
            }
            return sql;

        }
        public override IExplore Expolore(DelegateExpessionExplorer del)
        {
            List<TableClause> Tables2 = new List<TableClause>();
            Tables.ForEach(a =>
            {
                TableClause g2 = (TableClause)a.Expolore(del);
                if (g2 != null) Tables2.Add(g2);
            });
            Tables.Replace(Tables2);
            List<ColumnClause> Columns2 = new List<ColumnClause>();
            Columns.ForEach(a =>
            {
                ColumnClause c = (ColumnClause)a.Expolore(del);
                if (c != null) Columns2.Add(c);
            }
                );
            Columns.Replace(Columns2);
            if (WhereExpr != null) WhereExpr = (Expression)WhereExpr.Expolore(del);
            if (GroupBys != null)
            {
                List<GroupByClause> GroupBys2 = new List<GroupByClause>();
                GroupBys.ForEach(a =>
                {
                    GroupByClause g2 = (GroupByClause)a.Expolore(del);
                    if (g2 != null) GroupBys2.Add(g2);
                });
                GroupBys.Replace(GroupBys2);
            }
            if (Having != null) Having = (Expression)Having.Expolore(del);
            if (OrderBys != null)
            {
                List<OrderByClause> OrderBys2 = new List<OrderByClause>();
                OrderBys.ForEach(a =>
                {
                    OrderByClause g2 = (OrderByClause)a.Expolore(del);
                    if (g2 != null) OrderBys2.Add(g2);
                });
                OrderBys.Replace(OrderBys2);
            }




            if (ExtSelects != null && ExtSelects.Count > 0)
            {
                var ExtSelects2 = new TokenList<ExtSelectClause>(this);
                foreach (var e in ExtSelects)
                {
                    var t = (ExtSelectClause)e.Expolore(del);
                    if (t != null) ExtSelects2.Add(t);
                }
                ExtSelects = ExtSelects2;
            }
            return base.Expolore(del);
        }

        public override void ParseInside(ExpressionParser parser)
        {
            SelectParser selectParser = new SelectParser(parser.Collection);
            selectParser.SelectExpresion = this;
            selectParser.Parse();
        }

    }
}
