using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Getters.NativeGetter
{
    public class QueryParams
    {
        public int CommandTimeout { get; set; }
        public string Sql;
        public object[] NameAndValues;
        public int OpenConnectionTime = 0;
        public int ExecuteTime = 0;

        public QueryParams Clone()
        {
            QueryParams qp = this.MemberwiseClone() as QueryParams;
            return qp;
        }

        public static QueryParams Create(string sql)
        {
            QueryParams qp = new QueryParams();
            qp.Sql = sql;
            return qp;
        }

        public static QueryParams Create(string sql, params object[] nameAndValues)
        {
            QueryParams qp = new QueryParams();
            qp.Sql = sql;
            qp.CommandTimeout = 0;
            qp.NameAndValues = nameAndValues;
            return qp;
        }
        public static QueryParams Create(int timeout, string sql, params object[] nameAndValues)
        {
            QueryParams qp = new QueryParams();
            qp.Sql = sql;
            qp.CommandTimeout = timeout;
            qp.NameAndValues = nameAndValues;
            return qp;
        }
    }
}
