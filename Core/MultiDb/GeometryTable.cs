using System;
using System.Collections.Generic;
using System.Linq;

namespace TableQuery
{


    public class GeometryTable
    {
        public List<ColumnInfo> Columns = new List<ColumnInfo>();
        public ColumnInfo IDColumn { get; set; }
        public int CoordinateSystem { get; set; }
    }
}
