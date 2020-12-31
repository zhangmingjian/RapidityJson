using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Rapidity.Json.JsonPath
{
    internal class JsonPathResolver : IJsonPathResolver
    {
        private CharScanner _scanner;
        private JsonPathFilter _filter;

        public IEnumerable<JsonPathFilter> Resolve(string jsonPath)
        {
            if (string.IsNullOrEmpty(jsonPath)) yield break;
            _scanner = new CharScanner(jsonPath);

            while (_scanner.Next())
            {
                if (IsSpace(_scanner.Value)) continue; //跳过空字符
                switch (_scanner.Value)
                {
                    case '$': ReadRootFilter(); break;
                    case '*': ReadWildcardFilter(); break;
                    //case '.': return NextFilter();
                    //case '[': return ReadFilter(true);
                    default: ReadFilter(); break;
                }
                yield return _filter;
            }
        }

        private bool IsSpace(char? @char)
        {
            switch (@char)
            {
                case ' ':
                case '\t':
                case '\r':
                case '\n': return true;
                default: return false;
            }
        }

        /// <summary>
        /// 根节点过滤器
        /// </summary>
        /// <returns></returns>
        private bool ReadRootFilter()
        {
            var next = _scanner.Peek();
            //$后面跟.或[，则解析为RootFilter，否则继续读取
            if (next == '.' || next == '[') return SetFilter(new RootFilter());
            return false;
        }

        /// <summary>
        /// 通配符过滤器
        /// </summary>
        /// <returns></returns>
        private bool ReadWildcardFilter()
        {
            var next = _scanner.Peek();
            if (next == '.' || next == '[') return SetFilter(new WildcardFilter());
            return true;
        }

        private bool ReadFilter()
        {

            return true;
        }

        /// <summary>
        /// 设置下一个filter并验证合法性
        /// </summary>
        /// <param name="newFilter"></param>
        private bool SetFilter(JsonPathFilter newFilter)
        {
            if (_filter == null) //当filter=null时为第一个片段
            {
                if (!(newFilter is RootFilter)
                    && !(newFilter is MultipleFilters)
                    && !(newFilter is WildcardFilter)
                    && !(newFilter is PropertyFilter))
                {
                    _filter = new InvalidFilter();
                    return true;
                }
            }
            if (_filter is RecursiveFilter
                && newFilter is RecursiveFilter) //不能有2个连续递归filter
            {
                _filter = new InvalidFilter();
                return true;
            }
            _filter = newFilter;
            return true;
        }
    }
}