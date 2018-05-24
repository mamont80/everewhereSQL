using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore.Expr.Extend;
using ParserCore.Expr.Simple;
using ParserCore.Expr.Sql;

namespace ParserCore
{
    public class ExtendFunctions: IExpressionFactory
    {
        public Expression GetNode(ExpressionParser parser)
        {
            Lexem lex = parser.Collection.CurrentLexem();
            Expression ex = null;
            bool uniar = parser.waitValue;
            if (lex.LexemType == LexType.Arfimetic)
            {
                switch (lex.LexemText)
                {
                    case "+":
                        if (uniar) ex = new UniarPlus_BoolExpr();
                        else
                            ex = new Plus_Arifmetic();
                        break;
                    case "-":
                        if (uniar) ex = new UniarMinus_BoolExpr();
                        else
                            ex = new Minus_Arifmetic();
                        break;
                    case "*":
                        if (uniar) ex = new AllColumnExpr();
                        else
                            ex = new Multi_Arifmetic();
                        break;
                    case "/":
                        ex = new Div_Arifmetic();
                        break;
                    case "<":
                        ex = new Less_CompExpr();
                        break;
                    case "<=":
                        ex = new LessOrEqual_CompExpr();
                        break;
                    case ">=":
                        ex = new GreatOrEqual_CompExpr();
                        break;
                    case ">":
                        ex = new Great_CompExpr();
                        break;
                    case "=":
                        ex = new Equal_CompExpr();
                        break;
                    case "<>":
                    case "!=":
                        ex = new NotEqual_CompExpr();
                        break;
                }
            }
            if (lex.LexemType == LexType.Number)
            {
                ex = new ConstExpr();
                if (lex.LexemText.Contains('.'))
                {
                    (ex as ConstExpr).Init(lex.LexemText.ParseDouble(), SimpleTypes.Float);
                }
                else (ex as ConstExpr).Init(long.Parse(lex.LexemText), SimpleTypes.Integer);
            }
            if (lex.LexemType == LexType.Text)
            {
                if (lex.LexemText.StartsWith("'"))
                {
                    ex = new ConstExpr();
                    (ex as ConstExpr).Init(ParserUtils.StandartDecodeEscape(lex.LexemText), SimpleTypes.String);
                }
            }
            Lexem n1;
            if (lex.LexemType == LexType.Command)
            {
                switch (lex.LexemText.ToLower())
                {
                    case "not":
                        n1 = parser.Collection.GetNext();
                        if (n1 != null && n1.LexemType == LexType.Command && n1.LexemText.ToLower() == "in")
                        {
                            ex = new NotInExpr();
                            parser.Collection.GotoNext();
                            break;
                        }
                        ex = new Not_BoolExpr();
                        break;
                    case "case":
                        ex = new CaseExpr();
                        break;
                    case "contains":
                        ex = new Contains();
                        break;
                    case "containsic": //ic = ignore case
                    case "containscase":
                        ex = new ContainsIgnoreCase();
                        break;
                    case "startwith":
                    case "startswith":
                        ex = new StartsWith();
                        break;
                    case "endwith":
                    case "endswith":
                        ex = new EndsWith();
                        break;
                }
                if (parser.Collection.GetNext() != null && parser.Collection.GetNext().IsSkobraOpen())
                {
                    switch (lex.LexemText.ToLower())
                    {
                        case "cast":
                            ex = new Cast();
                            break;
                        case "in":
                            ex = new InExpr();
                            break;
                        case "abs":
                            ex = new Abs();
                            break;
                        case "substring":
                            ex = new SubString();
                            break;
                        case "position":
                            ex = new Position();
                            break;
                        case "ltrim":
                            ex = new LTrim();
                            break;
                        case "rtrim":
                            ex = new RTrim();
                            break;
                        case "trim":
                            ex = new Trim();
                            break;
                        case "length":
                            ex = new Length();
                            break;
                        case "upper":
                            ex = new Upper_operation();
                            break;
                        case "lower":
                            ex = new Lower_operation();
                            break;
                        case "left":
                            ex = new Left();
                            break;
                        case "right":
                            ex = new Right();
                            break;
                        case "now":
                            ex = new Now();
                            break;
                        case "tostr":
                            ex = new ToString();
                            break;
                        case "strtotime":
                            ex = new StrToTime();
                            break;
                        case "strtodatetime":
                            ex = new StrToDateTime();
                            break;
                        case "addseconds":
                            ex = new AddSeconds();
                            break;
                        case "addminutes":
                            ex = new AddMinutes();
                            break;
                        case "addhours":
                            ex = new AddHours();
                            break;
                        case "adddays":
                            ex = new AddDays();
                            break;
                        case "day":
                            ex = new Day();
                            break;
                        case "month":
                            ex = new Month();
                            break;
                        case "year":
                            ex = new Year();
                            break;
                        case "round":
                            ex = new ParserCore.Expr.Extend.Math.Round();
                            break;
                        case "pi":
                            ex = new ParserCore.Expr.Extend.Math.Pi();
                            break;
                        case "tan":
                            ex = new ParserCore.Expr.Extend.Math.Tan();
                            break;
                        case "log":
                            ex = new ParserCore.Expr.Extend.Math.Log();
                            break;
                    }
                }
            }
            return ex;
        }
    }
}
