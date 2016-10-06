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
        public HashSet<string> StopCommandLower = new HashSet<string>();

        public ExpressionParser(LexemCollection collection) : base(collection)
        {
        }

        public static Expression ParseCollection(LexemCollection collection)
        {
            ExpressionParser p = new ExpressionParser(collection);
            p.Parse();
            return p.Single();
        }

        public override void Parse()
        {
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
                            SubExpressionParser tt = new SubExpressionParser(Collection);
                            tt.Parse();
                            le.Expr = tt.Results[0];
                            le.Prior = 1;
                            OutExp.Add(le);
                            waitValue = false;
                            //continue;
                            Collection.GotoNext();
                        }
                        else
                        {
                            if (le.LexemType == LexType.Command && StopCommandLower.Contains(le.LexemText.ToLower()))
                            {
                                Finish();
                                return;
                            }
                            le.Expr = Collection.NodeFactory.GetNode(this);
                            if (le.Expr == null)
                            {
                                Finish();
                                return;
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
                                if (!le.Expr.IsLeftOperand() && waitValue) 
                                {
                                    Collection.Error("Two operation in expression", Collection.CurrentLexem());
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
                                if (!le.Expr.IsLeftOperand()) waitValue = true;
                            }
                            le.Expr.ParseInside(this);
                        }
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
