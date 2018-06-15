using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using ParserCore.Expr.Simple;
using ParserCore.Expr.Sql;

namespace ParserCore
{
    public class SimpleExpressionFactory : IExpressionFactory
    {
        public Expression GetNode(ExpressionParser parser)
        {
            var collection = parser.Collection;
            Lexem lex = parser.Collection.CurrentLexem();
            bool uniar = parser.waitValue;
            Expression res = null;
            if (lex.LexemType == LexType.Arfimetic)
            {
                switch (lex.LexemText)
                {
                    case "+":
                        if (uniar) res = new UniarPlus_BoolExpr();
                        else
                        res = new Plus_Arifmetic();
                        break;
                    case "-":
                        if (uniar) res = new UniarMinus_BoolExpr();
                        else
                            res = new Minus_Arifmetic();
                        break;
                    case "*":
                        if (uniar) res = new AllColumnExpr();
                        else
                            res = new Multi_Arifmetic();
                        break;
                    case "/":
                        res = new Div_Arifmetic();
                        break;
                    case "<":
                        res = new Less_CompExpr();
                        break;
                    case "<=":
                        res = new LessOrEqual_CompExpr();
                        break;
                    case ">=":
                        res = new GreatOrEqual_CompExpr();
                        break;
                    case ">": 
                        res = new Great_CompExpr();
                        break;
                    case "=":
                        res = new Equal_CompExpr();
                        break;
                    case "<>":
                    case "!=":
                        res = new NotEqual_CompExpr();
                        break;
                }
            }
            if (lex.LexemType == LexType.Number)
            {
                res = new ConstExpr();
                if (lex.LexemText.Contains('.'))
                {
                    (res as ConstExpr).Init(lex.LexemText.ParseDouble(), SimpleTypes.Float);
                }
                else (res as ConstExpr).Init(long.Parse(lex.LexemText), SimpleTypes.Integer);
            }
            if (lex.LexemType == LexType.Text)
            {
                if (lex.LexemText.StartsWith("'"))
                {
                    res = new ConstExpr();
                    (res as ConstExpr).Init(ParserUtils.StandartDecodeEscape(lex.LexemText), SimpleTypes.String);
                }
            }
            if (lex.LexemType == LexType.Command)
            {
                if (lex.LexemText.StartsWith("@"))
                {
                    var varName = lex.LexemText;
                    for (int i = 0; i < collection.ParamDeclarations.Count; i++)
                    {
                        var pd = collection.ParamDeclarations[i];
                        if (pd.Name == varName)
                        {
                            var tp = collection.DotNetTypeToSimpleType(pd.Value);
                            if (tp == null) collection.Error("Unknow variable type ("+pd.Name+")", lex);
                            res = new VariableExpr();
                            ((VariableExpr)res).VariableName = lex.LexemText;
                            ((VariableExpr)res).Bind(pd.Value, tp.Value);
                        }
                    }
                }
                switch (lex.LexemText.ToLower())
                { 
                    case "not":
                        res = new Not_BoolExpr();
                        break;
                    case "and":
                        res = new And_BoolExpr();
                        break;
                    case "or":
                        res = new Or_BoolExpr();
                        break;
                    case "true":
                        res = new ConstExpr();
                        (res as ConstExpr).Init(true, SimpleTypes.Boolean);
                        break;
                    case "false":
                        res = new ConstExpr();
                        (res as ConstExpr).Init(false, SimpleTypes.Boolean);
                        break;
                    case "date":
                        if (collection.GetNext() != null)
                        {
                            var c = collection.GetNext();
                            if (c.LexemType == LexType.Text && c.LexemText.StartsWith("'"))
                            {
                                res = new ConstExpr();
                                string s = ParserUtils.StandartDecodeEscape(c.LexemText);
                                var dt = CommonUtils.ParseDateTime(s);
                                if (dt != null)
                                {
                                    collection.GotoNext();
                                    ((ConstExpr)res).Init(dt.Value, SimpleTypes.Date);
                                }
                                else collection.Error("Can not parse date lexem", lex);
                            }
                        }
                        break;
                    case "datetime":
                    case "timestamp":
                        if (collection.GetNext() != null)
                        {
                            var c = collection.GetNext();
                            if (c.LexemType == LexType.Text && c.LexemText.StartsWith("'"))
                            {
                                res = new ConstExpr();
                                string s = ParserUtils.StandartDecodeEscape(c.LexemText);
                                var dt = CommonUtils.ParseDateTime(s);
                                if (dt != null)
                                {
                                    collection.GotoNext();
                                    ((ConstExpr)res).Init(dt.Value, SimpleTypes.DateTime);
                                }
                                else collection.Error("Can not parse datetime lexem", lex);
                            }
                        }
                        break;
                    case "time":
                        if (collection.GetNext() != null)
                        {
                            var c = collection.GetNext();
                            if (c.LexemType == LexType.Text && c.LexemText.StartsWith("'"))
                            {
                                res = new ConstExpr();
                                string s = ParserUtils.StandartDecodeEscape(c.LexemText);
                                DateTime dt;
                                var r = CommonUtils.ParseDateTime(s, out dt);
                                if (r == ParserDateTimeStatus.Time)
                                {
                                    collection.GotoNext();
                                    ((ConstExpr)res).Init(dt.TimeOfDay, SimpleTypes.Time);
                                }
                                else collection.Error("Can not parse time lexem", lex);
                            }
                        }
                        break;

                }
            }
            return res;
        }
    }
}
