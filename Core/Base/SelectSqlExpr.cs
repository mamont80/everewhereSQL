using System;
using System.Data;

namespace ParserCore
{

    public enum JoinType
    {
        Left,
        Right,
        Cross,
        Full,
        Inner
    }


    public enum SortType
    {
        ASC,
        DESC
    }

    public class SelectTableJson
    {
        public string LayerName;
        public string Alias;
        public JoinType Join = JoinType.Cross;
        public string On;
    }

    public class SelectTable
    {
        private SelectTable()
        {
        }

        public string Alias;
        public JoinType Join = JoinType.Cross;
        /// <summary>
        /// внутренний алиас, используется непосредственно в SQL запросе
        /// </summary>
        public string InternalAlias;
        public Expression OnExpression;

        public ITableDesc Table { get; internal set; }
        public string Name {
            get { 
                if (!string.IsNullOrEmpty(Table.FakeName)) return Table.FakeName;
                if (!string.IsNullOrEmpty(Table.TableName)) return Table.TableName;
                if (Table is SubSelectTableDesc) return ((SubSelectTableDesc) Table).Alias;
                return null;
            }
        }

        public bool IsSubSelect()
        {
            return Table is SubSelectTableDesc;
        }

        public SubSelectTableDesc TableAsSubSelect()
        {
            return Table as SubSelectTableDesc;
        }

        public static SelectTable CreateBySubSelect(SubSelectTableDesc tableSubSelect)
        {
            SelectTable st = new SelectTable();
            st.Table = tableSubSelect;
            return st;
        }

        public static SelectTable CreateByTable(ITableDesc table)
        {
            SelectTable st = new SelectTable();
            st.Table = table;
            return st;
        }

        public bool CompareWithAlias(string alias)
        {
            if (Table is SubSelectTableDesc)
            {
                SubSelectTableDesc t = Table as SubSelectTableDesc;
                return StringComparer.InvariantCultureIgnoreCase.Compare(alias, t.Alias) == 0;
            }
            else
            {
                if (Table.FakeName != null) return StringComparer.InvariantCultureIgnoreCase.Compare(alias, Table.FakeName) == 0;
                if (!string.IsNullOrEmpty(Table.Schema)) return StringComparer.InvariantCultureIgnoreCase.Compare(alias, Table.Schema + "." + Table.TableName) == 0;
                return StringComparer.InvariantCultureIgnoreCase.Compare(alias, Table.TableName) == 0;
            }
        }

        public string ToStr()
        {
            if (Table is SubSelectTableDesc)
            {
                SubSelectTableDesc t = Table as SubSelectTableDesc;
                return "("+t.Select.ToStr()+")";
            }
            else
            {
                if (Table.FakeName != null) return "\"" + Table.FakeName + "\"";
                if (!string.IsNullOrEmpty(Table.Schema)) return "\"" + Table.Schema + "\".\""+Table.TableName+"\"";
                return "\"" + Table.TableName + "\"";
            }
        }

        public string ToSql(ExpressionSqlBuilder builder)
        {
            if (Table is SubSelectTableDesc)
            {
                SubSelectTableDesc t = Table as SubSelectTableDesc;
                return "(" + t.Select.Query.MakeSelectExpression(builder) + ")";
            }
            else
            {
                return BaseExpressionFactory.TableSqlCodeEscape(Table.Schema)+"."+BaseExpressionFactory.TableSqlCodeEscape(Table.TableName);
            }
        }
    }

    public class ColumnSelectJson
    {
        public string Value;
        public string Alias;
    }

    public class ColumnClause
    {
        public string Alias;

        public Expression ColumnExpression;

        public string DbAlias;
        public string ToValue(ExpressionSqlBuilder builder)
        {
            if (!string.IsNullOrEmpty(Alias)) return BaseExpressionFactory.TableSqlCodeEscape(Alias);
            return ColumnExpression.ToSQL(builder);
        }

        public string ExtractAlias()
        {
            if (!string.IsNullOrEmpty(Alias)) return Alias;
            return RecursiveFindField(ColumnExpression);
        }

        private string RecursiveFindField(Expression exp)
        {
            if (exp == null) return null;
            if (exp is FieldExpr) return ((FieldExpr)ColumnExpression).FieldName;
            if (exp is FieldCapExpr) return ((FieldCapExpr) ColumnExpression).FieldAlias;
            if (exp is SubExpression && exp.Childs != null && exp.Childs.Count == 1) return RecursiveFindField(exp.Childs[0]);
            return null;
        }
    }

    public class GroupByJson
    {
        public string Value;
    }

    public class GroupBy
    {
        public Expression Expression;
        public SortType Sort;
    }
}
