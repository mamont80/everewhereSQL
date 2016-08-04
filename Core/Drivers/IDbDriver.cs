using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public enum DbDriverType
    {
        SqlServer,
        PostgreSQL,
        Sqlite
        //,Oracle - в будущем
        //,MongoDB - под большим вопросом
    }

    public interface IDbDriver
    {
        DbDriverType DbDriverType { get; }
        string ToSql(Expression exp, ExpressionSqlBuilder builder);
    }
}
