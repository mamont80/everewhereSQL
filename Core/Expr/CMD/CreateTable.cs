using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.CMD
{


    public class CreateTable : CustomCmd
    {
        
        public TableName Table { get; set; }
        public TokenList<AlterColumnInfo> Columns;

        public CreateTable() : base()
        {
            Columns = new TokenList<AlterColumnInfo>(this);
        }

        public override string ToStr()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("CREATE TABLE ");
            sb.Append(Table.ToStr());
            sb.Append(" (");
            for (int i = 0; i < Columns.Count; i++)
            {
                if (i != 0) sb.Append(", ");
                var c = Columns[i];
                sb.Append(c.ToStr());
            }
            sb.Append(")");
            return sb.ToString();
        }

        public override void Prepare()
        {
            base.Prepare();
            foreach (var c in Columns)
            {
                c.Prepare();
            }
        }

        public override IExplore Expolore(DelegateExpessionExplorer del)
        {
            if (Columns != null && Columns.Count > 0)
            {
                var Columns2 = new TokenList<AlterColumnInfo>(this);
                foreach (var e in Columns)
                {
                    var t = (AlterColumnInfo)e.Expolore(del);
                    if (t != null) Columns2.Add(t);
                }
                Columns = Columns2;
            }
            return base.Expolore(del);
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("CREATE TABLE ");

            sb.Append(Table.ToSql(builder));
            sb.Append(" (");
            for (int i = 0; i < Columns.Count; i++)
            {
                if (i != 0) sb.Append(", ");
                var c = Columns[i];
                sb.Append(c.ToSql(builder));
            }
            sb.Append(")");
            return sb.ToString();
        }

        public override void ParseInside(ExpressionParser parser)
        {
            var collection = parser.Collection;
            var lex = collection.CurrentLexem();
            if (lex.LexemText.ToLower() != "create") parser.Collection.ErrorWaitKeyWord("CREATE", collection.CurrentLexem());
            lex = collection.GotoNextMust();
            if (lex.LexemText.ToLower() != "table") parser.Collection.ErrorWaitKeyWord("TABLE", collection.CurrentLexem());
            lex = collection.GotoNextMust();
            //название
            Table = TableName.Parse(parser);
            lex = collection.GotoNextMust();
            if (!lex.IsSkobraOpen()) parser.Collection.ErrorWaitKeyWord("(", collection.CurrentLexem());
            lex = collection.GotoNextMust();
            while (true)
            {
                AlterColumnInfo cd = ReadColumnInfo(collection);
                Columns.Add(cd);
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
            //переходим к следующей лексеме
            collection.GotoNext();
        }

        public static AlterColumnInfo ReadColumnInfo(LexemCollection collection)
        {
            var lex = collection.CurrentLexem();
            var col = CommonParserFunc.ReadColumnNameOnly(collection);
            AlterColumnInfo cd = new AlterColumnInfo();
            cd.Name = col;
            lex = collection.GotoNextMust();
            if (lex.LexemType != LexType.Command) collection.ErrorUnexpected(collection.CurrentLexem());
            ExactType? tp = ExactType.Parse(collection);
            if (tp == null) collection.Error("Unknow data type", collection.CurrentOrLast());
            cd.Type = tp.Value;
            lex = collection.GotoNext();
            while (lex != null && lex.LexemType != LexType.Zpt && !lex.IsSkobraClose())
            {
                string s1 = lex.LexemText.ToLower();
                if (ParserUtils.ParseCommandPhrase(collection, "not null"))
                {
                    cd.Nullable = false;
                }
                else if (ParserUtils.ParseCommandPhrase(collection, "null"))
                {
                    cd.Nullable = true;
                }
                else if (ParserUtils.ParseCommandPhrase(collection, "primary key"))
                {
                    cd.PrimaryKey = true;
                }
                else if (lex.LexemType == LexType.Command && (s1 == "auto_increment"))
                {
                    cd.AutoIncrement = true;
                }
                else collection.ErrorUnexpected(collection.CurrentOrLast());
                lex = collection.GotoNext();
            }
            return cd;
        }
    }
}
