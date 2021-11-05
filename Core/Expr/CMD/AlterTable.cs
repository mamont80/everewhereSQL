using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.CMD
{
    /* 
     * Система аля PostgreSQL
     * 
     * Добавление колокни
     * Удаление колонки
     * Изменение типа
     * Изменение Null
     * Изменение названия таблицы (отдельно)
     * Изменение названия колонки (отдельно)
     * 
     * https://www.postgresql.org/docs/9.1/static/sql-altertable.html
     * 
     * ALTER TABLE name action [, ... ]
     * ALTER TABLE name RENAME [ COLUMN ] column TO new_column
     * ALTER TABLE name RENAME TO new_name
     * ALTER TABLE name SET SCHEMA new_schema
     * where action is one of:
     *  ADD [ COLUMN ] column data_type [ column_constraint [ ... ] ]
     *  DROP [ COLUMN ] column
     *  ALTER [ COLUMN ] column [ SET DATA ] TYPE data_type
     *  ALTER [ COLUMN ] column { SET | DROP } NOT NULL
     */

    public enum AlterType
    {
        RenameTable, RenameSchema, RenameColumn, AlterColumn
    }

    public class AlterTable : CustomCmd
    {
        public TableName Table { get; set; }

        public string OldColumnName;
        public string NewName;
        public AlterType AlterType;
        public TokenList<AlterColumnInfo> AlterColumns;

        public AlterTable() : base()
        {
            AlterColumns = new TokenList<AlterColumnInfo>(this);
        }

        public override string ToStr()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ALTER TABLE ");
            sb.Append(Table.ToStr());

            sb.Append(" ");
            if (AlterType == AlterType.RenameColumn)
            {
                sb.Append("RENAME COLUMN ");
                sb.Append(ParserUtils.TableToStrEscape(OldColumnName));
                sb.Append(" TO ").Append(ParserUtils.TableToStrEscape(NewName));
            }
            if (AlterType == AlterType.RenameTable)
            {
                sb.Append("RENAME TO ");
                sb.Append(ParserUtils.TableToStrEscape(NewName));
            }
            if (AlterType == AlterType.RenameSchema)
            {
                sb.Append("SET SCHEMA ");
                sb.Append(ParserUtils.TableToStrEscape(NewName));
            }
            if (AlterType == AlterType.AlterColumn)
            {
                for (int i = 0; i < AlterColumns.Count; i++)
                {
                    var cd = AlterColumns[i];
                    if (i > 0) sb.Append(", ");
                    switch (cd.AlterColumn)
                    {
                        case AlterColumnType.AddColumn:
                            sb.Append("ADD COLUMN ").Append(cd.ToStr());
                            break;
                        case AlterColumnType.DropColumn:
                            sb.Append("DROP COLUMN ").Append(ParserUtils.TableToStrEscape(cd.Name));
                            break;
                        case AlterColumnType.AlterColumn:
                            sb.Append("ALTER COLUNN ").Append(ParserUtils.TableToStrEscape(cd.Name)).Append(" ").Append(cd.ToStr());
                            break;
                    }
                }
            }
            return sb.ToString();
        }

        public override void Prepare()
        {
            base.Prepare();
            foreach (var c in AlterColumns)
            {
                c.Prepare();
            }
        }

        public override IExplore Expolore(DelegateExpessionExplorer del)
        {
            if (AlterColumns != null && AlterColumns.Count > 0)
            {
                var Columns2 = new TokenList<AlterColumnInfo>(this);
                foreach (var e in AlterColumns)
                {
                    var t = (AlterColumnInfo)e.Expolore(del);
                    if (t != null) Columns2.Add(t);
                }
                AlterColumns = Columns2;
            }
            return base.Expolore(del);
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            if (builder.DbType == DriverType.SqlServer) return ToMSSql(builder);
            if (builder.DbType == DriverType.PostgreSQL) return ToPostgreSql(builder);
            throw new Exception("Not supported DBMS");
        }

        protected string ToMSSql(ExpressionSqlBuilder builder)
        {
            StringBuilder sb = new StringBuilder();
            if (AlterType == AlterType.RenameTable)
            {
                var p1 = builder.SqlConstant(Table.ToSql(builder));

                var p2 = builder.SqlConstant(NewName);
                sb.AppendFormat("EXEC sp_rename {0}, {1}", p1, p2).Append(";");
            }
            if (AlterType == AlterType.RenameSchema)
            {
                sb.AppendFormat("ALTER SCHEMA {0} TRANSFER ", builder.EncodeTable(NewName)).Append(";");
                sb.Append(Table.ToSql(builder));
            }
            if (AlterType == AlterType.RenameColumn)
            {
                string s = "";
                s += Table.ToSql(builder);

                var p1 = builder.SqlConstant(s);
                var p2 = builder.SqlConstant(NewName);
                sb.AppendFormat("EXEC sp_RENAME {0}, {1}, 'COLUMN'", p1, p2).Append(";");
            }
            if (AlterType == AlterType.AlterColumn)
            {
                if (AlterColumns.Count > 0)
                {
                    sb.Append("ALTER TABLE ").Append(Table.ToSql(builder)).Append(" ");
                    var addCols = AlterColumns.Where(a => a.AlterColumn == AlterColumnType.AddColumn).ToList();
                    if (addCols.Count > 0)
                    {
                        sb.Append(" ADD ");
                        for (int i = 0; i < addCols.Count; i++)
                        {
                            var ac = addCols[i];
                            if (i > 0) sb.Append(", ");
                            sb.Append(ac.ToSql(builder));
                        }
                    }
                    var dropCols = AlterColumns.Where(a => a.AlterColumn == AlterColumnType.DropColumn).ToList();
                    if (dropCols.Count > 0)
                    {
                        sb.Append(" DROP ");
                        for (int i = 0; i < dropCols.Count; i++)
                        {
                            var ac = dropCols[i];
                            if (i > 0) sb.Append(", ");
                            sb.Append(ac.ToSql(builder));
                        }
                    }
                    var alterCols = AlterColumns.Where(a => a.AlterColumn == AlterColumnType.AlterColumn).ToList();
                    if (alterCols.Count > 0)
                    {
                        sb.Append(" ALTER COLUMN ");
                        for (int i = 0; i < alterCols.Count; i++)
                        {
                            var ac = alterCols[i];
                            if (i > 0) sb.Append(", ");
                            sb.Append(ac.ToSql(builder));
                        }
                    }

                    sb.Append(";");
                }
            }
            return sb.ToString();
        }

        protected string ToPostgreSql(ExpressionSqlBuilder builder)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ALTER TABLE ");
            sb.Append(Table.ToSql(builder));
            sb.Append(" ");
            if (AlterType == AlterType.RenameColumn)
            {
                sb.Append("RENAME COLUMN ");
                sb.Append(builder.EncodeTable(OldColumnName));
                sb.Append(" TO ").Append(builder.EncodeTable(NewName));
            }
            if (AlterType == AlterType.RenameTable)
            {
                sb.Append("RENAME TO ");
                sb.Append(builder.EncodeTable(NewName));
            }
            if (AlterType == AlterType.RenameSchema)
            {
                sb.Append("SET SCHEMA ");
                sb.Append(builder.EncodeTable(NewName));
            }
            if (AlterType == AlterType.AlterColumn)
            {
                for (int i = 0; i < AlterColumns.Count; i++)
                {
                    var cd = AlterColumns[i];
                    if (i > 0) sb.Append(", ");
                    switch (cd.AlterColumn)
                    {
                        case AlterColumnType.AddColumn:
                            sb.Append("ADD COLUMN ").Append(cd.ToSql(builder));
                            break;
                        case AlterColumnType.DropColumn:
                            sb.Append("DROP COLUMN ").Append(builder.EncodeTable(cd.Name));
                            break;
                        case AlterColumnType.AlterColumn:
                            sb.Append("ALTER COLUNN ").Append(builder.EncodeTable(cd.Name));
                            if (cd.Nullable) sb.Append(" DROP NOT NULL");
                            else
                                sb.Append(" SET NOT NULL");
                            sb.Append(", ALTER COLUNN ").Append(builder.EncodeTable(cd.Name));
                            sb.Append("ALTER COLUNN ").Append(builder.EncodeTable(cd.Name)).Append(" TYPE ").Append(cd.Type.ToSql(builder));
                            break;
                    }
                }
            }
            return sb.ToString();
        }

        public override void ParseInside(ExpressionParser parser)
        {
            var collection = parser.Collection;
            if (!ParserUtils.ParseCommandPhrase(collection, "alter table")) collection.ErrorUnexpected(collection.CurrentOrLast());
            var lex = collection.GotoNextMust();

            Table = TableName.Parse(parser);

            lex = collection.GotoNextMust();
            string s = lex.LexemText.ToLower();
            if (ParserUtils.ParseCommandPhrase(collection, "set schema"))
            {
                lex = collection.GotoNextMust();
                NewName = CommonParserFunc.ReadColumnNameOnly(collection);
                AlterType = AlterType.RenameSchema;
                collection.GotoNext();
            }else
            if (ParserUtils.ParseCommandPhrase(collection, "rename to", true, false))
            {
                lex = collection.GotoNextMust();
                NewName = CommonParserFunc.ReadColumnNameOnly(collection);
                AlterType = AlterType.RenameTable;
                collection.GotoNext();
            }
            else if (ParserUtils.ParseCommandPhrase(collection, "rename column", true, false) || ParserUtils.ParseCommandPhrase(collection, "rename", true, false))
            {
                lex = collection.GotoNextMust();
                var col = CommonParserFunc.ReadColumnNameOnly(collection);
                OldColumnName = col;
                lex = collection.GotoNextMust();
                if (lex.LexemText.ToLower() != "to") collection.ErrorWaitKeyWord("TO", collection.CurrentOrLast());
                lex = collection.GotoNextMust();
                NewName = CommonParserFunc.ReadColumnNameOnly(collection);
                AlterType = AlterType.RenameColumn;
                collection.GotoNext();
            }
            else//actions
            {
                AlterType = AlterType.AlterColumn;
                while (true)
                {
                    lex = collection.CurrentLexem();
                    if (ParserUtils.ParseCommandPhrase(collection, "add column", true, false) || ParserUtils.ParseCommandPhrase(collection, "add", true, false))
                    {
                        collection.GotoNextMust();
                        var cd = CreateTable.ReadColumnInfo(collection);
                        cd.AlterColumn = AlterColumnType.AddColumn;
                        AlterColumns.Add(cd);
                    }
                    else if (ParserUtils.ParseCommandPhrase(collection, "drop column", true, false) || ParserUtils.ParseCommandPhrase(collection, "drop", true, false))
                    {
                        lex = collection.GotoNextMust();
                        var cd = new AlterColumnInfo();
                        cd.Name = CommonParserFunc.ReadColumnNameOnly(collection);
                        cd.AlterColumn = AlterColumnType.DropColumn;
                        AlterColumns.Add(cd);
                    }
                    else if (ParserUtils.ParseCommandPhrase(collection, "alter column", true, false) || ParserUtils.ParseCommandPhrase(collection, "alter", true, false))
                    {
                        lex = collection.GotoNextMust();
                        var cd = CreateTable.ReadColumnInfo(collection);
                        AlterColumns.Add(cd);
                    }else collection.ErrorUnexpected(collection.CurrentOrLast());
                    lex = collection.CurrentLexem();
                    if (lex != null && lex.LexemType == LexType.Zpt)
                    {
                        collection.GotoNextMust();
                        continue;
                    }
                    break;
                }
                collection.GotoNext();
            }
        }
    }
}
