using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace ParserCore.Getters.NativeGetter
{
    public class NativeTableGetterPG : NativeTableGetterCustom, ITableGetter
    {
        public string ConnStr { get; set; }

        public static string ProviderName = "Npgsql";
        private DbProviderFactory ConnectionFactory;
        private DbCommandBuilder CommandBuilder;

        public NativeTableGetterPG()
        {
            ConnectionFactory = DbProviderFactories.GetFactory(ProviderName);
            CommandBuilder = ConnectionFactory.CreateCommandBuilder();
        }

        public override ExpressionSqlBuilder GetSqlBuilder()
        {
            return new ExpressionSqlBuilder(DriverType.PostgreSQL);
        }

        public override string DefaultSchema()
        {
            return "public";
        }

        public override DbConnection GetConnection()
        {
            var c = ConnectionFactory.CreateConnection();
            c.ConnectionString = ConnStr;
            return c;
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
                string sql = @"SELECT EXISTS (
   SELECT 1 
   FROM   pg_catalog.pg_class c
   JOIN   pg_catalog.pg_namespace n ON n.oid = c.relnamespace
   WHERE  n.nspname = @schema
   AND    c.relname = @name
   );";
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
                string sql = @"SELECT 
      attname            AS column_name
     , atttypid::regtype  AS data_type
       -- more attributes?
FROM   pg_attribute 
WHERE  attrelid = @schemaTable::regclass  -- table name, optionally schema-qualified
AND    attnum > 0
AND    NOT attisdropped
ORDER  BY attnum";
                DbCommand cmd = con.CreateCommand();
                cmd.CommandText = sql;
                string st = "";
                if (!string.IsNullOrEmpty(sh)) st += "\"" + sh + "\"";
                if (!string.IsNullOrEmpty(tn))
                {
                    if (st != "") st += ".";
                    st += "\"" + tn + "\"";
                }

                ParserDbUtils.AddParam(cmd, "@schemaTable", st);
                TableDesc td = new TableDesc();

                td.PhysicalTableName = tn;
                td.LogicalTableName = tn;
                td.PhysicalSchema = sh;
                td.LogicalSchema = sh;
                //td.DbDriver = Driver;
                td.TableColumns = new List<Column>();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Column ci = new Column(
                            reader.GetString(reader.GetOrdinal("column_name")),
                            ColumnSqlTypeToSimpleType(reader.GetString(reader.GetOrdinal("data_type")))
                            );
                        td.TableColumns.Add(ci);
                    }
                }
                Set(names, td);
                return td;
            }
        }
        public static readonly HashSet<string> GeometryTypes =
            ParserDbUtils.CreateInvariantStringSet(new string[] { "geometry", "USER-DEFINED" });
        public static readonly HashSet<string> BoolTypes =
            ParserDbUtils.CreateInvariantStringSet(new string[] { "boolean" });

        public static readonly HashSet<string> TextTypes =
            ParserDbUtils.CreateInvariantStringSet(new string[] { "character varying", "varchar", "character", "char", "text" });

        public static readonly HashSet<string> DateTimeTypes =
            ParserDbUtils.CreateInvariantStringSet(new string[] { "timestamp", "timestamp with time zone", "timestamp without time zone" });
        public static readonly HashSet<string> TimeTypes =
            ParserDbUtils.CreateInvariantStringSet(new string[] { "time", "time with time zone", "time without time zone" });
        public static readonly HashSet<string> DateTypes =
            ParserDbUtils.CreateInvariantStringSet(new string[] { "date" });

        public static readonly HashSet<string> IntTypes =
            ParserDbUtils.CreateInvariantStringSet(new string[] { "smallint", "integer", "bigint", "serial", "bigserial" });
        public static readonly HashSet<string> FloatTypes =
            ParserDbUtils.CreateInvariantStringSet(new string[] { "real", "decimal", "numeric", "double precision" });
        public static readonly HashSet<string> BlobTypes =
            ParserDbUtils.CreateInvariantStringSet(new string[] { "bytea" });
        /// <summary>
        /// Преобразрование названия типа из СУБД в простой тип ColumnSimpleTypes
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public SimpleTypes ColumnSqlTypeToSimpleType(string type)
        {
            string sqlType = type.ToLower();
            SimpleTypes ColumnSimpleType = SimpleTypes.String;
            if (BoolTypes.Contains(sqlType)) ColumnSimpleType = SimpleTypes.Boolean;
            else
                if (TextTypes.Contains(sqlType)) ColumnSimpleType = SimpleTypes.String;
                else
                    if (IntTypes.Contains(sqlType)) ColumnSimpleType = SimpleTypes.Integer;
                    else
                        if (FloatTypes.Contains(sqlType)) ColumnSimpleType = SimpleTypes.Float;
                        else
                            if (DateTimeTypes.Contains(sqlType)) ColumnSimpleType = SimpleTypes.DateTime;
                            else
                                if (GeometryTypes.Contains(sqlType)) { ColumnSimpleType = SimpleTypes.Geometry; }
                                else
                                    if (TimeTypes.Contains(sqlType)) ColumnSimpleType = SimpleTypes.Time;
                                    else
                                        if (DateTypes.Contains(sqlType)) ColumnSimpleType = SimpleTypes.Date;
                                        else if (BlobTypes.Contains(sqlType)) ColumnSimpleType = SimpleTypes.Blob;
            return ColumnSimpleType;
        }


    }
}
