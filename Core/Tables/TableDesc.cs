using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public class TableDesc : SqlToken, ITableDesc
    {
        public string LogicalTableName { get; set; }
        public string LogicalSchema { get; set; }

        public string PhysicalTableName { get; set; }
        public string PhysicalSchema { get; set; }
        public List<Column> TableColumns { get; set; }
        public int CoordinateSystem { get; set; }

        public virtual Column ByName(string names)
        {
            foreach (var ci in TableColumns)
            {
                if (StringComparer.InvariantCultureIgnoreCase.Equals(ci.Name, names)) return ci;
            }
            return null;
        }

        public override void Prepare()
        {
        }

        public override string ToStr()
        {
            string s = "";
            if (!string.IsNullOrEmpty(LogicalSchema)) s += ParserUtils.TableToStrEscape(LogicalSchema) + ".";
            s += ParserUtils.TableToStrEscape(LogicalTableName);
            return s;
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            string s = "";
            if (!string.IsNullOrEmpty(PhysicalSchema)) s += builder.EncodeTable(PhysicalSchema) + ".";
            s += builder.EncodeTable(PhysicalTableName);
            return s;
        }
    }

}
