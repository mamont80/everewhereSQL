using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    /// <summary>
    /// For search tables for internal columns in statment
    /// Для поиска таблиц для внутренних колонок.
    /// </summary>
    interface ITableSource : ISqlConvertible
    {
        TableClause[] GetTables();
    }
}
