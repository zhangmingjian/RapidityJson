using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rapidity.Json.JsonPath
{
    public abstract class JsonPathFilter
    {
        public abstract IEnumerable<JsonElement> Filter(JsonElement root, IEnumerable<JsonElement> current);
    }

    /// <summary>
    /// $ 开头的path
    /// </summary>
    public class RootFilter : JsonPathFilter
    {
        public override IEnumerable<JsonElement> Filter(JsonElement root, IEnumerable<JsonElement> current)
        {
            return new List<JsonElement> { root };
        }
    }

    /// <summary>
    /// 无效jsonpath
    /// </summary>
    public class InvalidFilter : JsonPathFilter
    {

        public override IEnumerable<JsonElement> Filter(JsonElement root, IEnumerable<JsonElement> current)
        {
            return null;
        }
    }

    /// <summary>
    /// * 通配符。所有对象/元素
    /// </summary>
    public class WildcardFilter : JsonPathFilter
    {
        public override IEnumerable<JsonElement> Filter(JsonElement root, IEnumerable<JsonElement> current)
        {
            foreach (var element in current)
            {
                switch (element.ElementType)
                {
                    case JsonElementType.Object:
                        var obj = (JsonObject)element;
                        foreach (var name in obj.GetPropertyNames())
                        {
                            yield return obj[name];
                        }
                        break;
                    case JsonElementType.Array:
                        foreach (var item in (JsonArray)element)
                        {
                            yield return item;
                        }
                        break;
                    default: yield return element; break;
                }
            }
        }
    }

    /// <summary>
    /// .. 递归过滤器
    /// </summary>
    public class RecursiveFilter : JsonPathFilter
    {
        public override IEnumerable<JsonElement> Filter(JsonElement root, IEnumerable<JsonElement> current)
        {
            var list = new List<JsonElement>();
            foreach (var element in current)
            {
                RecursiveCollect(element, list, true);
            }
            return list;
        }

        /// <summary>
        /// 递归取出容器节点
        /// </summary>
        /// <param name="element"></param>
        /// <param name="target"></param>
        private void RecursiveCollect(JsonElement element, List<JsonElement> target, bool root = false)
        {
            if (element.ElementType == JsonElementType.Object)
            {
                target.Add(element);
                foreach (var propery in (JsonObject)element)
                {
                    RecursiveCollect(propery.Value, target);
                }
            }
            else if (element.ElementType == JsonElementType.Array)
            {
                target.Add(element);
                foreach (var item in (JsonArray)element)
                {
                    RecursiveCollect(item, target);
                }
            }
            else if (root) target.Add(element);
        }
    }

    /// <summary>
    /// 按属性名称过滤器
    /// </summary>
    public class PropertyNameFilter : JsonPathFilter
    {
        private readonly string _property;
        public PropertyNameFilter(string property)
        {
            _property = property;
        }

        public override IEnumerable<JsonElement> Filter(JsonElement root, IEnumerable<JsonElement> current)
        {
            var property = _property.Trim('\'');
            var list = new List<JsonElement>();
            foreach (var element in current)
            {
                if (element is JsonObject jObj)
                {
                    if (jObj.TryGetValue(property, out JsonElement value))
                        list.Add(value);
                }
            }
            return list;
        }
    }

    /// <summary>
    /// 数组索引过滤器
    /// </summary>
    public class ArrayIndexFilter : JsonPathFilter
    {
        private int _index;

        public ArrayIndexFilter(int index)
        {
            this._index = index;
        }

        public override IEnumerable<JsonElement> Filter(JsonElement root, IEnumerable<JsonElement> current)
        {
            var list = new List<JsonElement>();
            foreach (var element in current)
            {
                if (element is JsonArray jArr)
                {
                    if (_index >= 0 && _index < jArr.Count)
                        list.Add(jArr[_index]);
                }
            }
            return list;
        }
    }

    /// <summary>
    /// 数组切片过滤器
    /// </summary>
    public class ArraySliceFilter : JsonPathFilter
    {
        private int _start;
        private int _end;
        private int _step;

        public ArraySliceFilter(int start, int end, int step)
        {
            this._start = start;
            this._end = end;
            this._step = step;
        }

        public override IEnumerable<JsonElement> Filter(JsonElement root, IEnumerable<JsonElement> current)
        {
            var list = new List<JsonElement>();
            foreach (var element in current)
            {
                if (element is JsonArray jArr)
                {
                    var arr = jArr.Slice(_start, _end, _step);
                    if (arr.Count > 0) list.AddRange(arr);
                }
            }
            return list;
        }
    }

    /// <summary>
    /// 组合过滤器
    /// </summary>
    public class MultipleFilters : JsonPathFilter
    {
        private IEnumerable<JsonPathFilter> _filters;
        public MultipleFilters(IEnumerable<JsonPathFilter> filters)
        {
            _filters = filters;
        }
        public override IEnumerable<JsonElement> Filter(JsonElement root, IEnumerable<JsonElement> current)
        {
            var list = new List<JsonElement>();
            foreach (var filter in _filters)
            {
                var elements = filter.Filter(root, current);
                if (elements != null && elements.Count() > 0)
                    list.AddRange(elements);
            }
            return list;
        }
    }
}