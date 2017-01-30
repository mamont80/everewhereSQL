﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore.Expr.Simple
{
    /// <summary>
    /// Класс переменной. Эта фиговина пока толком не реализована и не используется. Сделать как понадобится.
    /// </summary>
    public class VariableExpr : Expression
    {
        /// <summary>
        /// Variable name with prefix
        /// </summary>
        public string VariableName;

        private bool _binded = false;
        private object _Value;
        public object Value {
            get { return _Value; }
        }

        public void Bind(object value)
        {
            _Value = value;
            _binded = true;
        }

        /// <summary>
        /// Это операция или значение
        /// </summary>
        public override bool IsOperation() { return false; }

        public override void Prepare()
        {
            base.Prepare();
            if (!_binded) throw new Exception("Variable "+VariableName+" is not binded");
            ConstExpr c = new ConstExpr();
            c.Init(null, SimpleTypes.String);
        }

        protected override bool CanCalcOnline()
        {
            return true;
        }

        public override string ToStr()
        {
            return VariableName;
        }
        
        public override string ToSql(ExpressionSqlBuilder builder)
        {
            if (builder.DbType == DriverType.SqlServer) return VariableName;
            if (builder.DbType == DriverType.PostgreSQL)
            {
                return ":" + VariableName.Substring(1);
            }
            throw new Exception("Do not supperted database");
        }
    }
}
