using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserCore.Expr.Sql;

namespace ParserCore
{
    public class SetClause : SqlToken
    {
        private Expression _Column;
        public Expression Column
        {
            get { return _Column; }
            set
            {
                _Column = value;
                if (value != null) value.ParentToken = this;
            }
        }
        private Expression _Value;
        public Expression Value
        {
            get { return _Value; }
            set
            {
                _Value = value;
                if (value != null) value.ParentToken = this;
            }
        }

        public override IExplore Expolore(DelegateExpessionExplorer del)
        {
            if (Column != null) Column = (Expression)Column.Expolore(del);
            if (Value != null) Value = (Expression)Value.Expolore(del);
            return base.Expolore(del);
        }
        public override void Prepare()
        {
            if (Column != null) Column.Prepare();
            if (Value != null) Value.Prepare();
        }

        public override string ToStr()
        {
            return Column.ToStr() +" = "+Value.ToStr();
        }

        public override string ToSql(ExpressionSqlBuilder builder)
        {
            if (!(Column is FieldExpr)) throw new Exception("В \"update set xxx = \" должны быть простые имена колонок");

            FieldExpr fe = Column as FieldExpr;

            return fe.ToSqlShort(builder) + " = " + Value.ToSql(builder);
        }
    }
}
