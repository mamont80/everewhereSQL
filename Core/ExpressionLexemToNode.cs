using System;
using System.Collections.Generic;

namespace ParserCore
{
    /* Осуществляет преобразование лексем в ноды
     * Типы выражений:
     * Значения
     *   Константы (числовые и строковые)
     *   Поле(колонка)
     *   Переменная
     * Операции над значениями
     *   + - * / UniarMinus And Or Not ...
     * Функции
     *  SQRT ABS Contain ...
     */

    

    

    

    public class ExpressionLexemToNode
    {
        /// <summary>
        /// Дефолтовая фабрика объектов нод выражений
        /// </summary>
        public BaseExpressionFactory NodeFactory = new BaseExpressionFactory();

        private LexemCollection Lexems;
        public int IndexLexem;
        public LexExpr ParentLexem;
        public bool IsParamsArray = false; //перечень параметров arg1, arg2, arg3
        public List<Expression> ParamsArray = new List<Expression>();
        public bool WaitSkobka = false;

        public LexExpr CurrentLexem()
        {
            if (IndexLexem < Lexems.Count) return Lexems[IndexLexem];
            return null;
        }

        public LexExpr SeekLexem(int delta)
        {
            int i = IndexLexem + delta;
            if (i < 0 || i >= Lexems.Count) return null;
            return Lexems[i];
        }

        private LexExpr GetTopStack(Stack<LexExpr> OperStack)
        {
            if (OperStack.Count > 0) return OperStack.Peek(); else return null;
        }

        public void Init(LexemCollection lexems)
        {
            Lexems = lexems;
            IndexLexem = 0;
        }

        public Expression ToNode(LexemCollection lexems)
        {
            if (lexems.Count == 0) return null;
            Init(lexems);
            return GetExpr();
        }

        private void InitGetExpr()
        {
            OperStack = new Stack<LexExpr>();
            OutExp = new List<LexExpr>();
            dopuskUniar = true;
            waitValue = true;//ожидаем значение
        }

        public ExpressionLexemToNode Clone()
        {
            ExpressionLexemToNode r = new ExpressionLexemToNode();
            r.NodeFactory = NodeFactory;
            r.Lexems = Lexems;
            r.IndexLexem = IndexLexem;
            return r;
        }

        private Stack<LexExpr> OperStack = new Stack<LexExpr>();
        private List<LexExpr> OutExp = new List<LexExpr>();
        private bool dopuskUniar = true;
        private bool waitValue = true;//ожидаем значение
        public HashSet<string> StopWords = new HashSet<string>();
        public bool StopOnSecondValue = false;
        public bool StopOnZpt = false;
        public bool HasStopOnSecondValue = false;
        public bool HasStopOnWord = false;
        public bool HasStopOnZpt = false;

        private void ReadArguments()
        {
            while (true)
            {
                var r = GetExpr();
                if (r != null) ParamsArray.Add(r);
                else return;
                if ( !(CurrentLexem() != null && CurrentLexem().LexemType == LexType.Zpt) ) break;
                IndexLexem++;
            }
        }

