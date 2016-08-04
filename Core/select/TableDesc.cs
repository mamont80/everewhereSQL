using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public interface ITableDesc
    {
        string FakeName { get; set; }
        string TableName { get; set; }
        string Schema { get; set; }
        IDbDriver DbDriver { get; }
        List<Column> Columns { get; }
        int CoordinateSystem { get; set; }
        Column ByName(string name);
    }

    public class TableDesc : ITableDesc
    {
        public string FakeName { get; set; }
        public string TableName { get; set; }
        public string Schema { get; set; }
        public IDbDriver DbDriver { get; set; }
        public List<Column> Columns { get; set; }
        public int CoordinateSystem { get; set; }

        public Column ByName(string name)
        {
            foreach (var ci in Columns)
            {
                if (StringComparer.InvariantCultureIgnoreCase.Equals(ci.Name, name)) return ci;
            }
            return null;
        }
    }

    
    public class SubSelectTableDesc : TableDesc
    {
        public SelectExpresion Select;

        public string Alias { get; set; }

        public SubSelectTableDesc(SelectExpresion select)
        {
            Select = select;
            DbDriver = select.Query.Driver;
            Columns = new List<Column>();
        }
    }

}
