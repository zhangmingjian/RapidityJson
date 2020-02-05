using Rapidity.Json.Test;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Xunit;

namespace Rapidity.Json.Tests
{
    public class ExpressionTest
    {
        [Fact]
        public void PropertySetValueTest()
        {
            var bindList = new List<MemberBinding>();
            var init = Expression.New(typeof(Person));
            foreach (var property in typeof(Person).GetProperties())
            {
                ConstantExpression constant = null;
                if (property.PropertyType == typeof(int))
                    constant = Expression.Constant(10, typeof(int));
                if (property.PropertyType == typeof(string))
                    constant = Expression.Constant("fawefagg ", typeof(string));
                MemberAssignment bindingId = Expression.Bind(property, constant);
                bindList.Add(bindingId);

            }
            //MemberInitExpression initExp = Expression.MemberInit(init, bindList);
            Expression<Func<Person>> expression = Expression.Lambda<Func<Person>>(init);
            var func = expression.Compile();
            var p = func();
        }     
    }
}
