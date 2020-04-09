using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore.Expr.Sql;

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

    public class TableClause : SqlToken, ISqlConvertible
    {
        private TableClause()
        {
        }

        public string Alias;
        public JoinType Join = JoinType.Cross;

        private Expression _OnExpression;
        /// <summary>
        /// внутренний алиас, используется непосредственно в SQL запросе
        /// </summary>
        public Expression OnExpression
        {
            get { return _OnExpression; }
            set
            {
                _OnExpression = value;
                if (value != null) value.ParentToken = this;
            }
        }

        private ITableDesc _Table;
        /// <summary>
        /// внутренний алиас, используется непосредственно в SQL запросе
        /// </summary>
        public ITableDesc Table
        {
            get { return _Table; }
            internal set
            {
                _Table = value;
                if (value != null) value.ParentToken = this;
            }
        }

        public override void Prepare()
        {
            Table.Prepare();
            if ((Table is ISelect) && string.IsNullOrEmpty(Alias)) throw new Exception("Subselect must have alias");
            if (OnExpression != null) OnExpression.Prepare();
        }

        public override IExplore Expolore(DelegateExpessionExplorer del)
        {
            if (OnExpression != null) OnExpression = (Expression)OnExpression.Expolore(del);
            if (Table != null) Table = (ITableDesc)Table.Expolore(del);
            return base.Expolore(del);
        }

        public bool CompareWithColumn(string[] names)
        {
            string name = names.Last();
            string schema = null;
            if (names.Length > 1) schema = names[names.Length - 2];
            if (string.IsNullOrEmpty(schema))
            {
                if (StringComparer.InvariantCultureIgnoreCase.Compare(name, Alias) == 0) return true;
                if (Table is TableDesc)
                {
                    TableDesc td = (TableDesc)Table;
                    if (StringComparer.InvariantCultureIgnoreCase.Compare(name, td.LogicalTableName) == 0) return true;
                }
            }
            else
            {
                if (Table is TableDesc)
                {
                    TableDesc td = (TableDesc)Table;
                    if (StringComparer.InvariantCultureIgnoreCase.Compare(name, td.LogicalTableName) == 0 &&
                        StringComparer.InvariantCultureIgnoreCase.Compare(schema, td.LogicalSchema) == 0) return true;
                }
            }
            return false;
        }

        public static TableClause CreateBySelect(ITableDesc select)
        {
            TableClause st = new TableClause();
            st.Table = select;
            return st;
        }

        public static TableClause CreateByTable(string[] logicalName, ITableDesc table)
        {
            TableClause st = new TableClause();
            st.Table = table;
            return st;
        }

        public override string ToStr()
        {
            string s = "";
            if (Table is ISelect) s = "(" + Table.ToStr() + ")";
            else s = Table.ToStr();
            if (!string.IsNullOrEmpty(Alias)) s += " as " + ParserUtils.TableToStrEscape(Alias) + " ";
            return s;
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            string s = "";
            if (Table is ISelect) s = "(" + Table.ToSql(builder) + ")";
            else s = Table.ToSql(builder);
            if (!string.IsNullOrEmpty(Alias)) s += " as " + builder.EncodeTable(Alias) + " ";
            return s;
        }
    }

}
