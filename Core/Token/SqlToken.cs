using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public delegate IExplore DelegateExpessionExplorer(IExplore exp);

    public interface ISqlConvertible : IExplore
    {
        string ToStr();
        string ToSql(ExpressionSqlBuilder builder);
        void Prepare();
    }

    public interface IExplore
    {
        IExplore Expolore(DelegateExpessionExplorer del);
        SqlToken ParentToken { get; set; }
    }

    public abstract class SqlToken : IExplore, ISqlConvertible
    {
        public SqlToken ParentToken { get; set; }
        public virtual IExplore Expolore(DelegateExpessionExplorer del)
        {
            return del(this);
        }
        public abstract string ToStr();

        public abstract string ToSql(ExpressionSqlBuilder builder);

        public virtual void Prepare()
        {
        }
    }
}
