using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore.Expr.Sql;
using ParserCore.Expr.Simple;

namespace ParserCore
{
    public class ExpressionParser : CustomParser
    {
        public Stack<Lexem> OperStack = new Stack<Lexem>();
        public List<Lexem> OutExp = new List<Lexem>();
        public bool waitValue = true;//ожидаем значение

        public static Expression ParseCollection(LexemCollection collection)
        {
            ExpressionParser p = new ExpressionParser();
            p.Parse(collection);
            return p.Single();
        }

        public override void Parse(LexemCollection collection)
        {
            base.Parse(collection);
            while (!Collection.IsEnd())
            {
                //bool needNext = true;
                Lexem le = Collection.CurrentLexem();
                if (le.LexemType == LexType.Zpt)
                {
                    Finish();
                    return;
                }
                else
                    if (le.IsSkobraClose())// )
                    {
                        Finish();
                        return;
                    }
                    else
                        if (le.IsSkobraOpen())// (
                        {
                            if (!waitValue)
                            {
                                Finish();
                                return;
                            }
                            SubExpressionParser tt = new SubExpressionParser();
                            tt.Parse(Collection);
                            le.Expr = tt.Results[0];
                            le.Prior = 1;
                            OutExp.Add(le);
                            waitValue = false;
                            //continue;
                            Collection.GotoNext();
                        }
                        else
                        //if (le.LexemType == LexType.Arfimetic || le.LexemType == LexType.Command || le.LexemType == LexType.Number || le.LexemType == LexType.Text)
                        {
                            le.Expr = Collection.NodeFactory.GetNode(this);
                            if (le.Expr == null)
                            {
                                Finish();
                                return;
                                //collection.Error("Unknow lexem", collection.CurrentLexem());
                            }
                            le.Prior = le.Expr.Priority();

                            if (!le.Expr.IsOperation())
                            {
                                //Если это значение (константа, переменная, колонка и т.п.)
                                if (!waitValue)
                                {
                                    Finish();
                                    return;
                                }
                                OutExp.Add(le);
                            }
                            else
                            {//операция
                                if (!le.Expr.IsRightAssociate() && waitValue) {
                                    collection.Error("Two operation in expression", collection.CurrentLexem());
                                    }
                                while (GetTopStack(OperStack) != null)
                                {
                                    if (le.Prior < GetTopStack(OperStack).Prior)
                                    {
                                        OutExp.Add(OperStack.Pop());
                                    }
                                    else break;
                                }
                                OperStack.Push(le);
                            }
                            //настройки для следующего цикла
                            if (!le.Expr.IsOperation())
                            {
                                waitValue = false;
                            }
                            else
                            {
                                if (!le.Expr.IsRightAssociate()) waitValue = true;
                            }
                            le.Expr.ParseInside(this);
                            /*
                            if (le.Expr is InExpr)
                            {
                                var q = Collection.GotoNext();
                                if (q == null || !q.IsSkobraOpen())
                                    Collection.Error("Function arguments not found", Collection.GetPrev());
                                FuncArgsParser tt = new FuncArgsParser();
                                tt.Parse(Collection);
                                SubExpression sub = new SubExpression();
                                tt.Results.ForEach(a => sub.AddChild(a));
                                q.Expr = sub;
                                OutExp.Add(q);
                                waitValue = false;
                                dopuskUniar = true;
                            }
                            else if (le.Expr is CaseExpr)//ok
                            {
                                CaseParser cp = new CaseParser();
                                cp.Case = (CaseExpr) le.Expr;
                                cp.Parse(collection);
                            }
                            else if (le.Expr is SelectExpresion)
                            {
                                SelectParser selectParser = new SelectParser();
                                selectParser.SelectExpresion = le.Expr as SelectExpresion;
                                selectParser.Parse(collection);
                                needNext = false;
                            }
                            else if (le.Expr.IsFunction())//ok
                            {
                                var q = Collection.GotoNext();
                                if (q == null || !q.IsSkobraOpen())
                                    Collection.Error("Function arguments not found", Collection.GetPrev());
                                FuncArgsParser tt = new FuncArgsParser();
                                tt.Parse(Collection);
                                tt.Results.ForEach(a => le.Expr.AddChild(a));
                            }*/
                        }
                //if (needNext) Collection.GotoNext();
            }
            Finish();
        }

        private Lexem GetTopStack(Stack<Lexem> OperStack)
        {
            if (OperStack.Count > 0) return OperStack.Peek(); else return null;
        }

        private void Finish()
        {
            while (GetTopStack(OperStack) != null)
            {
                Lexem le = OperStack.Pop();
                OutExp.Add(le);
            }

            Stack<Lexem> sortStack = new Stack<Lexem>();
            for (int i = 0; i < OutExp.Count; i++)
            {
                Lexem le = OutExp[i];
                if (!le.Expr.IsOperation()) sortStack.Push(le);
                else
                {
                    if (le.Expr.NumChilds() < 0)
                    {
                        while (sortStack.Count > 0)
                        {
                            le.Expr.AddInvertChild(sortStack.Pop().Expr);
                        }
                    }
                    else
                    {
                        for (int ii = 0; ii < le.Expr.NumChilds(); ii++)
                        {
                            le.Expr.AddInvertChild(sortStack.Pop().Expr);
                        }
                    }
                    sortStack.Push(le);
                }
            }
            if (sortStack.Count > 1)
            {
                Collection.Error("Error in expression", sortStack.First());
            }
            if (sortStack.Count < 0)
            {
                Collection.Error("Error in expression", null);
            }
            var res = sortStack.Pop();
            OperStack.Clear();
            OutExp.Clear();
            Results.Add(res.Expr);
        }
    }
}
