using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace Rapidity.Json.JsonPath
{
    /// <summary>
    /// jsonpath解析器
    /// </summary>
    public interface IJsonPathResolver
    {
        IEnumerable<JsonPathFilter> Resolve(string jsonPath);
    }

    internal class DefaultJsonPathResolver : IJsonPathResolver
    {
        private string _jsonPath;
        private int _cursor = -1;
        private JsonPathFilter _filter;

        public IEnumerable<JsonPathFilter> Resolve(string jsonPath)
        {
            if (string.IsNullOrEmpty(jsonPath)) yield break;
            _jsonPath = jsonPath;
            while (Next())
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
                    case '$': return ReadRootFilter();
                    case '*': return ReadWildcardFilter();
                    case '.': return NextFilter();
                    case '[': return ReadFilter(true);
                    default: return ReadFilter();
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
                _filter = new InvalidFilter();
                return true;
            }
            Move();
            var current = _jsonPath[_cursor];
            switch (current)
            {
                case '$': return ReadRootFilter();
                case '*': return ReadWildcardFilter();
                case '.': SetFilter(new RecursiveFilter()); return true;
                case '[': //.[组合解析成.. 递归查询
                    SetFilter(new RecursiveFilter());
                    _cursor--; //回退一个字符
                    return true;
                default: return ReadFilter();
            }
        }

        private bool ReadRootFilter()
        {
            if (_cursor != _jsonPath.Length - 1)
            {
                var next = _jsonPath[_cursor + 1];
                if (next != '.' && next != '[') return ReadFilter();
            }
            SetFilter(new RootFilter());
            return true;
        }

        private bool ReadWildcardFilter()
        {
            if (_cursor != _jsonPath.Length - 1)
            {
                var next = _jsonPath[_cursor + 1];
                if (next != '.' && next != '[') return ReadFilter();
            }
            SetFilter(new WildcardFilter());
            return true;
        }

        private bool ReadFilter(bool hasBracket = false)
        {
            var start = hasBracket ? _cursor + 1 : _cursor;
            JsonPathFilter filter = null;
            List<JsonPathFilter> filters = null;
            bool multi = false;
            while (Move())
            {
                var current = _jsonPath[_cursor];
                switch (current)
                {
                    case ']':
                        if (hasBracket)
                        {
                            var part2 = _jsonPath.Substring(start, _cursor - start);
                            filter = ResolveFilter(part2);
                            if (multi) filters.Add(filter);
                            SetFilter(multi ? new MultipleFilters(filters) : filter);
                            return true;
                        }
                        break;
                    case ',':
                        var part = _jsonPath.Substring(start, _cursor - start);
                        if (!multi)
                        {
                            multi = true;
                            filters = new List<JsonPathFilter>();
                        }
                        filters.Add(ResolveFilter(part));
                        start = _cursor + 1;
                        break;
                    case '.':
                        if (!hasBracket) //不在[]内时中断循环
                        {
                            var part3 = _jsonPath.Substring(start, _cursor - start);
                            filter = ResolveFilter(part3);
                            if (multi) filters.Add(filter);
                            SetFilter(multi ? new MultipleFilters(filters) : filter);
                            return true;
                        }
                        break;
                    case '?': //按照expressfilter来预期读取，仅移动位置
                    case '(': ConsumeExpFilter(current); break;
                }
            }
            if (hasBracket) _filter = new InvalidFilter(); //读到这里说明没找到闭括号],一定是非法格式
            else
            {
                var part2 = _jsonPath.Substring(start, _cursor - start + 1);
                filter = ResolveFilter(part2);
                if (multi) filters.Add(filter);
                SetFilter(multi ? new MultipleFilters(filters) : filter);
                return true;
            }
            return true;
        }

        private JsonPathFilter ResolveFilter(string name)
        {
            if (name == "$") return new RootFilter();
            else if (name == "*") return new WildcardFilter();
            else if (name == "..") return new RecursiveFilter();
            else if (int.TryParse(name, out int index))  return new ArrayIndexFilter(index);//按索引查找
            else if ((name.StartsWith("?(") || name.StartsWith("(")) && name.EndsWith(")"))
            {
                return new ExpressionFilter(name);
            }
            else if (name.Contains(':')) //数组切片
            {
                var slices = name.Split(':');
                if (slices.Length > 3) return new InvalidFilter();
                int? start = 0;
                int? end = int.MaxValue;
                int? step = 1;
                if (slices.Length > 0)
                {
                    var startVal = slices[0];
                    if (startVal.Length > 0) start = startVal.TryToInt();
                }
                if (slices.Length > 1)
                {
                    var endVal = slices[1];
                    if (endVal.Length > 0) end = endVal.TryToInt();
                }
                if (slices.Length > 2)
                {
                    var stepVal = slices[2];
                    if (stepVal.Length > 0) step = stepVal.TryToInt();
                }
                if (!start.HasValue || !end.HasValue || !step.HasValue)
                    return new InvalidFilter(); //有一个没有值则为输入格式非法，跳过
                return new ArraySliceFilter(start.Value, end.Value, step.Value);
            }
            else return new PropertyFilter(name);
        }

        /// <summary>
        /// 按照expressfilter来预期读取，仅移动位置，不处理扫描结果
        /// </summary>
        /// <param name="current"></param>
        private void ConsumeExpFilter(char current)
        {
            if (current == '?' && _cursor != _jsonPath.Length - 1)
            {
                var next = _jsonPath[_cursor + 1];
                if (next != '(')
                {
                    ReadFilter();
                }
                else Move();
            }
            int deep = 1;
            while (Move())
            {
                current = _jsonPath[_cursor];
                switch (current)
                {
                    case '(': deep++; break;
                    case ')':
                        deep--;
                        if (deep == 0) return;
                        break;
                }
            }
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
                if (!(newFilter is RootFilter) && !(newFilter is MultipleFilters) && !(newFilter is WildcardFilter)
                    && !(newFilter is PropertyFilter))
                {
                    _filter = new InvalidFilter();
                    return;
                }
            }
            //if (_filter is RootFilter)
            //{
            //    if (!(newFilter is RecursiveFilter) && !(newFilter is WildcardFilter))
            //    {
            //        _filter = new InvalidPathFilter();
            //        return;
            //    }
            //}
            if (_filter is RecursiveFilter && newFilter is RecursiveFilter) //不能有2个连续递归filter
            {
                _filter = new InvalidFilter();
                return;
            }
            _filter = newFilter;
        }
    }
}