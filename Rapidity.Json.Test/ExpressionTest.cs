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
            //ParameterExpression parameter = Expression.Parameter(typeof(Person), "p");
            //MemberExpression property = Expression.Property(parameter, "Id");

            //ConstantExpression constant = Expression.Constant(10, typeof(int));
            //MemberAssignment bindingId = Expression.Bind(property.Member, constant);

            //MemberInitExpression initExp = Expression.MemberInit(Expression.New(typeof(Person)), bindingId);

            //Expression<Func<Person>> expression = Expression.Lambda<Func<Person>>(initExp, Expression.Parameter(typeof(Person), "p"));
            //var func = expression.Compile();       
            //var p = func();

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
            MemberInitExpression initExp = Expression.MemberInit(init, bindList);
            Expression<Func<Person>> expression = Expression.Lambda<Func<Person>>(initExp);
            var func = expression.Compile();
            var p = func();
        }

        public class Person
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
