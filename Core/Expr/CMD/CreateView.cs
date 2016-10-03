using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore.Expr.Sql;

namespace ParserCore.Expr.CMD
{
    public class CreateView : CustomCmd
    {
        public TableName Table { get; set; }
        public List<string> Columns = new List<string>();
        public SelectExpresion AsSelect;

        public override string ToStr()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("CREATE VIEW ");
            sb.Append(Table.ToStr()).Append(" ");
            if (Columns.Count > 0)
            {
                sb.Append("(");
                for (int i = 0; i < Columns.Count; i++)
                {
                    if (i != 0) sb.Append(", ");
                    var c = Columns[i];
                    sb.Append(ParserUtils.TableToStrEscape(c));
                }
                sb.Append(") ");
            }
            sb.Append("AS ");
            sb.Append(AsSelect.ToStr());
            return sb.ToString();
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("CREATE VIEW ");
            sb.Append(Table.ToSql(builder)).Append(" ");
            if (Columns.Count > 0)
            {
                sb.Append("(");
                for (int i = 0; i < Columns.Count; i++)
                {
                    if (i != 0) sb.Append(", ");
                    var c = Columns[i];
                    sb.Append(ParserUtils.TableToStrEscape(c));
                }
                sb.Append(") ");
            }
            sb.Append("AS ");
            sb.Append(AsSelect.ToSql(builder));
            return sb.ToString();
        }

        public override void Prepare()
        {
            base.Prepare();
            AsSelect.Prepare();
        }

        public override IExplore Expolore(DelegateExpessionExplorer del)
        {
            AsSelect = AsSelect.Expolore(del) as SelectExpresion;
            return base.Expolore(del);
        }

        public override void ParseInside(ExpressionParser parser)
        {
            var collection = parser.Collection;
            var lex = collection.CurrentLexem();
            if (lex.LexemText.ToLower() != "create") parser.Collection.Error("error", collection.CurrentLexem());
            lex = collection.GotoNextMust();
            if (lex.LexemText.ToLower() != "view") parser.Collection.Error("error", collection.CurrentLexem());
            lex = collection.GotoNextMust();
            //название
            Table = TableName.Parse(parser);
            lex = collection.GotoNextMust();
            if (lex.IsSkobraOpen())
            {
                lex = collection.GotoNextMust();
                Columns.Clear();
                while (true)
                {
                    var col = CommonParserFunc.ReadColumnNameOnly(collection);
                    Columns.Add(col);
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
                lex = collection.GotoNextMust();
            }
            lex = collection.CurrentLexem();
            if (lex.LexemText.ToLower() != "as") parser.Collection.ErrorWaitKeyWord("AS", collection.CurrentLexem());
            lex = collection.GotoNextMust();
            AsSelect = new SelectExpresion();
            AsSelect.ParseInside(parser);

            //переходим к следующей лексеме
            collection.GotoNext();
        }

    }
}
