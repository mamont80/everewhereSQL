using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public enum DriverType
    {
        SqlServer,
        PostgreSQL,
        Sqlite
        //,Oracle - в будущем
        //,MongoDB - под большим вопросом
    }

    public interface IDriverDatabase
    {
        DriverType DriverType { get; }
        string ToSql(Expression exp, ExpressionSqlBuilder builder);
    }
}
