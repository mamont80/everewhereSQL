using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public class ColumnSubSelect : Column
    {
        public ColumnClause ColumnClause;

        public override string PhysicalName
        {
            get
            {
                return ColumnClause.InternalDbAlias;
            }
            set
            {
            }
        }
    }
}
