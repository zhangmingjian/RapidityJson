using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json.JsonPath
{
    /// <summary>
    /// jsonpath解释器
    /// </summary>
    public interface IJsonPathResolver
    {
        IEnumerable<JsonPathFilter> ResolveFilters(string jsonPath);
    }

    public class DefaultJsonPathResolver : IJsonPathResolver
    {
        public DefaultJsonPathResolver()
        {
        }

        private string _jsonPath;
        private int _cursor = -1;
        private JsonPathFilter _filter;

        public IEnumerable<JsonPathFilter> ResolveFilters(string jsonPath)
        {
            if (string.IsNullOrEmpty(jsonPath)) yield break;
            _jsonPath = jsonPath;
            while (!(_filter is InvalidPathFilter) && Next()) //遇到InvalidPathFilter时直接中止路径解析
            {
                yield return _filter;
            }
        }

        private bool Next()
        {
            while (Move())
            {
                var @char = _jsonPath[_cursor];
                switch (@char)
                {
                    case '$': SetFilter(new RootPathFilter()); return true;
                    case '.': return NextFilter();
                    case '*': SetFilter(new WildcardFilter()); return true;
                    case '[': return ReadQuery(true);
                    default: return ReadQuery();
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool NextFilter()
        {
            if (_filter == null)  //起始位置不能是.
            {
                _filter = new InvalidPathFilter();
                return true;
            }
            if (Move() == false) return false;
            var current = _jsonPath[_cursor];
            switch (current)
            {
                case '.': SetFilter(new RecursiveFilter()); return true;
                case '*': SetFilter(new WildcardFilter()); return true;
                case '$': SetFilter(new RootPathFilter()); return true;
                case '[': return ReadQuery(true);
                default: return ReadQuery();
            }
        }

        //private bool ReadName()
        //{
        //    var start = _cursor;  //从上一个字符开始计数 
        //    while (Move())
        //    {
        //        var current = _jsonPath[_cursor];
        //        if (current == '.' || current == '[')
        //        {
        //            _cursor--;
        //            break;
        //        }
        //    }
        //    var part = _jsonPath.Substring(start, _cursor - start + 1);
        //    //SetFilter(new PathNameFilter(part));
        //    return true;
        //}

        private bool ReadQuery(bool hasBracket = false)
        {
            var start = _cursor + 1;
            var filters = new List<string>();
            while (Move())
            {
                var current = _jsonPath[_cursor];
                switch (current)
                {
                    case '?':
                    case '(': return ReadExpQuery();
                    case ',':
                        var part = _jsonPath.Substring(start, _cursor - start);
                        filters.Add(part);
                        start = _cursor + 1;
                        break;
                    case ']':
                        if (hasBracket)
                        {
                            var part2 = _jsonPath.Substring(start, _cursor - start);
                            filters.Add(part2);
                            SetFilter(new SubNodeFilter(filters, true));
                            return true;
                        }
                        break;
                }
            }
            if(hasBracket)  _filter = new InvalidPathFilter(); //读到这里说明没找到关闭符号],肯定是非法路径格式
            else
            {
                var part2 = _jsonPath.Substring(start, _cursor - start);
                filters.Add(part2);
                SetFilter(new SubNodeFilter(filters));
            }
            return true;
        }

        private bool ReadExpQuery()
        {
            return true;
        }

        private bool Move()
        {
            if (_cursor + 1 < _jsonPath.Length)
            {
                _cursor++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 设置下一个filter并验证合法性
        /// </summary>
        /// <param name="newFilter"></param>
        private void SetFilter(JsonPathFilter newFilter)
        {
            if (_filter == null) //当filter=null时为第一个片段
            {
                if (!(newFilter is RootPathFilter) && !(newFilter is SubNodeFilter) && !(newFilter is WildcardFilter))
                {
                    _filter = new InvalidPathFilter();
                    return;
                }
            }
            if (_filter is RootPathFilter)
            {
                if (!(newFilter is SubNodeFilter) && !(newFilter is WildcardFilter))
                {
                    _filter = new InvalidPathFilter();
                    return;
                }
            }
            if (_filter is RecursiveFilter && newFilter is RecursiveFilter) //不能有2个连续递归filter
            {
                _filter = new InvalidPathFilter();
                return;
            }
            _filter = newFilter;
        }
    }

    //public enum JsonFilterType : byte
    //{
    //    None,
    //    Root,           //$ 表示根节点
    //    Current,        //@ 表示当前
    //    AllChildern,    //*  通配符,取出所有子节点
    //    Recursive,      //.. 递归取出所有子节点及子节点的子节点元素，直到节点是值类型为止
    //    Name,           //.<name>   点，表示子节点
    //    Query,          //[] []内的一系列查询操作
    //    Invalid         //非法jsonpath
    //    //QueryNames,     //['<name>' (, '<name>')] 括号表示子项
    //    //QueryIndexs,    //[<number> (, <number>)] 数组索引或索引
    //    //ArraySlicing,   //[start:end] 数组切片操作
    //    //Expression      //[?(<expression>)] 过滤表达式。 表达式必须求值为一个布尔值。
    //}
}