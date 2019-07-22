using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace ParserCore
{
    /// <summary>
    /// Используется как аргумент для построения SQL строки where из expression
    /// </summary>
    public class ExpressionSqlBuilder
    {
        public DriverType DbType { get; private set; }

        /// <summary>
        /// Sepcial mode for SQL Server. Replace column table alias as "INSERTED.column"
        /// </summary>
        internal bool FieldsAsInsertedAlias = false;
        /// <summary>
        /// Replace the constant variables.
        /// </summary>
        public bool ConstatAsVariable { get; set; }

        public ExpressionSqlBuilder(DriverType type)
        {
            ConstatAsVariable = false;
            DbType = type;
            Parameters = new List<DbParameter>();
            if (DbType == DriverType.PostgreSQL)
            {
                ConnectionFactory = Npgsql.NpgsqlFactory.Instance;
                VariablePrefix = ":v";
            }
            if (DbType == DriverType.SqlServer)
            {
                ConnectionFactory = System.Data.SqlClient.SqlClientFactory.Instance;
                VariablePrefix = "@v";
            }
            CommandBuilder = ConnectionFactory.CreateCommandBuilder();
        }

        private DbProviderFactory ConnectionFactory;
        private DbCommandBuilder CommandBuilder;

        private string VariablePrefix { get; set; }
        public List<DbParameter> Parameters;
        public List<DbParameter> UserParameters;
        private int VariableCounter = 0;

        public bool ReplaceConstantAsVariable = false;

        public string EncodeTable(string table)
        {
            return CommandBuilder.QuoteIdentifier(table);
        }

        /// <summary>
        /// Create safe string constant
        /// </summary>
        /// <param name="str">string without ' '</param>
        internal string EncodeStringConstant(string str)
        {
            return "'" + str.Replace("'", "''") + "'";
        }

        public string SqlConstant(object value)
        {
            if (ReplaceConstantAsVariable)
            {
                var p = GenerateParameter();
                p.Value = value;
                return p.ParameterName;
            }
            else
            {
                if (value == null || !(value is string)) throw new Exception("Not supported");
                string s = value as string;
                return EncodeStringConstant(s);
            }
        }

        internal DbParameter GenerateParameter()
        {
            DbParameter p = ConnectionFactory.CreateParameter();
            p.ParameterName = VariablePrefix + VariableCounter.ToString();
            Parameters.Add(p);
            VariableCounter++;
            return p;
        }
    }
}
