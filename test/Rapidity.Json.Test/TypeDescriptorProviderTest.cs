using Rapidity.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Xunit;

namespace Rapidity.Json.Test
{
    public class TypeDescriptorProviderTest
    {
        [Fact]
        public void ObjectProviderTest()
        {
            //var provider = new TypeDescriptorProvider();
            var desc = TypeDescriptor.Create(typeof(Person)) as ObjectDescriptor;
            var person = desc.CreateInstance();
            desc.TrySetValue(person, "Name", "张三");
            desc.TrySetValue(person, nameof(Person.Number), Guid.NewGuid());
            desc.TrySetValue(person, nameof(Person.Birthday), DateTime.Now);
            desc.TrySetValue(person, nameof(Person.Child), new Person());

            var name = desc.GetValue(person, "Name");
        }

        [Fact]
        public void GetFieldTest()
        {
            var desc = TypeDescriptor.Create(typeof(Person)) as ObjectDescriptor;
            var person = desc.CreateInstance();
            var floatField = desc.GetValue(person, "floadField");
            Assert.Equal(0f, floatField);

            desc.TrySetValue(person, "floadField", 12345f);

            floatField = desc.GetValue(person, "floadField");
            Assert.Equal(12345f, floatField);
        }

        [Fact]
        public void ListProviderTest()
        {
            var desc = TypeDescriptor.Create(typeof(List<Person>)) as EnumerableDescriptor;
            var list = desc.CreateInstance() as List<Person>;
            desc.AddItem(list, new Person() { Id = 100, Name = "faefafeafeaef" });
            Assert.Equal(1, list.Count);
        }

        [Fact]
        public void CollectionTest()
        {
            var desc = TypeDescriptor.Create(typeof(Collection<Person>)) as EnumerableDescriptor;
            var list = desc.CreateInstance() as Collection<Person>;
            desc.AddItem(list, new Person() { Id = 100, Name = "faefafeafeaef" });
            Assert.Equal(1, list.Count);

            var enumer = desc.GetEnumerator(list);
            while (enumer.MoveNext())
            {
                var item = enumer.Current;
            }
        }

        [Fact]
        public void ValueTypeTest()
        {
            var flag = float.TryParse("-∞", out float va);
            var f = float.Parse("-∞", CultureInfo.InvariantCulture);
        }
    }
}
