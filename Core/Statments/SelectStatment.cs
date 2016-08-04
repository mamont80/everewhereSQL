using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LayerData;

namespace TableQuery
{
    public class SelectStatment: Statment
    {
        public SelectExpresion Select;

        public override string ToStr()
        {
            return Select.ToStr();
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            return Select.ToSQL(builder);
        }

        public override void Prepare()
        {
            Select.Prepare();
        }

        public override void Optimize()
        {
            Select.PrepareAndOptimize();
        }
    }
}
