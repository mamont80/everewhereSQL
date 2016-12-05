using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public abstract class CustomStatement: Expression
    {
        public CustomStatement()
            : base()
        {
            TableColumns = new List<Column>();
        }

        #region ITableDesc
        public int CoordinateSystem { get; set; }

        public List<Column> TableColumns { get; set; }

        public virtual Column ByName(string names)
        {
            var cl = TableColumns;
            foreach (var ci in cl)
            {
                if (ci.Name == null) continue;
                if (StringComparer.InvariantCultureIgnoreCase.Equals(ci.Name, names)) return ci;
            }
            return null;
        }
        #endregion

        public override bool IsFunction() { return false; }
        public override bool IsOperation() { return false; }
        protected override bool CanCalcOnline() { return false; }

        public int Timeout = 0;

        public abstract List<ColumnClause> GetAllColumns();

    }
}
