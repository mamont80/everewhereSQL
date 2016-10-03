using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore;

namespace ParserCore.Expr.Sql
{
    // Фунции работающие только для экспорта в SQL
    public class AllColumnExpr : Expression
    {
        /// <summary>
        /// Здесь находятся колонки которые подразумеваются под *
        /// </summary>
        public TokenList<ColumnClause> Columns;

        public string Prefix;

        public AllColumnExpr()
        {
            Columns = new TokenList<ColumnClause>(this);
        }

        public override bool IsOperation()
        {
            return false;
        }
        public override void Prepare()
        {
            Columns.Replace(GetColumns());
            foreach (var cc in Columns)
            {
                cc.Prepare();
            }
            base.Prepare();
        }

        protected List<ColumnClause> GetColumns()
        {
            List<ColumnClause> res = new List<ColumnClause>();
            var sExpr1 = CommonUtils.FindParentSelect(this);
            if (sExpr1 == null) throw new Exception("Select expression is not found");
            var tables = sExpr1.GetTables();
            for (int j = 0; j < tables.Length; j++)
            {
                var st = tables[j];
                var t = tables[j].Table;
                if (!string.IsNullOrEmpty(Prefix))
                {
                    string tableAlias = Prefix;
                    if (!st.CompareWithColumn(new string[1] { tableAlias })) continue;
                }
                var tlist = t.TableColumns;
                for (int k = 0; k < tlist.Count; k++)
                {
                    ColumnClause cc = new ColumnClause();
                    FieldExpr fe = new FieldExpr();
                    fe.Bind(tables[j], tlist[k].Name);
                    cc.ColumnExpression = fe;
                    res.Add(cc);
                }
            }
            return res;
        }

        public override int NumChilds() { return 0; }
        protected override bool CanCalcOnline() { return false; }

        private string GetResult(object data)
        {
            throw new Exception("Can not on fly calculation");
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            if (Columns.Count > 0)
            {
                StringBuilder sqlColumns = new StringBuilder();
                int i = 0;
                foreach (var cs in Columns)
                {
                    if (i != 0) sqlColumns.Append(", ");
                    sqlColumns.Append(cs.ColumnExpression.ToSql(builder));


                    if (!string.IsNullOrEmpty(cs.Alias)) sqlColumns.Append(" as ").Append(builder.EncodeTable(cs.Alias)).Append(" ");
                    i++;
                }
                sqlColumns.Append(" ");
                return sqlColumns.ToString();
            }
            else
            {
                string s = "";
                if (!string.IsNullOrEmpty(Prefix)) s = Prefix + ".";
                s = s + "*";
                return s;
            }
        }

        public override string ToStr()
        {
            string s = "";
            if (!string.IsNullOrEmpty(Prefix)) s = ParserUtils.TableToStrEscape(Prefix) + ".";
            s = s + "*";
            return s;
        }

        public override IExplore Expolore(DelegateExpessionExplorer del)
        {
            if (Columns != null)
            {
                List<ColumnClause> column2 = new List<ColumnClause>();
                foreach (var cc in Columns)
                {
                    var cc2 = (ColumnClause)cc.Expolore(del);
                    if (cc2 != null) column2.Add(cc2);
                }
                Columns.Replace(column2);
            }
            return base.Expolore(del);
        }
    }
}
