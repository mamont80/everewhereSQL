using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace ParserCore
{
    public class CustomTableGetter
    {
        private ConcurrentDictionary<string, ITableDesc> Cache = new ConcurrentDictionary<string, ITableDesc>();

        public void ClearCache()
        {
            Cache.Clear();
        }

        protected string FullTableName(string[] names)
        {
            string s = "";
            for (int i = 0; i < names.Length; i++)
            {
                if (i > 0) s += ".";
                s += "["+names[i]+"]";
            }
            return s;
        }

        protected ITableDesc Get(string[] names)
        {
            string s = FullTableName(names);
            ITableDesc val;
            if (Cache.TryGetValue(s, out val)) return val;
            return null;
        }

        protected void Set(string[] names, ITableDesc value)
        {
            string s = FullTableName(names);
            Cache.AddOrUpdate(s, value, (s1, desc) => { return value; });
        }
    }
}
