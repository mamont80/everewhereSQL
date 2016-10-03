using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public class GroupByClause : SqlToken, ISqlConvertible
    {
        private Expression _Expression;
        public Expression Expression
        {
            get { return _Expression; }
            set
            {
                _Expression = value;
                if (value != null) value.ParentToken = this;
            }
        }

        public override IExplore Expolore(DelegateExpessionExplorer del)
        {
            Expression = (Expression)Expression.Expolore(del);
            return base.Expolore(del);
        }

        public override void Prepare()
        {
            if (Expression != null) Expression.Prepare();
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            return Expression.ToSql(builder);
        }

        public override string ToStr()
        {
            return Expression.ToStr();
        }
    }
}
