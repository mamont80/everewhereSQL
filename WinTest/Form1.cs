using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ParserCore;

namespace WinTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                textBox2.Clear();
                LexemCollection coll = null;
                try
                {
                    coll = ParserLexem.Parse(textBox1.Text);
                }
                catch (Exception ex)
                {
                    textBox2.Text = ex.Message;
                    return;
                }
                foreach (var kvp in coll)
                {
                    textBox2.AppendText("|" + kvp.LexemText + "|  " + kvp.LexemType.ToString() + "\r\n");
                }
                coll.NodeFactory = new BaseFactoryComplite();
                ExpressionParser toNode = new ExpressionParser();
                toNode.Parse(coll);
                Expression root = toNode.Single();
                root.Prepare();
                StringBuilder sb = new StringBuilder();

                textBox2.AppendText("разбор\r\n");
                StringBuilder sb2 = new StringBuilder();
                MakeShowExpr(root, 0, sb2);
                textBox2.AppendText(sb2.ToString());
                root.Prepare();
                textBox2.AppendText("разбор после Preapre\r\n");
                textBox2.AppendText(root.ToStr() + "\r\n");
                root = root.PrepareAndOptimize();

                textBox2.AppendText("разбор after optimize\r\n");
                sb2 = new StringBuilder();
                MakeShowExpr(root, 0, sb2);
                textBox2.AppendText(sb2.ToString());
                textBox2.AppendText("\r\n after optimize: " + root.ToStr() + "\r\n");
                
                if (checkBox1.Checked)
                    textBox2.AppendText("Result: " + root.GetStrResultOut(null) + "\r\n");
            }
            catch (Exception ex) { textBox2.AppendText(ex.Message + "\r\n" + ex.StackTrace); }
        }

        private void MakeShowExpr(Expression exp, int numline, StringBuilder sb)
        {
            string space = "";
            for (int i = 0; i < numline; i++) space += "  ";
            space += exp.GetType().Name + exp.ToStr();
            space += "\r\n";
            sb.Append(space);
            if (exp.Childs != null) { foreach (var c in exp.Childs) MakeShowExpr(c, numline + 1, sb); }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
            LexemCollection collection = null;
            try
            {
                collection = ParserLexem.Parse(textBox1.Text);
            }
            catch (Exception ex)
            {
                textBox2.Text = ex.Message;
                return;
            }
            try
            {
                var getter = new SqlServerTableGetter();
                getter.ConnStr = "Data Source=mamont_pk\\sqlexpress; Initial Catalog=LayersDB; Integrated Security=true;Max Pool Size=100000";
                collection.TableGetter = getter;
                collection.NodeFactory = new SqlFactoryComplite();

                StatmentParser sp = new StatmentParser();
                Statment stmt = sp.Parse(collection);
                //FieldCreator fc = new FieldCreator(collection);
                //fc.MakeFields(stmt);
                stmt.Prepare();

                textBox2.Text = stmt.ToStr();
                var builder = new ExpressionSqlBuilder(DriverType.SqlServer);
                textBox3.Text = stmt.ToSql(builder);
            }
            catch (Exception ex)
            { textBox2.AppendText(ex.Message + "\r\n" + ex.StackTrace); }
        }

    }

}
