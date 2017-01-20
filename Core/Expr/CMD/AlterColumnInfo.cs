using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.CMD
{
    public enum AlterColumnType
    {
        AddColumn,
        AlterColumn,
        DropColumn
    }

    public class AlterColumnInfo : SqlToken
    {
        public string Name;
        public ExactType Type = new ExactType();
        public bool Nullable = true;
        public bool AutoIncrement = false;
        public bool PrimaryKey = false;
        public AlterColumnType AlterColumn = AlterColumnType.AddColumn;
        public SortType Sort = SortType.ASC;

        public override void Prepare()
        {
            if (AutoIncrement && Type.GetSimpleType() != SimpleTypes.Integer)
            {
                throw new Exception("Can not create autoincrement by not ineteger type column");
            }
        }

        public override string ToStr()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ParserUtils.TableToStrEscape(Name));
            sb.Append(" ");
            sb.Append(Type.ToStr());
            if (Nullable) sb.Append(" NULL");
            else sb.Append(" NOT NULL");
            if (AutoIncrement) sb.Append(" AUTO_INCRENENT");
            if (PrimaryKey) sb.Append(" PRIMARY KEY");
            return sb.ToString();
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(builder.EncodeTable(Name));
            sb.Append(" ");
            if (builder.DbType == DriverType.PostgreSQL && AutoIncrement)
            {
                if (Type.Type == DbColumnType.BigInt) sb.Append("bigserial");
                else sb.Append("serial");
            }
            else
                sb.Append(Type.ToSql(builder));
            if (AutoIncrement && builder.DbType == DriverType.SqlServer) sb.Append(" IDENTITY(1,1)");
            if (Nullable) sb.Append(" NULL");
            else sb.Append(" NOT NULL");
            if (PrimaryKey) sb.Append(" PRIMARY KEY");
            
            return sb.ToString();
        }
    }
}
