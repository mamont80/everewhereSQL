using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data.Common;
using System.Text;


namespace ParserCore
{
    public class GmSqlQuery
    {
        public long LimitRecords = -1;
        public long SkipRecords = 0;
        public int Timeout = 0;
        public bool Distinct = false;
        public List<SelectTable> Tables = new List<SelectTable>();
        public List<ColumnClause> Columns = new List<ColumnClause>();
        public List<GroupBy> GroupBys = new List<GroupBy>();
        public Expression Having;
        public List<GroupBy> OrderBys = new List<GroupBy>();

        public Expression WhereExpr;
        public IDbDriver Driver { get; set; }

        protected ExpressionFactoryTable ExprFactory;

        public string OrderByColumn;
        public bool OrderDesc;

        private string _VariablePrefix = "prm";
        public string VariablePrefix { get { return _VariablePrefix; }
            set { _VariablePrefix = value; }
        }
        
        public void Prepare()
        {
            ExprFactory = new ExpressionFactoryTable();
            List<SelectTable> tempTbl = new List<SelectTable>();

            //подготовка таблиц
            Driver = null;
            for (int i = 0; i < Tables.Count; i++)
            {
                SelectTable st = Tables[i];
                if (st.IsSubSelect())
                {
                    st.TableAsSubSelect().Select.Query.Prepare();
                    st.TableAsSubSelect().DbDriver = st.TableAsSubSelect().Select.Query.Driver;
                }
                if (Driver == null)
                {
                    Driver = st.Table.DbDriver;
                }
                else
                {
                    if (Driver != (st.Table.DbDriver))
                    {
                        throw new Exception("Tables are in different databases");
                    }
                }
                tempTbl.Add(st);
                
                if (st.OnExpression != null) st.OnExpression = st.OnExpression.PrepareAndOptimize();
                
            }
            // TODO: fix ? DefaultDbDriver
            /*if (Driver == null) Driver = Db.DefaultDbDriver;
            */
            
            //ExprFactory.Tables = Tables;
            //подготовка колонок
            if (Columns == null || Columns.Count == 0)
            {
                List<ColumnClause> lst = new List<ColumnClause>();
                for (int i = 0; i < Tables.Count; i++)
                {
                    SelectTable st = Tables[i];
                    foreach (var c in st.Table.Columns)
                    {
                        ColumnClause cs = new ColumnClause();
                        FieldExpr fe = new FieldExpr();
                        fe.FieldName = c.Name;
                        fe.Table = st;
                        fe.Init(c.SimpleType, st.Table.CoordinateSystem);
                        cs.ColumnExpression = fe;
                        lst.Add(cs);
                    }
                }
                Columns = lst;
            }
            else
            {
                for (int i = 0; i < Columns.Count; i++)
                {
                    ColumnClause cs = Columns[i];
                    if (cs.ColumnExpression != null) cs.ColumnExpression = cs.ColumnExpression.PrepareAndOptimize();
                }
            }
            //Защита от получения GMX_RasterCatalogID
            for (int i = 0; i < Columns.Count; i++)
            {
                List<Expression> lst = Columns[i].ColumnExpression.GetAllExpressions();
                foreach (var exp in lst)
                {
                    if (exp is FieldExpr)
                    {
                        FieldExpr fe = exp as FieldExpr;
                        // TODO: FIX for geomixer
                        /*
                        if (fe.FieldName.ToLower() == GeometryFeature.ColumnRcLayerNameLower && lst.Count > 1)
                        {//нельзя использовать ColumnRcLayerName в составном выражении
                            throw new Exception(string.Format("Can not use column {0} in complex column expression", GeometryFeature.ColumnRcLayerName));
                        }*/
                    }
                }
            }


            if (GroupBys != null && GroupBys.Count > 0)
            {
                //подготовка group by. В колонках пока не поддерживаются агрегатные функции
                for (int i = 0; i < GroupBys.Count; i++)
                {
                    GroupBy gb = GroupBys[i];
                    if (gb.Expression != null) gb.Expression = gb.Expression.PrepareAndOptimize();
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
            if (WhereExpr != null) WhereExpr = WhereExpr.PrepareAndOptimize();
            if (Having != null) Having = Having.PrepareAndOptimize();
            if (OrderBys != null && OrderBys.Count > 0)
            {
                //подготовка group by. В колонках пока не поддерживаются агрегатные функции
                for (int i = 0; i < OrderBys.Count; i++)
                {
                    GroupBy gb = OrderBys[i];
                    if (gb.Expression != null) gb.Expression = gb.Expression.PrepareAndOptimize();
                }
            }
        }

        private bool InternalFindGeometry(SelectTable st, Expression exp, bool wasSpatialOperation = false)
        {
            bool spatOp = wasSpatialOperation;
            if (exp == null) return false;
            if (exp is FieldExpr)
            {
                if (wasSpatialOperation && (exp as FieldExpr).Table == st && (exp as FieldExpr).GetResultType() == SimpleTypes.Geometry)
                {
                    return true;
                }
            }
            else
            {
                if (exp is IOperationForSpatialIndex)
                {
                    spatOp = true;
                }
                bool ok = false;
                for (int i = 0; i < exp.ChildsCount(); i++)
                {
                    ok = InternalFindGeometry(st, exp.GetChild(i), spatOp);
                    if (ok) return ok;
                }
            }
            return false;
        }

        public string ToStr()
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("select ");
            if (Distinct) sql.Append("DISTINCT ");
            for (int i = 0; i < Columns.Count; i++)
            {
                var cs = Columns[i];
                if (i != 0) sql.Append(", ");
                sql.Append(cs.ColumnExpression.ToStr());
                if (!string.IsNullOrEmpty(cs.Alias)) sql.Append(" as \"").Append(cs.Alias).Append("\"");
            }
            if (Tables.Count > 0)
            {
                sql.Append(" from "+Tables[0].ToStr());
                if (!string.IsNullOrEmpty(Tables[0].Alias)) sql.Append(" as \"").Append(Tables[0].Alias).Append("\"");
                for (int i = 1; i < Tables.Count; i++)
                {
                    var t = Tables[i];
                    if (t.Join == JoinType.Cross) sql.Append(" cross join ");
                    if (t.Join == JoinType.Inner) sql.Append(" inner join ");
                    if (t.Join == JoinType.Full) sql.Append(" full join ");
                    if (t.Join == JoinType.Left) sql.Append(" left join ");
                    if (t.Join == JoinType.Right) sql.Append(" right join ");
                    sql.Append(" ").Append(t.ToStr());
                    if (!string.IsNullOrEmpty(t.Alias)) sql.Append(" as \"").Append(t.Alias).Append("\"");
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
                    sql.Append(g.Expression.ToStr());
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
                    sql.Append(g.Expression.ToStr());
                    if (g.Sort != SortType.ASC) sql.Append(" desc");
                }
            }
            if (LimitRecords >= 0) sql.Append(" limit " + LimitRecords.ToString());
            if (SkipRecords > 0) sql.Append(" offset " + SkipRecords.ToString());
            return sql.ToString();
        }

        public string MakeSelect()
        {
            return MakeSelectExpression(null);
        }

        public string MakeSelectExpression(ExpressionSqlBuilder builder)
        {
            if (builder == null)
            {
                builder = new ExpressionSqlBuilder();
                builder.Driver = Driver;
            }
            return MakeSelect(false, builder);
        }

        protected string MakeSelect(bool forCount, ExpressionSqlBuilder builder)
        {

            // TODO Fix this function.
            
            if (builder == null)
            {
                builder = new ExpressionSqlBuilder();
                builder.Driver = Driver;
            }
            
            if (builder.Driver == null) builder.Driver = Driver;
            else
            {
                if (builder.Driver != Driver) throw new Exception("Table from different databases");
            }

            for (int j = 0; j < Tables.Count; j++)
            {
                Tables[j].InternalAlias = "tab"+builder.InternalTableAliasCounter.ToString();
                builder.InternalTableAliasCounter++;
            }

            //добавляем колонки
            StringBuilder sqlColumns = new StringBuilder();
            HashSet<string> uniqCols = new HashSet<string>();

            foreach (var cs in Columns)
            {
                cs.DbAlias = cs.ExtractAlias();
                if (!string.IsNullOrEmpty(cs.DbAlias))
                {
                    if (uniqCols.Contains(cs.DbAlias))
                    {
                        throw new Exception("Column " + cs.DbAlias + " is not unique");
                    }
                    uniqCols.Add(cs.DbAlias);
                }
            }
            int i = 0;
            foreach (var cs in Columns)
            {
                if (string.IsNullOrEmpty(cs.DbAlias))
                {
                    while (true)
                    {
                        string nm = "col" + i.ToString();
                        if (uniqCols.Contains(nm))
                        {
                            i++;
                            continue;
                        }
                        cs.DbAlias = nm;
                        uniqCols.Add(cs.DbAlias);
                        break;
                    }
                }
            }
            i = 0;
            foreach (var cs in Columns)
            {
                if (i != 0) sqlColumns.Append(", ");
                sqlColumns.Append(cs.ColumnExpression.ToSQL(builder));


                sqlColumns.Append(" as ").Append(BaseExpressionFactory.TableSqlCodeEscape(cs.DbAlias)).Append(" ");
                i++;
            }

            StringBuilder sqlTables = new StringBuilder();
            if (Tables.Count > 0) sqlTables.Append("from ");
            i = 0;
            foreach (SelectTable st in Tables)
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

                sqlTables.Append(st.ToSql(builder)).Append(" as ").Append(BaseExpressionFactory.TableSqlCodeEscape(st.InternalAlias)).Append(" ");


                if (i != 0 && st.Join != JoinType.Cross)
                {
                    sqlTables.Append(" ON ").Append(st.OnExpression.ToSQL(builder));
                }
                sqlTables.Append(" ");
                i++;
            }
            //sqlSb.Append(" ");
            string sqlWhere = "";
            if (WhereExpr != null)
            {
                sqlWhere = " where ";
                sqlWhere += WhereExpr.ToSQL(builder);
            }
            StringBuilder sqlGroups = new StringBuilder();
            if (GroupBys != null && GroupBys.Count > 0)
            {
                sqlGroups.Append(" group by ");
                i = 0;
                foreach (GroupBy groupBy in GroupBys)
                {
                    if (i != 0) sqlGroups.Append(", ");
                    sqlGroups.Append(groupBy.Expression.ToSQL(builder));
                    i++;
                }
            }
            string orderby = "";
            ColumnClause orderByColumn = null;
            if (!string.IsNullOrEmpty(OrderByColumn))
            {
                ColumnClause[] acs = Columns.Where(a => GetVisibleColumnName(a).ToLower() == OrderByColumn.ToLower()).ToArray();
                if (acs.Length == 0) throw new Exception("Column for sort not found");
                if (acs.Length > 1) throw new Exception("Compliance with no-one between the names of the columns to sort.");
                orderByColumn = acs[0];
                orderby = "order by [" + orderByColumn.DbAlias+"]";
                if (OrderDesc) orderby = orderby + " DESC";
            }

            string sql = "";
            if (forCount)
            {
                return string.Format("select {0} {1} {2} {3}", "count(*)", sqlTables.ToString(), sqlWhere, sqlGroups.ToString());
            }
            if (SkipRecords == 0 && LimitRecords < 0)
            {
                return string.Format("select {5}{0} {1} {2} {3} {4}", sqlColumns.ToString(), sqlTables.ToString(), sqlWhere, sqlGroups.ToString(), orderby, 
                    Distinct ? " distinct " : "");
            }
            //вместе с ограничениями
            if (Driver.DbDriverType == DbDriverType.SqlServer)
            {
                if (string.IsNullOrEmpty(orderby)) orderby = "ORDER BY (SELECT null)";

                StringBuilder aliasesCol = new StringBuilder();
                for (int i2 = 0; i2 < Columns.Count; i2++)
                {
                    if (i2 != 0) aliasesCol.Append(", ");
                    aliasesCol.Append(BaseExpressionFactory.TableSqlCodeEscape(Columns[i2].DbAlias) );
                }

                sql = string.Format(@"
select {0} from(
select {0}, Row_Number()  OVER ({1}) as ""num19376194i9""
from
(SELECT {6} {2} {3} {4} {5}
  ) as t1)as t2 where 1=1 ", aliasesCol.ToString(), orderby, sqlColumns.ToString(), sqlTables.ToString(), sqlWhere, sqlGroups.ToString(), Distinct ? " distinct " : "");
                if (SkipRecords > 0)
                    sql += " and \"num19376194i9\" > " + SkipRecords.ToString();
                if (LimitRecords >= 0)
                    sql += " and \"num19376194i9\" <= " + (LimitRecords + SkipRecords).ToString();
            }
            if (Driver.DbDriverType == DbDriverType.PostgreSQL)
            {
                sql = string.Format("select {5} {0} {1} {2} {3} {4}", sqlColumns.ToString(), sqlTables.ToString(), sqlWhere, sqlGroups.ToString(), orderby, Distinct ? " distinct " : "");
                if (SkipRecords > 0)
                    sql += "  OFFSET " + SkipRecords.ToString();
                if (LimitRecords >= 0)
                    sql += " LIMIT " + LimitRecords.ToString();
            }
            return sql;
             
        }

        /// <summary>
        /// Возвращает описание колонок
        /// </summary>
        public Column[] GetColumnInfo()
        {
            List<Column> res = new List<Column>();
            foreach (var cs in Columns)
            {
                string nm = GetVisibleColumnName(cs);
                Column ci = new Column(nm, cs.ColumnExpression.GetResultType());
                res.Add(ci);
            }
            return res.ToArray();
        }

        protected string GetVisibleColumnName(ColumnClause cs)
        {
            string nm = cs.Alias;
            if (string.IsNullOrEmpty(nm))
            {
                if (cs.ColumnExpression is FieldExpr)
                {
                    nm = ((FieldExpr)cs.ColumnExpression).FieldName;
                }
                else nm = cs.ColumnExpression.ToStr();
            }
            return nm;
        }


    }
}
