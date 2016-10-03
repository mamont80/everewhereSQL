using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public enum SortType
    {
        ASC,
        DESC
    }


    public class OrderByClause : GroupByClause
    {
        public SortType Sort = SortType.ASC;

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            string s = Expression.ToSql(builder);
            if (Sort == SortType.DESC) s += " DESC";
            return s;
        }

        public override string ToStr()
        {
            string s = Expression.ToStr();
            if (Sort == SortType.DESC) s += " DESC";
            return s;
        }
    }
}
