using Rapidity.Json;
using System;
using System.Diagnostics;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Rapidity.Json.Tests
{
    public class StackTest
    {
        private ITestOutputHelper _output;

        public StackTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void StackPushTest()
        {
            {
                var stack = new Stack<string>();
                var watch = Stopwatch.StartNew();
                for (int i = 0; i < 100000; i++)
                {
                    stack.Push(i.ToString());
                }
                stack.Clear();
                for (int i = 0; i < 100000; i++)
                {
                    stack.Push(i.ToString());
                }
                for (int i = 0; i < 100000; i++)
                {
                    var data = stack.Peek();
                    stack.Pop();
                }
   
                watch.Stop();
                _output.WriteLine($"用时：{watch.ElapsedMilliseconds}ms");
            }

            {
                var stack = new System.Collections.Generic.Stack<string>();
                var watch = Stopwatch.StartNew();
                for (int i = 0; i < 100000; i++)
                {
                    stack.Push(i.ToString());
                }
                stack.Clear();
                for (int i = 0; i < 100000; i++)
                {
                    stack.Push(i.ToString());
                }
                for (int i = 0; i < 100000; i++)
                {
                    var data = stack.Peek();
                    stack.Pop();
                }

                watch.Stop();
                _output.WriteLine($"System.Stack用时：{watch.ElapsedMilliseconds}ms");
            }
        }

        [Fact]
        public void PeekTest()
        {
            var stack = new Stack<string>();
            stack.Push("aaaaa");
            var peek = stack.Peek();
            stack.Push("bbbb");
            var peek2 = stack.Peek();
        }

        [Fact]
        public void StackToArrayTest()
        {
            var stack = new Stack<char>();
            stack.Push('a');
            stack.Push('b');

            var array = stack.ToArray();
            stack.Pop();
            array = stack.ToArray();
            stack.Pop();
            array = stack.ToArray();
        }
    }
}
