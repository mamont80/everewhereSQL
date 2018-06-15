using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ParserCore.Getters.NativeGetter
{
    public class NativeTableGetterMsSql : NativeTableGetterCustom
    {
        public string ConnStr { get; set; }

        public static string ProviderName = "System.Data.SqlClient";

        public override ExpressionSqlBuilder GetSqlBuilder()
        {
            return new ExpressionSqlBuilder(DriverType.SqlServer);
        }

        public override string DefaultSchema()
        {
            return "dbo";
        }

        public override DbConnection GetConnection()
        {
            return new SqlConnection(ConnStr);
        }

        public override ITableDesc GetTableByName(string[] names, bool useCache)
        {
            if (useCache)
            {
                var t = Get(names);
                if (t != null) return t;
            }
            string tn;
            string sh;
            if (names.Length == 2)
            {
                sh = names[0];
                tn = names[1];
            }
            else
                if (names.Length == 1)
                {
                    sh = DefaultSchema();
                    tn = names[0];
                }
                else throw new Exception("Incorrect table name");
            using (var con = GetConnection())
            {
                con.Open();
                string sql = "select TABLE_SCHEMA, TABLE_NAME from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA = @schema and TABLE_NAME = @name";
                DbCommand cmd = con.CreateCommand();
                cmd.CommandText = sql;
                ParserDbUtils.AddParam(cmd, "@schema", sh);
                ParserDbUtils.AddParam(cmd, "@name", tn);
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) throw new Exception("Table " + string.Join(".", names) + " is not found");
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
                ParserDbUtils.AddParam(cmd, "@schema", sh);
                ParserDbUtils.AddParam(cmd, "@name", tn);
                TableDesc td = new TableDesc();

                td.PhysicalTableName = tn;
                td.LogicalTableName = tn;
                td.PhysicalSchema = sh;
                td.LogicalSchema = sh;
                td.TableColumns = new List<Column>();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Column ci = new Column(
                            reader.GetString(reader.GetOrdinal("COLUMN_NAME")),
                            ColumnSqlTypeToSimpleType(reader.GetString(reader.GetOrdinal("DATA_TYPE"))));
                        td.TableColumns.Add(ci);
                    }
                }
                Set(names, td);
                return td;
            }
        }

        public const string GeometryType = "geometry";
        private const string TimeType = "time";
        private const string DateType = "date";
        public static readonly HashSet<string> BoolTypes =
            ParserDbUtils.CreateInvariantStringSet(new string[] { "bit" });

        public static readonly HashSet<string> TextTypes =
            ParserDbUtils.CreateInvariantStringSet(new string[] { "nvarchar", "varchar", "ntext", "text", "char", "nchar" });

        public static readonly HashSet<string> DateTimeTypes =
            ParserDbUtils.CreateInvariantStringSet(new string[] { "datetime", "datetime2", "smalldatetime" });

        public static readonly HashSet<string> IntTypes =
            ParserDbUtils.CreateInvariantStringSet(new string[] { "tinyint", "int", "bigint", "smallint" });
        public static readonly HashSet<string> FloatTypes =
            ParserDbUtils.CreateInvariantStringSet(new string[] { "float", "real", "decimal", "money", "numeric" });
        public static readonly HashSet<string> BlobTypes =
            ParserDbUtils.CreateInvariantStringSet(new string[] { "binary", "varbinary" });

        public SimpleTypes ColumnSqlTypeToSimpleType(string type)
        {
            string sqlType = type.ToLower();
            SimpleTypes SimpleType = SimpleTypes.String;
            if (BoolTypes.Contains(sqlType)) SimpleType = SimpleTypes.Boolean;
            else
                if (TextTypes.Contains(sqlType)) SimpleType = SimpleTypes.String;
                else
                    if (IntTypes.Contains(sqlType)) SimpleType = SimpleTypes.Integer;
                    else
                        if (FloatTypes.Contains(sqlType)) SimpleType = SimpleTypes.Float;
                        else
                            if (DateTimeTypes.Contains(sqlType)) SimpleType = SimpleTypes.DateTime;
                            else
                                if (sqlType == GeometryType) { SimpleType = SimpleTypes.Geometry; }
                                else
                                    if (sqlType == TimeType) SimpleType = SimpleTypes.Time;
                                    else
                                        if (sqlType == DateType) SimpleType = SimpleTypes.Date;
                                        else
                                            if (BlobTypes.Contains(sqlType)) SimpleType = SimpleTypes.Blob;
                                            else throw new Exception("Type of column is not support.");
            return SimpleType;
        }

    }

}
