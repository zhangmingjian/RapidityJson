using Rapidity.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using Xunit;

namespace Rapidity.Json.Test
{
    public class TypeDescriptorProviderTest
    {
        [Fact]
        public void ObjectDescriptorTest()
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
        public void ListDescriptorTest()
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
        public void DictionaryDescriptorTest()
        {
            var desc = TypeDescriptor.Create(typeof(StringDictionary)) as DictionaryDescriptor;
            var dic = desc.CreateInstance();
            desc.SetKeyValue(dic, "aaa", "afefe");
            desc.SetKeyValue(dic, "bbbb", DateTime.Now.ToString());
            var enumer = desc.GetKeys(dic);
            while (enumer.MoveNext())
            {
                var value = desc.GetValue(dic, enumer.Current);
            }
        }


        [Fact]
        public void NameValueCollectionTest()
        {
            var desc = TypeDescriptor.Create(typeof(NameValueCollection)) as DictionaryDescriptor;
            var dic = desc.CreateInstance();
            desc.SetKeyValue(dic, "aaa", "afefe");
            desc.SetKeyValue(dic, "bbbb", DateTime.Now.ToString());
            var enumer = desc.GetKeys(dic);
            while (enumer.MoveNext())
            {
                var value = desc.GetValue(dic, enumer.Current);
            }
        }

        [Fact]
        public void ConcurrentDictionaryTest()
        {
            var desc = TypeDescriptor.Create(typeof(ConcurrentDictionary<string, string>)) as DictionaryDescriptor;
            var dic = desc.CreateInstance();
            desc.SetKeyValue(dic, "aaa", "afefe");
            desc.SetKeyValue(dic, "bbbb", DateTime.Now.ToString());
            var enumer = desc.GetKeys(dic);
            while (enumer.MoveNext())
            {
                var value = desc.GetValue(dic, enumer.Current);
            }
        }

        [Fact]
        public void DictionaryTest()
        {
            var desc = TypeDescriptor.Create(typeof(Dictionary<string, object>)) as DictionaryDescriptor;
            var dic = desc.CreateInstance();
            desc.SetKeyValue(dic, "aaa", "afefe");
            desc.SetKeyValue(dic, "cccc", 1565);
            desc.SetKeyValue(dic, "bbbb", DateTime.Now);
            desc.SetKeyValue(dic, "aaa", true);
            var enumer = desc.GetKeys(dic);
            while (enumer.MoveNext())
            {
                var value = desc.GetValue(dic, enumer.Current);
            }
        }

        [Fact]
        public void ValueTypeTest()
        {
            var properties = Array.CreateInstance(typeof(int), 0);
            var arr = Array.Empty<int>();
            array(arr);
        }

        private void array(int[] arr)
        {
            arr = new int[] { 123,1232 };
        }
    }
}
