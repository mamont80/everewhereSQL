using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableQuery
{
    public interface ITableGetter
    {
        ITableDesc GetTableByName(string[] name);
    }
}
