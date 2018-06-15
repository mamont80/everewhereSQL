using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace ParserCore.Getters.NativeGetter
{
    public abstract class NativeTableGetterCustom : CustomTableGetter, ITableGetter
    {
        public abstract ExpressionSqlBuilder GetSqlBuilder();
        public abstract ITableDesc GetTableByName(string[] names, bool useCache);
        public abstract string DefaultSchema();
        public abstract DbConnection GetConnection();


    }
}
