using Rapidity.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static Rapidity.Json.Tests.ExpressionTest;

namespace Rapidity.Json.Test
{
    public class TypeDescriptorProviderTest
    {
        [Fact]
        public void ObjectProviderTest()
        {
            var provider = new TypeDescriptorProvider();
            var desc = provider.GetDescriptor(typeof(Person));
            var person = desc.Create();
        }

        [Fact]
        public void ListProviderTest()
        {
            var provider = new TypeDescriptorProvider();
            var desc = provider.GetDescriptor(typeof(List<Person>)) as ListDesciptor;
            var person = desc.Create();
            desc.Add(person,new Person("aeff"));
        }
    }
}
