using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore.Expr.Sql;
using ParserCore.Expr.Simple;

namespace ParserCore
{
    public class ColumnClause : SqlToken, ISqlConvertible
    {
        public string Alias;

        private Expression _ColumnExpression;
        public Expression ColumnExpression
        {
            get { return _ColumnExpression; }
            set
            {
                _ColumnExpression = value;
                if (value != null) value.ParentToken = this;
            }
        }

        internal string InternalDbAlias;

        public string ExtractAlias()
        {
            if (!string.IsNullOrEmpty(Alias)) return Alias;
            return RecursiveFindField(ColumnExpression);
        }

        private string RecursiveFindField(Expression exp)
        {
            if (exp == null) return null;
            if (exp is FieldExpr) return ((FieldExpr)ColumnExpression).FieldName;
            if (exp is SubExpression && exp.Childs != null && exp.Childs.Count == 1) return RecursiveFindField(exp.Childs[0]);
            return null;
        }

        public override IExplore Expolore(DelegateExpessionExplorer del)
        {
            if (ColumnExpression != null) ColumnExpression = (Expression)ColumnExpression.Expolore(del);
            return del(this);
        }

        public override void Prepare()
        {
            if (ColumnExpression != null) ColumnExpression.Prepare();
        }

        public override string ToStr()
        {
            string s = ColumnExpression.ToStr();
            if (!string.IsNullOrEmpty(Alias)) s = s + " AS " + ParserUtils.ConstToStrEscape(Alias);
            return s;
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            string s = ColumnExpression.ToSql(builder);
            if (!string.IsNullOrEmpty(InternalDbAlias)) s = s + " AS " + builder.EncodeTable(InternalDbAlias);
            else
                if (!string.IsNullOrEmpty(Alias)) s = s + " AS " + builder.EncodeTable(Alias);
            return s;
        }
    }

}
