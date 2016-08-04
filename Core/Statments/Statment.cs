using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TableQuery
{
	public abstract class Statment
	{
	    public abstract string ToStr();

	    public abstract string ToSql(ExpressionSqlBuilder builder);

	    public abstract void Prepare();

	    public abstract void Optimize();
	}
}
