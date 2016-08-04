using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public class ExpressionToNode2 : CustomToNode
    {
        private Stack<LexExpr> OperStack = new Stack<LexExpr>();
        private List<LexExpr> OutExp = new List<LexExpr>();
        private bool dopuskUniar = true;
        private bool waitValue = true;//ожидаем значение

        public override void Parse(LexemCollection collection)
        {
            base.Parse(collection);
            while (!Collection.IsEnd())
            {
                bool needNext = true;
                LexExpr le = Collection.CurrentLexem();
                if (le.LexemType == LexType.Zpt)
                {
                    Finish(true);
                    return;
                }
                else
                    if (le.IsSkobraClose())// )
                    {
                        Finish(true);
                        return;
                    }
                    else
                        if (le.IsSkobraOpen())// (
                        {
                            if (!waitValue)
                            {
                                Finish(true);
                                return;
                            }
                            SubExpressionToNode2 tt = new SubExpressionToNode2();
                            tt.Parse(Collection);
                            le.Expr = tt.Results[0];
                            le.Prior = 1;
                            OutExp.Add(le);
                            waitValue = false;
                            dopuskUniar = false;
                            //continue;
                        }
                        else
                        //if (le.LexemType == LexType.Arfimetic || le.LexemType == LexType.Command || le.LexemType == LexType.Number || le.LexemType == LexType.Text)
                        {
                            le.Expr = Collection.NodeFactory.GetNode(le, dopuskUniar, Collection);
                            if (le.Expr == null)
                            {
                                collection.Error("Unknow lexem " + le.Lexem, collection.CurrentLexem());
                            }
                            le.Prior = le.Expr.Priority();

                            if (!le.Expr.IsOperation() || le.Expr.IsVirtualField)
                            {
                                //Если это значение (константа, переменная, колонка и т.п.)
                                if (!waitValue)
                                {
                                    Finish(true);
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
                            if (!le.Expr.IsOperation() || le.Expr.IsVirtualField)
                            {
                                waitValue = false;
                                dopuskUniar = false;
                            }
                            else
                            {
                                dopuskUniar = true;
                                if (!le.Expr.IsRightAssociate()) waitValue = true;
                            }
                            if (le.Expr is InExpr)
                            {
                                var q = Collection.GotoNext();
                                if (q == null || !q.IsSkobraOpen())
                                    Collection.Error("Function arguments not found", Collection.GetPrev());
                                FuncArgsToNode2 tt = new FuncArgsToNode2();
                                tt.Parse(Collection);
                                SubExpression sub = new SubExpression();
                                tt.Results.ForEach(a => sub.AddChild(a));
                                q.Expr = sub;
                                OutExp.Add(q);
                                waitValue = false;
                                dopuskUniar = true;
                            }
                            else
                            if (le.Expr is SelectExpresion)
                            {
                                SelectToNode2 selectToNode2 = new SelectToNode2();
                                selectToNode2.SelectExpresion = le.Expr as SelectExpresion;
                                selectToNode2.Parse(collection);
                                needNext = false;
                            }else
                            if (le.Expr.IsFunction() && !le.Expr.IsVirtualField)
                            {
                                var q = Collection.GotoNext();
                                if (q == null || !q.IsSkobraOpen())
                                    Collection.Error("Function arguments not found", Collection.GetPrev());
                                FuncArgsToNode2 tt = new FuncArgsToNode2();
                                tt.Parse(Collection);
                                tt.Results.ForEach(a => le.Expr.AddChild(a));
                            }
                        }
                if (needNext) Collection.GotoNext();
            }
            Finish(false);
        }

        private LexExpr GetTopStack(Stack<LexExpr> OperStack)
        {
            if (OperStack.Count > 0) return OperStack.Peek(); else return null;
        }

        private void Finish(bool noWaitLexemFound)
        {
            noWaitLexem = noWaitLexemFound;
            while (GetTopStack(OperStack) != null)
            {
                LexExpr le = OperStack.Pop();
                OutExp.Add(le);
            }

            Stack<LexExpr> sortStack = new Stack<LexExpr>();
            for (int i = 0; i < OutExp.Count; i++)
            {
                LexExpr le = OutExp[i];
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
