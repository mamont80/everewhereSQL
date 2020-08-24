using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public class UpdateStatement : OneTableStatement
    {

        public TokenList<SetClause> Set { get; private set; }

        private Expression _Where;

        public Expression Where
        {
            get { return _Where; }
            set
            {
                _Where = value;
                if (value != null) value.ParentToken = this;
            }
        }
        
        public UpdateStatement():base()
        {
            Set = new TokenList<SetClause>(this);
        }

        public override IExplore Expolore(DelegateExpessionExplorer del)
        {
            List<SetClause> Set2 = new List<SetClause>();
            Set.ForEach(a =>
                {
                    SetClause ss = (SetClause)a.Expolore(del);
                    if (ss != null) Set2.Add(ss);
                });
            Set.Replace(Set2);
            if (Where != null) Where = (Expression)Where.Expolore(del);

            return base.Expolore(del);
        }

        public override string ToStr()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("update ").Append(TableClause.ToStr());
            if (Set.Count > 0)
            {
                sb.Append(" SET ");
                for (int i = 0; i < Set.Count; i++)
                {
                    var sc = Set[i];
                    if (i > 0) sb.Append(", ");
                    sb.Append(sc.ToStr());
                }
            }
            if (Where != null) sb.Append(" where ").Append(Where.ToStr());
            AddReturningToStr(sb);
            return sb.ToString();
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("update ");
            if (builder.DbType == DriverType.PostgreSQL)
            {
                sb.Append(TableClause.ToSql(builder));
                writeSET(sb, builder);
            }
            if (builder.DbType == DriverType.SqlServer)
            {
                if (string.IsNullOrEmpty(TableClause.Alias))
                {
                    sb.Append(TableClause.ToSql(builder));
                    writeSET(sb, builder);
                }
                else
                {
                    sb.Append(builder.EncodeTable(TableClause.Alias)+" ");
                    writeSET(sb, builder);
                    sb.Append(" from ").Append(TableClause.ToSql(builder)).Append(" ");
                }
            }

            AddReturningToSql1(builder, sb);
            if (Where != null) sb.Append(" where ").Append(Where.ToSql(builder));
            AddReturningToSql2(builder, sb);
            return sb.ToString();
        }

        private void writeSET(StringBuilder sb, ExpressionSqlBuilder builder)
        {
            if (Set.Count > 0)
            {
                sb.Append(" SET ");
                for (int i = 0; i < Set.Count; i++)
                {
                    var sc = Set[i];
                    if (i > 0) sb.Append(", ");
                    sb.Append(sc.ToSql(builder));
                }
            }
        }


        public override void Prepare()
        {
            base.Prepare();
            for (int i = 0; i < Set.Count; i++)
            {
                Set[i].Prepare();
            }

            if (Where != null)
            {
                Where.Prepare();
                ExpUtils.CheckWhere(Where);
            }
        }

        public override void ParseInside(ExpressionParser parser)
        {
            var collection = parser.Collection;
            var lex = collection.CurrentLexem();
            if (lex.LexemText.ToLower() != "update") throw new Exception("Not UPDATE statment");
            lex = collection.GotoNextMust();

            FromParser fc = new FromParser();
            fc.Parse(collection);
            if (fc.Tables.Count > 1) collection.Error("Multi tables in update", collection.CurrentLexem());
            if (fc.Tables.Count == 0) collection.Error("Not table clause", collection.CurrentLexem());
            TableClause = fc.Tables[0];


            //string[] tablename = CommonParserFunc.ReadTableName(collection);
            // TODO: fixed! ok
            //TableClause = TableClause.CreateByTable(tablename, collection.TableGetter.GetTableByName(tablename));
            //lex = collection.GotoNextMust();
            lex = collection.CurrentLexem();

            if (lex.LexemText.ToLower() != "set")
            {
                collection.Error("SET keyword is not found", collection.CurrentLexem());
            }

            while (true)
            {
                lex = collection.GotoNextMust();//пропускаем SET или ','
                //lex = collection.CurrentLexem();

                SetClause sc = new SetClause();
                sc.Column = CommonParserFunc.ReadColumn(collection);
                lex = collection.GotoNextMust();
                if (lex.LexemText != "=") collection.Error("Operator '=' is not found", collection.CurrentLexem());
                lex = collection.GotoNextMust();
                ExpressionParser e = new ExpressionParser(parser.Collection);
                e.Parse();
                sc.Value = e.Single();
                Set.Add(sc);
                lex = collection.CurrentLexem();
                if (lex == null) break;
                if (lex.LexemType == LexType.Zpt) continue;
                break;
            }

            if (lex == null) return;
            if (lex.LexemText.ToLower() == "where")
            {
                collection.GotoNextMust();
                ExpressionParser e = new ExpressionParser(parser.Collection);
                e.Parse();
                Where = e.Single();
            }
            ParseReturining(parser);
        }
    }
}
