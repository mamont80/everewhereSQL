using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore.Expr.Sql;

namespace ParserCore
{
    public class InsertStatement : OneTableStatement
    {
        public TokenList<Expression> ColumnOfValues { get; private set; }
        public TokenList<Expression> Values { get; private set; }

        private SelectExpresion _Select;
        public SelectExpresion Select
        {
            get { return _Select; }
            set
            {
                _Select = value;
                if (value != null) value.ParentToken = this;
            }
        }

        public bool DefaultValues = false;

        public InsertStatement():base()
        {
            ColumnOfValues = new TokenList<Expression>(this);
            Values = new TokenList<Expression>(this);
        }

        public override IExplore Expolore(DelegateExpessionExplorer del)
        {
            if (ColumnOfValues != null)
            {
                List<Expression> ColumnOfValues2 = new List<Expression>();
                ColumnOfValues.ForEach(a =>
                    {
                        Expression e2 = (Expression)a.Expolore(del);
                        if (e2 != null) ColumnOfValues2.Add(e2);
                    });
                ColumnOfValues.Replace(ColumnOfValues2);
            }
            if (Values != null)
            {
                List<Expression> Values2 = new List<Expression>();
                Values.ForEach(a =>
                {
                    Expression e2 = (Expression)a.Expolore(del);
                    if (e2 != null) Values2.Add(e2);
                });
                Values.Replace(Values2);
            }
            if (Select != null) Select.Expolore(del);
            return base.Expolore(del);
        }

        public override string ToStr()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("insert into ").Append(TableClause.ToStr());
            if (DefaultValues)
            {
                sb.Append(" DEFAULT VALUES");
            }
            else
            {
                if (ColumnOfValues.Count > 0)
                {
                    sb.Append(" ( ");
                    for (int i = 0; i < ColumnOfValues.Count; i++)
                    {
                        var sc = ColumnOfValues[i];
                        if (i > 0) sb.Append(", ");
                        sb.Append(sc.ToStr());
                    }
                    sb.Append(")");
                }
                if (Values != null && Values.Count > 0)
                {
                    sb.Append(" values(");
                    for (int i = 0; i < Values.Count; i++)
                    {
                        var sc = Values[i];
                        if (i > 0) sb.Append(", ");
                        sb.Append(sc.ToStr());
                    }
                    sb.Append(")");
                }
                else
                {
                    sb.Append(" ").Append(Select.ToStr());
                }
            }
            AddReturningToStr(sb);
            return sb.ToString();
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("insert into ").Append(TableClause.ToSql(builder));
            if (DefaultValues)
            {
                sb.Append(" DEFAULT VALUES");
                AddReturningToSql1(builder, sb);
            }
            else
            {
                if (ColumnOfValues.Count > 0)
                {
                    sb.Append(" ( ");
                    for (int i = 0; i < ColumnOfValues.Count; i++)
                    {
                        var sc = ColumnOfValues[i];
                        if (i > 0) sb.Append(", ");

                        if (!(sc is FieldExpr)) throw new Exception("В перечне колонок INSERT INTO () должны быть простые имена колонок");
                        FieldExpr fe = sc as FieldExpr;
                        sb.Append(fe.ToSqlShort(builder));
                    }
                    sb.Append(")");
                }
                AddReturningToSql1(builder, sb);
                if (Values != null && Values.Count > 0)
                {
                    sb.Append(" values(");
                    for (int i = 0; i < Values.Count; i++)
                    {
                        var sc = Values[i];
                        if (i > 0) sb.Append(", ");
                        sb.Append(sc.ToSql(builder));
                    }
                    sb.Append(")");
                }
                else
                {
                    sb.Append(" ").Append(Select.ToSql(builder));
                }
            }
            AddReturningToSql2(builder, sb);
            return sb.ToString();
        }

        public override void Prepare()
        {
            base.Prepare();

            if (ColumnOfValues != null)
            {
                foreach (var sc in ColumnOfValues)
                {
                    sc.Prepare();
                }
            }
            if (Values != null)
            {
                foreach (var sc in Values)
                {
                    sc.Prepare();
                }
            }
            if (Select != null) Select.Prepare();
        }

        public override void ParseInside(ExpressionParser parser)
        {
            var collection = parser.Collection;
            var lex = collection.CurrentLexem();
            if (lex.LexemText.ToLower() != "insert") throw new Exception("Not INSERT statment");
            lex = collection.GotoNextMust();
            if (lex.LexemText.ToLower() != "into") throw new Exception("INTO keyword is not found");
            lex = collection.GotoNextMust();
            string[] tablename = CommonParserFunc.ReadTableName(collection);
            // TODO: Fixed! ok
            TableClause = TableClause.CreateByTable(tablename, collection.TableGetter.GetTableByName(tablename));
            lex = collection.GotoNextMust();
            if (lex.IsSkobraOpen())
            {
                while (true)
                {
                    lex = collection.GotoNextMust(); //пропускаем SET или ','
                    //lex = collection.CurrentLexem();

                    var col = CommonParserFunc.ReadColumn(collection);
                    ColumnOfValues.Add(col);
                    lex = collection.GotoNextMust();
                    if (lex == null) break;
                    if (lex.LexemType == LexType.Zpt) continue;
                    if (lex.IsSkobraClose()) break;
                    collection.Error("Unknow lexem", collection.CurrentLexem());
                }
                //пропускаем ')'
                lex = collection.GotoNextMust();
            }
            else
            {
                if (lex.LexemType == LexType.Command && lex.LexemText.ToLower() == "default")
                {
                    lex = collection.GotoNextMust();
                    if (lex.LexemType == LexType.Command && lex.LexemText.ToLower() == "values")
                    {
                        DefaultValues = true;
                        return;
                    }
                    else collection.Error("Expected keyword VALUES", lex);
                }
            }

            if (lex == null) return;
            if (lex.LexemText.ToLower() == "values")
            {
                lex = collection.GotoNextMust();
                if (!lex.IsSkobraOpen()) collection.Error("'(' not found", lex);

                while (true)
                {
                    lex = collection.GotoNextMust(); //пропускаем SET или ','
                    //lex = collection.CurrentLexem();

                    ExpressionParser e = new ExpressionParser();
                    e.Parse(collection);
                    Values.Add(e.Single());
                    lex = collection.CurrentLexem();
                    if (lex == null) break;
                    if (lex.LexemType == LexType.Zpt) continue;
                    if (lex.IsSkobraClose()) break;
                    collection.Error("Unknow lexem", collection.CurrentLexem());
                }
                lex = collection.GotoNext();
            }
            else
                if (lex.LexemText.ToLower() == "select" || lex.IsSkobraOpen())
                {
                    ExpressionParser e = new ExpressionParser();
                    e.Parse(collection);
                    var expr = e.Single();
                    var sel = ParserUtils.FindSelect(expr);
                    if (sel == null) throw new Exception("Values in INSERT not found");
                    Select = sel;
                }
            ParseReturining(parser);
        }

    }
}
