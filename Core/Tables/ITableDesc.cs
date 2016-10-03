using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public interface ITableDesc : IExplore, ISqlConvertible
    {
        List<Column> TableColumns { get; set; }
        int CoordinateSystem { get; set; }
        Column ByName(string names);
    }
}
