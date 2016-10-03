using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public interface ITableGetter
    {
        ITableDesc GetTableByName(string[] names);
        string DefaultSchema();
    }
}
