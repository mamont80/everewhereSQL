using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{

    /// <summary>
    /// Общий класс для insert, update, delete
    /// </summary>
    public abstract class OneTableStatement : CustomStatement, ITableSource, ISelect
    {
        private TableClause _TableClause;

        public TableClause TableClause
        {
            get { return _TableClause; }
            set
            {
                _TableClause = value;
                if (value != null) value.ParentToken = this;
            }
        }

        public TokenList<ColumnClause> ReturningColumns { get; private set; }

        public override List<ColumnClause> GetAllColumns()
        {
            return ReturningColumns.ToList();
        }

        public OneTableStatement() : base()
        {
            ReturningColumns = new TokenList<ColumnClause>(this);
        }

        #region ITableSource
        public TableClause[] GetTables()
        {
            var r = new TableClause[1] { TableClause };
            return r;
        }
        #endregion

        public override IExplore Expolore(DelegateExpessionExplorer del)
        {
            if (TableClause != null) TableClause = (TableClause)TableClause.Expolore(del);
            List<ColumnClause> Returning2 = new List<ColumnClause>();
            ReturningColumns.ForEach(a =>
            {
                ColumnClause ss = (ColumnClause)a.Expolore(del);
                if (ss != null) Returning2.Add(ss);
            });
            ReturningColumns.Replace(Returning2);
            return base.Expolore(del);
        }

        public override void Prepare()
        {
            base.Prepare();
            for (int i = 0; i < ReturningColumns.Count; i++)
            {
                ReturningColumns[i].Prepare();
            }

            TableColumns.Clear();
            List<ColumnClause> lst = ReturningColumns.ToList();
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

        protected void ParseReturining(ExpressionParser parser)
        {
            var collection = parser.Collection;
            var lex = collection.CurrentLexem();
            if (lex == null) return;
            if (lex.LexemText.ToLower() == "returning")
            {
                collection.GotoNextMust();
                int idx = collection.IndexLexem;
                ColumnClauseParser colsParser = new ColumnClauseParser();
                colsParser.Parse(collection);
                if (colsParser.Columns.Count == 0) collection.Error("Columnn not found", collection.Get(idx));
                ReturningColumns.Replace(colsParser.Columns);
            }
        }

        protected void AddReturningToStr(StringBuilder sb)
        {
            if (ReturningColumns.Count > 0)
            {
                sb.Append(" returning ");
                for (int i = 0; i < ReturningColumns.Count; i++)
                {
                    if (i > 0) sb.Append(", ");
                    sb.Append(ReturningColumns[i].ToStr());
                }
            }
        }

        protected void AddReturningToSql1(ExpressionSqlBuilder builder, StringBuilder sb)
        {
            if (ReturningColumns.Count > 0 && builder.DbType == DriverType.SqlServer)
            {
                sb.Append(" OUTPUT ");
                for (int i = 0; i < ReturningColumns.Count; i++)
                {
                    if (i > 0) sb.Append(", ");
                    builder.FieldsAsInsertedAlias = true;
                    try
                    {
                        sb.Append(ReturningColumns[i].ToSql(builder));
                    }
                    finally
                    {
                        builder.FieldsAsInsertedAlias = false;
                    }
                }
            }
        }

        protected void AddReturningToSql2(ExpressionSqlBuilder builder, StringBuilder sb)
        {
            if (ReturningColumns.Count > 0 && builder.DbType == DriverType.PostgreSQL)
            {
                sb.Append(" RETURNING ");
                for (int i = 0; i < ReturningColumns.Count; i++)
                {
                    if (i > 0) sb.Append(", ");
                    sb.Append(ReturningColumns[i].ToSql(builder));
                }
            }
        }

    }
}
