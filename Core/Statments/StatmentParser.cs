using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LayerData;

namespace TableQuery
{
	public class StatmentParser
	{
	    public Statment Parse(LexemCollection collection)
	    {
	        var lex = collection.CurrentLexem();
            if (lex == null) throw new Exception("Text is empty");
            if (lex.Lexem.ToLower() == "select")
            {
                return ParseSelect(collection);
            }
            if (lex.Lexem.ToLower() == "update")
            {
                return ParseUpdate(collection);
            }
	        if (lex.Lexem.ToLower() == "delete")
	        {
	            return ParseDelete(collection);
	        }
	        if (lex.Lexem.ToLower() == "insert")
	        {
	            return ParseInsert(collection);
	        }
	        throw new Exception("Unknow statment");
	    }

        public DeleteStatment ParseDelete(LexemCollection collection)
        {
            DeleteStatment stmt = new DeleteStatment();
            var lex = collection.CurrentLexem();
            if (lex.Lexem.ToLower() != "delete") throw new Exception("Not DELETE statment");
            lex = collection.GotoNextMust();
            if (lex.Lexem.ToLower() != "from") throw new Exception("Keyword 'FROM' is not found");
            lex = collection.GotoNextMust();
            string[] tablename = CommonFunc.ReadTableName(collection);
            // TODO: fixed!
            stmt.Table = SelectTable.CreateByTable(collection.TableGetter.GetTableByName(tablename));
            
            lex = collection.GotoNextMust();

            if (lex == null) return stmt;
            if (lex.Lexem.ToLower() == "where")
            {
                collection.GotoNextMust();
                ExpressionToNode2 e = new ExpressionToNode2();
                e.Parse(collection);
                stmt.Where = e.Single();
            }
            return stmt;
        }

	    public SelectStatment ParseSelect(LexemCollection collection)
	    {
            SelectStatment stmt = new SelectStatment();
            var lex = collection.CurrentLexem();
            if (lex.Lexem.ToLower() != "select") throw new Exception("Not SELECT statment");

            ExpressionToNode2 expToNode = new ExpressionToNode2();
            expToNode.Parse(collection);
            SelectExpresion select = expToNode.Single() as SelectExpresion;
            FieldCreator fa = new FieldCreator(collection);
            fa.MakeFields(select);
            stmt.Select = select;
	        return stmt;
	    }

	    public UpdateStatment ParseUpdate(LexemCollection collection)
        {
            UpdateStatment stmt = new UpdateStatment();
            var lex = collection.CurrentLexem();
            if (lex.Lexem.ToLower() != "update") throw new Exception("Not UPDATE statment");
	        lex = collection.GotoNextMust();
            string[] tablename = CommonFunc.ReadTableName(collection);
            // TODO: fixed!
            stmt.Table = SelectTable.CreateByTable(collection.TableGetter.GetTableByName(tablename));

	        lex = collection.GotoNextMust();
            if (lex.Lexem.ToLower() != "set")
            {
                collection.Error("SET keyword is not found", collection.CurrentLexem());
            }
            

            while (true)
            {
                lex = collection.GotoNextMust();//пропускаем SET или ','
                //lex = collection.CurrentLexem();
                
                SetClause sc = new SetClause();
                sc.Column = CommonFunc.ReadColumn(collection);
                lex = collection.GotoNextMust();
                if (lex.Lexem != "=") collection.Error("Operator '=' is not found", collection.CurrentLexem());
                lex = collection.GotoNextMust();
                ExpressionToNode2 e = new ExpressionToNode2();
                e.Parse(collection);
                sc.Value = e.Single();
                stmt.Set.Add(sc);
                lex = collection.CurrentLexem();
                if (lex == null) break;
                if (lex.LexemType == LexType.Zpt) continue;
                break;
            }

            if (lex == null) return stmt;
            if (lex.Lexem.ToLower() == "where")
            {
                collection.GotoNextMust();
                ExpressionToNode2 e = new ExpressionToNode2();
                e.Parse(collection);
                stmt.Where = e.Single();
            }
            return stmt;
        }

        public InsertStatment ParseInsert(LexemCollection collection)
        {
            InsertStatment stmt = new InsertStatment();
            var lex = collection.CurrentLexem();
            if (lex.Lexem.ToLower() != "insert") throw new Exception("Not INSERT statment");
            lex = collection.GotoNextMust();
            if (lex.Lexem.ToLower() != "into") throw new Exception("INTO keyword is not found");
            lex = collection.GotoNextMust();
            string[] tablename = CommonFunc.ReadTableName(collection);
            // TODO: Fixed!
            stmt.Table = SelectTable.CreateByTable(collection.TableGetter.GetTableByName(tablename));
            lex = collection.GotoNextMust();
            if (lex.IsSkobraOpen())
            {
                while (true)
                {
                    lex = collection.GotoNextMust(); //пропускаем SET или ','
                    //lex = collection.CurrentLexem();

                    var col = CommonFunc.ReadColumn(collection);
                    stmt.Columns.Add(col);
                    lex = collection.GotoNextMust();
                    if (lex == null) break;
                    if (lex.LexemType == LexType.Zpt) continue;
                    if (lex.IsSkobraClose()) break;
                    collection.Error("Unknow lexem", collection.CurrentLexem());
                }
                //пропускаем ')'
                lex = collection.GotoNextMust();
            }

            if (lex == null) return stmt;
            if (lex.Lexem.ToLower() == "values")
            {
                lex = collection.GotoNextMust();
                if (!lex.IsSkobraOpen()) collection.Error("'(' not found", lex);

                while (true)
                {
                    lex = collection.GotoNextMust(); //пропускаем SET или ','
                    //lex = collection.CurrentLexem();

                    ExpressionToNode2 e = new ExpressionToNode2();
                    e.Parse(collection);
                    stmt.Values.Add(e.Single());
                    lex = collection.CurrentLexem();
                    if (lex == null) break;
                    if (lex.LexemType == LexType.Zpt) continue;
                    if (lex.IsSkobraClose()) break;
                    collection.Error("Unknow lexem", collection.CurrentLexem());
                }
                lex = collection.GotoNext();
            }else
            if (lex.Lexem.ToLower() == "select" || lex.IsSkobraOpen())
            {
                ExpressionToNode2 e = new ExpressionToNode2();
                e.Parse(collection);
                var expr = e.Single();
                var sel = FindSelect(expr);
                if (sel == null) throw new Exception("Values in INSERT not found");
                stmt.Select = sel;
            }
            return stmt;
        }

	    private SelectExpresion FindSelect(Expression exp)
	    {
            if (exp == null) return null;
	        if (exp is SelectExpresion) return exp as SelectExpresion;
            if (exp is SubExpression && exp.Childs != null && exp.Childs.Count == 1) return FindSelect(exp.Childs[0]);
            return null;

	    }
	}
}
