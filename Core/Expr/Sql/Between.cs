using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore.Expr.Simple;

namespace ParserCore.Expr.Sql
{
    
    public class Between: Expression
    {
        public override void Prepare()
        {
            base.Prepare();
            SetResultType(SimpleTypes.Boolean);
        }
        //indexes
        //  0          2     1
        // col between a and b
        public override void ParseInside(ExpressionParser parser)
        {
            //parser.Parse(parser.Collection);
            parser.Collection.GotoNextMust();
            ExpressionParser ep = new ExpressionParser(parser.Collection);
            ep.StopCommandLower.Add("and");
            ep.Parse();
            AddChild(ep.Single());
            base.ParseInside(parser);
        }
        public override int NumChilds()
        {
            return 2;
        }
        public override int Priority()
        {
            return PriorityConst.Between;
        }

        protected override bool CanCalcOnline()
        {
            return false;
        }

        public override string ToStr()
        {
            return Childs[0].ToStr() + " BETWEEN " + Childs[2].ToStr() + " AND " + Childs[1].ToStr();
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            return "(" + Childs[0].ToSql(builder) + " BETWEEN " + Childs[2].ToSql(builder) + " AND " + Childs[1].ToSql(builder) + ")";
        }
    }
     
}
