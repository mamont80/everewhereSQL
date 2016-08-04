using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TableQuery;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.Common;

namespace WinTest
{
    public class SqlServerTableGetter: ITableGetter
    {
        public string ConnStr;
        public static string ProviderName = "System.Data.SqlClient";
        private static DbProviderFactory ConnectionFactory;
        private static DbCommandBuilder CommandBuilder;

        public IDbDriver Driver;

        public SqlServerTableGetter()
        {
            ConnectionFactory = DbProviderFactories.GetFactory(ProviderName);
            CommandBuilder = ConnectionFactory.CreateCommandBuilder();
            Driver = new MsSqlServerDriver();
        }

        protected DbConnection GetConnection()
        {
            return new SqlConnection(ConnStr);
        }

        public ITableDesc GetTableByName(string[] name)
        {
            string tn;
            string sh = "dbo";
            if (name.Length == 2)
            {
                sh = name[0];
                tn = name[1];
            }
            else
            if (name.Length == 1)
            {
                tn = name[0];
            }else throw new Exception("Incorrect table name");
            using (var con = GetConnection())
            {
                con.Open();
                string sql = "select TABLE_SCHEMA, TABLE_NAME from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA = @schema and TABLE_NAME = @name";
                DbCommand cmd = con.CreateCommand();
                cmd.CommandText = sql;
                cmd.AddParam("@schema", sh);
                cmd.AddParam("@name", tn);
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) throw new Exception("Table "+name+" is not found");
                }
            }

            using (var con = GetConnection())
            {
                con.Open();
                string sql = @"select
                    COLUMN_NAME,
                    DATA_TYPE
                  from INFORMATION_SCHEMA.COLUMNS where TABLE_SCHEMA = @schema and TABLE_NAME = @name";
                DbCommand cmd = con.CreateCommand();
                cmd.CommandText = sql;
                cmd.AddParam("@schema", sh);
                cmd.AddParam("@name", tn);
                TableDesc td = new TableDesc();
                td.TableName = tn;
                td.Schema = sh;
                td.DbDriver = Driver;
                td.Columns = new List<ColumnInfo>();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ColumnInfo ci = new ColumnInfo(
                            reader.GetString(reader.GetOrdinal("COLUMN_NAME")), 
                            ColumnSqlTypeToSimpleType(reader.GetString(reader.GetOrdinal("DATA_TYPE"))));
                        td.Columns.Add(ci);
                    }
                }
                return td;
            }
        }

        public const string GeometryType = "geometry";
        private const string TimeType = "time";
        private const string DateType = "date";
        public static readonly HashSet<string> BoolTypes =
            CommonUtils.CreateInvariantStringSet(new string[] { "bit" });

        public static readonly HashSet<string> TextTypes =
            CommonUtils.CreateInvariantStringSet(new string[] { "nvarchar", "varchar", "ntext", "text", "char", "nchar" });

        public static readonly HashSet<string> DateTimeTypes =
            CommonUtils.CreateInvariantStringSet(new string[] { "datetime", "datetime2", "smalldatetime" });

        public static readonly HashSet<string> IntTypes =
            CommonUtils.CreateInvariantStringSet(new string[] { "tinyint", "int", "bigint", "smallint" });
        public static readonly HashSet<string> FloatTypes =
            CommonUtils.CreateInvariantStringSet(new string[] { "float", "real", "decimal", "money", "numeric" });

        public ColumnSimpleTypes ColumnSqlTypeToSimpleType(string type)
        {
            string sqlType = type.ToLower();
            ColumnSimpleTypes ColumnSimpleType = ColumnSimpleTypes.String;
            if (BoolTypes.Contains(sqlType)) ColumnSimpleType = ColumnSimpleTypes.Boolean;
            else
                if (TextTypes.Contains(sqlType)) ColumnSimpleType = ColumnSimpleTypes.String;
                else
                    if (IntTypes.Contains(sqlType)) ColumnSimpleType = ColumnSimpleTypes.Integer;
                    else
                        if (FloatTypes.Contains(sqlType)) ColumnSimpleType = ColumnSimpleTypes.Float;
                        else
                            if (DateTimeTypes.Contains(sqlType)) ColumnSimpleType = ColumnSimpleTypes.DateTime;
                            else
                                if (sqlType == GeometryType) { ColumnSimpleType = ColumnSimpleTypes.Geometry; }
                                else
                                    if (sqlType == TimeType) ColumnSimpleType = ColumnSimpleTypes.Time;
                                    else
                                        if (sqlType == DateType) ColumnSimpleType = ColumnSimpleTypes.Date;
            return ColumnSimpleType;
        }

    }
}
