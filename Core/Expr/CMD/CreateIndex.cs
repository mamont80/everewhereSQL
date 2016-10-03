using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.CMD
{
    public class CreateIndex: CustomCmd
    {
        public TableName IndexName;
        public TableName OnTable;
        public bool Unique = false;
        public bool IfNotExists = false;
        public List<AlterColumnInfo> IndexColumns = new List<AlterColumnInfo>();

        public override string ToStr()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("CREATE");
            if (Unique) sb.Append(" UNIQUE");
            sb.Append(" INDEX");
            if (IfNotExists) sb.Append(" IF NOT EXISTS");
            sb.Append(" ").Append(IndexName.ToStr());
            sb.Append(" ON ").Append(OnTable.ToStr());
            sb.Append(" (");
            for (int i = 0; i < IndexColumns.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                var ac = IndexColumns[i];
                sb.Append(ParserUtils.TableToStrEscape(ac.Name));
                if (ac.Sort == ParserCore.SortType.ASC) sb.Append(" ASC");
                else sb.Append(" DESC");
            }
            sb.Append(")");
            return sb.ToString();
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            StringBuilder sb = new StringBuilder();
            if (builder.DbType == DriverType.SqlServer)
            {
                if (IfNotExists)
                {
                    var p = builder.SqlConstant(IndexName.ToSql(builder, true));
                    sb.Append( string.Format("IF NOT EXISTS (SELECT name FROM sysindexes WHERE name = {0}) ", p) );
                }
                
                sb.Append("CREATE");
                if (Unique) sb.Append(" UNIQUE");
                sb.Append(" INDEX");
                sb.Append(" ").Append(IndexName.ToSql(builder));
                sb.Append(" ON ").Append(OnTable.ToSql(builder));
                sb.Append(" (");
                for (int i = 0; i < IndexColumns.Count; i++)
                {
                    if (i > 0) sb.Append(", ");
                    var ac = IndexColumns[i];
                    sb.Append(builder.EncodeTable(ac.Name));
                    if (ac.Sort == ParserCore.SortType.ASC) sb.Append(" ASC");
                    else sb.Append(" DESC");
                }
                sb.Append(");");
            }
            if (builder.DbType == DriverType.PostgreSQL)
            {
                string create = "CREATE";
                if (Unique) create += " UNIQUE";
                create += (" INDEX");
                create += " " + IndexName.ToSql(builder);
                create += " ON "+OnTable.ToSql(builder);
                create += " (";
                for (int i = 0; i < IndexColumns.Count; i++)
                {
                    if (i > 0) create +=", ";
                    var ac = IndexColumns[i];
                    create += builder.EncodeTable(ac.Name);
                    if (ac.Sort == ParserCore.SortType.ASC) create += " ASC";
                    else create += " DESC";
                }
                create += ");";

                if (!IfNotExists) sb.Append(create);
                else
                {
                    string schema = "public";
                    if (!string.IsNullOrEmpty(OnTable.Schema))
                        schema = OnTable.Schema;
                    var schemaP = builder.SqlConstant(schema);
                    var tableP = builder.SqlConstant(OnTable.Name);
                    var indexP = builder.SqlConstant(IndexName);
                    
                    string s = string.Format(@"
do
$$
declare 
   l_count integer;
begin
  select count(*)
     into l_count
  from pg_indexes
    where schemaname = {0}
    and tablename = {1}
    and indexname = {2};

  if l_count = 0 then 
     {3}
  end if;

end;
$$;", schemaP, tableP, indexP, create);
                    sb.Append(s);
                }
            }
            return sb.ToString();
        }

        public override void ParseInside(ExpressionParser parser)
        {
            var collection = parser.Collection;
            var lex = collection.CurrentLexem();
            if (lex.LexemText.ToLower() != "create") collection.ErrorWaitKeyWord("CREATE", collection.CurrentOrLast());
            lex = collection.GotoNextMust();
            if (lex.LexemText.ToLower() == "unique")
            {
                Unique = true;
                lex = collection.GotoNextMust();
            }
            if (lex.LexemText.ToLower() != "index") collection.ErrorWaitKeyWord("INDEX", collection.CurrentOrLast());
            lex = collection.GotoNextMust();
            if (ParserUtils.ParseCommandPhrase(collection, "if not exists"))
            {
                IfNotExists = true;
                lex = collection.GotoNextMust();
            }

            IndexName = TableName.Parse(parser);
            lex = collection.GotoNextMust();
            lex = collection.CurrentLexem();
            if (lex.LexemType != LexType.Command || lex.LexemText.ToLower() != "on") collection.ErrorWaitKeyWord("ON", collection.CurrentOrLast());
            lex = collection.GotoNextMust();
            OnTable = TableName.Parse(parser);
            lex = collection.GotoNextMust();
            if (!lex.IsSkobraOpen()) collection.ErrorWaitKeyWord("(", collection.CurrentOrLast());
            lex = collection.GotoNextMust();
            while (true)
            {
                AlterColumnInfo cd = new AlterColumnInfo();
                lex = collection.CurrentLexem();
                cd.Name = CommonParserFunc.ReadColumnNameOnly(collection);
                lex = collection.GotoNextMust();
                if (lex.LexemText.ToLower() == "desc")
                {
                    cd.Sort = ParserCore.SortType.DESC;
                    lex = collection.GotoNextMust();
                }else if (lex.LexemText.ToLower() == "asc")
                {
                    cd.Sort = ParserCore.SortType.ASC;
                    lex = collection.GotoNextMust();
                }

                IndexColumns.Add(cd);
                lex = collection.CurrentLexem();
                if (lex == null) parser.Collection.UnexceptEndError();
                if (lex.LexemType == LexType.Zpt)
                {
                    collection.GotoNextMust();
                    continue;
                }
                if (lex.IsSkobraClose()) break;
                collection.ErrorUnexpected(collection.CurrentOrLast());
            }
            lex = collection.CurrentLexem();            
        }
    }
}
