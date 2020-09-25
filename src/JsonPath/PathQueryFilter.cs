using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json.JsonPath
{
    /// <summary>
    /// 中括号包围的查询条件
    /// ['<name>' (, '<name>')] 括号表示子项
    /// [<number> (, <number>)] 数组索引或索引
    /// [start:end:step]  数组切片操作
    /// [?(<expression>)] 过滤表达式。 表达式必须求值为一个布尔值。</summary>
    /// 
    public class PathQueryFilter : JsonPathFilter
    {
        public string Query; //查询字符串
        public bool InvalidSyntax; //是否有语法错误

        public PathQueryFilter(string query)
        {
            Query = query;
        }

        public override IEnumerable<JsonElement> Filter(JsonElement root, IEnumerable<JsonElement> current)
        {
            if (string.IsNullOrEmpty(Query)) return null;
            return null;
        }

        public static PathQueryFilter Create(string query)
        {
            if (string.IsNullOrEmpty(query)) return new PathQueryFilter(query);
            return null;
            //[name] ['name'] [name1,name2],[*],[..]
        }
    }
}