using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace ParserCore.Getters.NativeGetter
{
    public abstract class NativeTableGetterCustom : CustomTableGetter, ITableGetter
    {
        public abstract ExpressionSqlBuilder GetSqlBuilder();
        public abstract ITableDesc GetTableByName(string[] names, bool useCache);
        public abstract string DefaultSchema();
        public abstract DbConnection GetConnection();

        public DbCommand GetCommand(QueryParams queryParams)
        {
            var conn = GetConnection();
            var com = conn.CreateCommand();
            com.Connection = conn;
            com.CommandText = queryParams.Sql;
            if (queryParams.CommandTimeout > 0) com.CommandTimeout = queryParams.CommandTimeout;
            
            AddParams(com, queryParams.NameAndValues);
            return com;
        }
        public DbCommand GetCommand(string sql, params object[] nameAndValues)
        {
            QueryParams qp = QueryParams.Create(sql, nameAndValues);
            return GetCommand(qp);
        }

        public object ExecuteScalar(string sql, params object[] nameAndValues)
        {
            QueryParams qp = QueryParams.Create(sql, nameAndValues);
            return ExecuteScalar(qp);
        }
        public object ExecuteScalar(QueryParams queryParams)
        {
            DbCommand com = GetCommand(queryParams);
            com.Connection.Open();
            try
            {
                return com.ExecuteScalar();
            }
            finally
            {
                com.Connection.Dispose();
                com.Dispose();
            }
        }

        //DbReader обязательно Dispose, connection будет освобождён автоматом
        public DbDataReader ExecuteReader(string sql, params object[] nameAndValues)
        {
            QueryParams qp = QueryParams.Create(sql, nameAndValues);
            return ExecuteReader(qp);
        }

        //DbReader обязательно Dispose, connection будет освобождён автоматом
        public DbDataReader ExecuteReader(QueryParams queryParams)
        {
            DbCommand com = GetCommand(queryParams);
            com.Connection.Open();
            return com.ExecuteReader(CommandBehavior.CloseConnection);
        }


        private void AddParams(DbCommand com, params object[] nameAndValues)
        {
            if (nameAndValues == null) return;
            for (int i = 0; i < nameAndValues.Length; i = i + 2)
            {
                DbParameter p = com.CreateParameter();
                if (nameAndValues[i] is string) p.ParameterName = PrepareParamName(nameAndValues[i] as string);
                if (nameAndValues[i + 1] == null) p.Value = DBNull.Value; else p.Value = nameAndValues[i + 1];
                com.Parameters.Add(p);
            }
        }

        protected virtual string PrepareParamName(string paramName)
        {
            //if (PostreSqlStyle)return paramName.Replace('@', ':') else 
            return paramName;
        }


    }
}
