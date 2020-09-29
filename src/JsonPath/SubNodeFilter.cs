using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rapidity.Json.JsonPath
{
    /// <summary>
    /// []中括号包围的查询条件
    /// ['<name>' (, '<name>')] 括号表示子项
    /// [<number> (, <number>)] 数组索引或索引
    /// [start:end:step] 数组切片操作
    /// </summary>
    public class SubNodeFilter : JsonPathFilter
    {
        public bool HasBracket { get; set; }  //是否有中括号包围

        private IEnumerable<string> _names;

        public SubNodeFilter(IEnumerable<string> names, bool _hasBracket = false)
        {
            _names = names;
            HasBracket = _hasBracket;
        }

        public override IEnumerable<JsonElement> Filter(JsonElement root, IEnumerable<JsonElement> current)
        {
            if (_names == null || _names.Count() == 0) return null;
            var list = new List<JsonElement>();
            foreach (var filter in _names)
            {
                if (string.IsNullOrEmpty(filter)) continue;
                var name = filter.Trim('\''); //去掉外层的单引号
                if (name == "*")
                {
                    list.AddRange(new WildcardFilter().Filter(root, current));
                }
                else if (name == "..")
                {
                    list.AddRange(new RecursiveFilter().Filter(root, current));
                }
                else if (int.TryParse(name, out int index)) //按索引查找
                {
                    list.AddRange(FilterByIndex(current, index));
                }
                else if (name.Contains(':')) //数组切片
                {
                    var slices = filter.Split(':');
                    if (slices.Length > 3) continue; //超出3位不合法跳过
                    int? start = 0;
                    int? end = int.MaxValue;
                    int? step = 1;
                    if (slices.Length > 0)
                    {
                        var startVal = slices[0];
                        if (startVal.Length >= 0) start = startVal.TryToInt();
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
                    if (!start.HasValue || !end.HasValue || !step.HasValue) continue; //有一个没有值则为输入格式非法，跳过
                    list.AddRange(FilterBySlice(current, start.Value, end.Value, step.Value));
                }
                else  //属性名称查找
                {
                    list.AddRange(FilterByName(current, name));
                }
            }
            return list;
        }

        #region filterByName
        /// <summary>
        /// 名称匹配
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private IEnumerable<JsonElement> FilterByName(IEnumerable<JsonElement> elements, string name)
        {
            var list = new List<JsonElement>();
            foreach (var element in elements)
            {
                FilterByName(element, name, list);
            }
            return list;
        }

        private void FilterByName(JsonElement element, string name, List<JsonElement> list)
        {
            if (element.ElementType == JsonElementType.Object)
            {
                var jObj = (JsonObject)element;
                if (jObj.ContainsProperty(name)) list.Add(jObj[name]);
                foreach (var key in jObj.GetPropertyNames())
                {
                    FilterByName(jObj[key], name, list);
                }
            }
            else if (element.ElementType == JsonElementType.Array)
            {
                foreach (var ele in (JsonArray)element)
                    FilterByName(ele, name, list);
            }
        }
        #endregion

        #region filterByIndex
        /// <summary>
        /// 索引匹配
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private IEnumerable<JsonElement> FilterByIndex(IEnumerable<JsonElement> elements, int index)
        {
            var list = new List<JsonElement>();
            foreach (var element in elements)
            {
                FilterByIndex(element, index, list);
            }
            return list;
        }

        private void FilterByIndex(JsonElement element, int index, List<JsonElement> list)
        {
            if (element.ElementType == JsonElementType.Array)
            {
                var jArr = (JsonArray)element;
                if (index >= 0 && index < jArr.Count) list.Add(jArr[index]);
                foreach (var ele in jArr)
                {
                    FilterByIndex(ele, index, list);
                }
            }
            else if (element.ElementType == JsonElementType.Object)
            {
                foreach (var ele in ((JsonObject)element))
                {
                    FilterByIndex(ele.Value, index, list);
                }
            }
        }
        #endregion

        #region filterBySlice

        /// <summary>
        /// 数组切片
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        private IEnumerable<JsonElement> FilterBySlice(IEnumerable<JsonElement> elements, int start, int end, int step)
        {
            var list = new List<JsonElement>();
            foreach (var element in elements)
            {
                FilterBySlice(element, start, end, step, list);
            }
            return list;
        }

        private void FilterBySlice(JsonElement element, int start, int end, int step, List<JsonElement> list)
        {
            if (element.ElementType == JsonElementType.Array)
            {
                var jArr = (JsonArray)element;
                var arr = jArr.Slice(start, end, step);
                if (arr.Count > 0) list.AddRange(arr);
                foreach (var ele in jArr)
                {
                    FilterBySlice(ele, start, end, step, list);
                }
            }
            else if (element.ElementType == JsonElementType.Object)
            {
                foreach (var ele in ((JsonObject)element))
                {
                    FilterBySlice(ele.Value, start, end, step, list);
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// 表达式查询  [?(<expression>)] 过滤表达式。 表达式必须求值为一个布尔值。</summary>
    /// </summary>
    public class ExpressionQueryFilter : JsonPathFilter
    {
        private string _expression;

        public ExpressionQueryFilter(string exp)
        {
            _expression = exp;
        }

        public override IEnumerable<JsonElement> Filter(JsonElement root, IEnumerable<JsonElement> current)
        {
            throw new NotImplementedException();
        }
    }
}