        public Expression GetExpr()
        {
            InitGetExpr();
            while (IndexLexem < Lexems.Count)
            {
                LexExpr le = Lexems[IndexLexem];
                if (le.LexemType == LexType.Zpt)
                {
                    if (StopOnZpt)
                    {
                        HasStopOnZpt = true;
                        return Finish();
                    }
                    if (!IsParamsArray) throw new Exception("Неожиданная запятая");
                    return Finish();
                }
                if (le.IsSkobraClose())// )
                {
                    if (!WaitSkobka) throw new Exception("Неожиданно закрывающая скобка");
                    if (OutExp.Count == 0 && ParentLexem != null && ParentLexem.Expr.IsFunction()) return null;//функция без аргументов
                    return Finish();
                }
                if (le.IsSkobraOpen())// (
                {
                    if (!waitValue) throw new Exception("Неожиданная скобка");
                    le.Expr = new SubExpression();
                    le.Prior = 1;
                    ReadSubExpression(le);
                    OutExp.Add(le);
                    waitValue = false;
                    dopuskUniar = false;
                    continue;
                }
                if (le.LexemType == LexType.Arfimetic || le.LexemType == LexType.Command || le.LexemType == LexType.Number || le.LexemType == LexType.Text)
                {
                    if (le.LexemType == LexType.Command && StopWords.Contains(le.Lexem.ToLower()))
                    {
                        HasStopOnWord = true;
                        break;
                    }
                    le.Expr = NodeFactory.GetNode(le, dopuskUniar, Lexems);
                    if (le.Expr == null) throw new Exception("Unknow lexem " + le.Lexem);
                    le.Prior = le.Expr.Priority();

                    if (!le.Expr.IsOperation() || le.Expr.IsVirtualField)
                    {
                        //Если это значение (константа, переменная, колонка и т.п.)
                        if (!waitValue) 
                        {
                            if (StopOnSecondValue)
                            {
                                HasStopOnSecondValue = true;
                                break;
                            }
                            throw new Exception("Two operand");//два значения подряд
                        }
                        OutExp.Add(le);
                    }
                    else
                    {//операция
                        if (!le.Expr.IsRightAssociate() && waitValue) throw new Exception("Two operation");//две операции подряд
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
                    if (le.Expr.IsFunction() && !le.Expr.IsVirtualField)
                    {
                        ReadFunctionArgumets(le);
                        
                        continue;//не делаем IndexLexem++;
                    }
                }
                IndexLexem++;
            }
            return Finish();
        }

        private void ReadSubExpression(LexExpr lex)
        {
            var l = Lexems[IndexLexem];
            if (!l.IsSkobraOpen()) throw new Exception("Нет открывающихся скобок");
            IndexLexem++;
            var r = Clone();
            r.ParentLexem = lex;
            r.WaitSkobka = true;
            r.IsParamsArray = true;
            r.ReadArguments();
            IndexLexem = r.IndexLexem;//должна быть закрывающая скобка
            if (CurrentLexem() == null || !CurrentLexem().IsSkobraClose()) throw new Exception("Нет закрывающей скобки");
            IndexLexem++;
            if (r.ParamsArray.Count == 0) throw new Exception("Empty subexpression");
            foreach (var expression in r.ParamsArray)
            {
                lex.Expr.AddChild(expression);
            }
            return;
        }

        private void ReadFunctionArgumets(LexExpr lex)
        {
            IndexLexem++;
            if (IndexLexem >= Lexems.Count) throw new Exception("Нет аргументов функции");
            var l = Lexems[IndexLexem];
            if (!l.IsSkobraOpen()) throw new Exception("Нет открывающихся скобок");
            IndexLexem++;
            var r = Clone();
            r.ParentLexem = lex;
            r.WaitSkobka = true;
            r.IsParamsArray = true;
            r.ReadArguments();
            IndexLexem = r.IndexLexem;//должна быть закрывающая скобка
            if (CurrentLexem() == null || !CurrentLexem().IsSkobraClose()) throw new Exception("Нет закрывающей скобки");
            IndexLexem++;
            foreach (var expression in r.ParamsArray)
            {
                lex.Expr.AddChild(expression);
            }
            return;
        }

        private Expression Finish()
        {
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
            if (sortStack.Count != 1) throw new Exception("не знаю как назвать");
            var res = sortStack.Pop().Expr;
            OperStack.Clear();
            OutExp.Clear();
            return res;
        }

        /*
        private Expression GetExpr2(bool inFunc = false)
        {
            Stack<LexExpr> OperStack = new Stack<LexExpr>();
            List<LexExpr> OutExp = new List<LexExpr>();
            bool wasOp2 = false;//операция между двумя аргументами  arg op2 arg
            bool wasOp1 = false;//операция перед аргументом op1 arg
            int numSkob = 0;
            while (IndexLexem < lexems.Count)
            {
                KeyValuePair<string, LexType> kvp = lexems[IndexLexem];
                LexExpr le = new LexExpr();
                le.Lexem = kvp.Key;
                le.LexemType = kvp.Value;
                if (le.LexemType == LexType.Skobka)
                {
                    if (le.Lexem == "(")
                    {
                        le.Prior = 1;
                        OperStack.Push(le);
                        numSkob++;
                        wasOp2 = false;
                    }
                    else // le.Lexem == ")"
                    {
                        if (numSkob == 0)
                        {
                            if (inFunc) break;// немедленно выходим. Конец функции
                                else throw new Exception("лишние скобки");
                        }
                        if (wasOp2) throw new Exception("Syntaxis error");
                        else
                        {
                            while (GetTopStack(OperStack) != null)
                            {
                                if (GetTopStack(OperStack).LexemType == LexType.Skobka && GetTopStack(OperStack).Lexem == "(")
                                {
                                    OperStack.Pop();
                                    break;
                                }
                                OutExp.Add(OperStack.Pop());
                            }
                            if (GetTopStack(OperStack) != null && GetTopStack(OperStack).Expr != null && GetTopStack(OperStack).Expr.IsFunction())
                            {
                                OutExp.Add(OperStack.Pop());
                            }
                            numSkob--;
                        }
                    }
                }
                if (le.LexemType == LexType.Zpt)
                {
                    if (inFunc) break;//конец очередного параметра функции
                }

                if (le.LexemType != LexType.Skobka && le.LexemType != LexType.Zpt)
                {
                    le.Expr = null;
                    if (le.Lexem == "-" || le.Lexem == "+")//частный случай - унарный минус
                    {
                        if (IndexLexem == 0 || (lexems[IndexLexem - 1].Value == LexType.Skobka && lexems[IndexLexem - 1].Key == "(") || (lexems[IndexLexem - 1].Value == LexType.Zpt) || (lexems[IndexLexem - 1].Value == LexType.Arfimetic))
                        {
                            if (le.Lexem == "-") le.Expr = new UniarMinus_BoolExpr();
                            if (le.Lexem == "+") le.Expr = new UniarPlus_BoolExpr();
                        }
                    }
                    if (le.Lexem == "*")//частный случай - (*) для count(*)
                    {
                        if (IndexLexem == 0 || (lexems[IndexLexem - 1].Value == LexType.Skobka && lexems[IndexLexem - 1].Key == "(") || (lexems[IndexLexem - 1].Value == LexType.Zpt))
                            le.Expr = new AllColumnExpr();
                    }
                    if (le.Expr == null) le.Expr = NodeFactory.GetNode(kvp.Key, kvp.Value, true);

                    if (le.Expr == null) throw new Exception("Unknow lexem " + kvp.Key);
                    le.Prior = le.Expr.Priority();
                    if ((!le.Expr.IsOperation() && !le.Expr.IsFunction()) || le.Expr.IsVirtualField)
                    {
//Если это значение (константа, переменная, колонка и т.п.)
                        OutExp.Add(le);
                        wasOp2 = false;
                    }
                    else 
                    {
                        if (le.Expr.IsOperation())
                        {//это операция 
                            
                            if (le.Expr.IsRightAssociate())
                                wasOp2 = false;
                            else
                            {
                                if (wasOp2) throw new Exception("Syntaxis error");
                                wasOp2 = true;
                            }
                            while (GetTopStack(OperStack) != null)
                            {
                                if (le.Prior < GetTopStack(OperStack).Prior)
                                {
                                    OutExp.Add(OperStack.Pop());
                                }
                                else break;
                            }
                            if (!le.Expr.IsFunction()) //Если операция ещё и функция ( операция IN) то не добавляем её в стек. Она обработается для случая функции
                                OperStack.Push(le);
                        }
                        if (le.Expr.IsFunction())
                        {
                            //читаем параметры
                            IndexLexem++;
                            if (IndexLexem >= lexems.Count ||
                                !(lexems[IndexLexem].Value == LexType.Skobka && lexems[IndexLexem].Key == "(")) throw new Exception("нет открывающихся скобок");
                            IndexLexem++;
                            while (true)
                            {
                                if (IndexLexem < lexems.Count && (lexems[IndexLexem].Value == LexType.Skobka && lexems[IndexLexem].Key == ")"))
                                {//конец функции
                                    break;
                                }
                                le.Expr.AddChild(GetExpr2(true));
                                if (IndexLexem < lexems.Count && (lexems[IndexLexem].Value == LexType.Zpt))
                                {//следующий параметр
                                    IndexLexem++;
                                    continue;
                                }
                                if (IndexLexem >= lexems.Count) throw new Exception("неожиданный конец выражения");
                            }
                            OutExp.Add(le);
                            wasOp2 = false;
                            //wasOp = false;
                            //OperStack.Push(le);
                        }
                    }
                }
                IndexLexem++;
            }
            while (GetTopStack(OperStack) != null)
            {
                LexExpr le = OperStack.Pop();
                if (le.LexemType != LexType.Skobka) OutExp.Add(le);
            }
            if (numSkob != 0) throw new Exception("Wrong number of brackets");

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
            return sortStack.Pop().Expr;

        }*/
    }
}
