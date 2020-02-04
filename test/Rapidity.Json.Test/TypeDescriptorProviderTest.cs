using Rapidity.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var desc = provider.GetDescriptor(typeof(Person)) as ObjectDescriptor;
            var person = desc.CreateMethod();
            var nameProperty = desc.PropertyDescriptors.FirstOrDefault(x => x.JsonAlias == "Name");
            nameProperty.SetValueMethod(person, Guid.NewGuid().ToString());

            var idProp = desc.PropertyDescriptors.FirstOrDefault(x => x.JsonAlias == "Id");
            idProp.SetValueMethod(person, 1212);

        }

        [Fact]
        public void ListProviderTest()
        {
            var provider = new TypeDescriptorProvider();
            var desc = provider.GetDescriptor(typeof(List<Person>)) as ListDescriptor;
            var person = desc.CreateMethod() as List<Person>;
            //desc.AddMethod(person, new Person("aeff") { Id = 100, Name = "faefafeafeaef" });
        }
    }
}
