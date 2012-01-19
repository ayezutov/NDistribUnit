using System;
using System.Linq.Expressions;
using NUnit.Framework;

namespace NDistribUnit.Server.Tests
{
    [TestFixture]
    public class CreateSetDelegateFromLambda
    {
        [TestAttribute]
        public void SimpleProperty()
        {
            var eWriteCompiled = CreateSetDelegate((TestClass a) => a.TestProperty);

            var t = new TestClass() {TestProperty = "oldValue"};
            Assert.That(t.TestProperty, Is.EqualTo("oldValue"));

            eWriteCompiled.DynamicInvoke(t, "newValue");

            Assert.That(t.TestProperty, Is.EqualTo("newValue"));
        }

        [TestAttribute]
        public void ComplexProperty()
        {
            var eWriteCompiled = CreateSetDelegate((TestClass a) => a.TestSubProperty.TestProperty2);

            var t = new TestClass() {TestProperty = "oldValue", TestSubProperty = new TestClass(){TestProperty2 = 2}};
            Assert.That(t.TestSubProperty.TestProperty2, Is.EqualTo(2));

            eWriteCompiled.DynamicInvoke(t, 987);

            Assert.That(t.TestSubProperty.TestProperty2, Is.EqualTo(987));
            //Expression.Prop
        }

        private static Delegate CreateSetDelegate<TEntity, TResult>(Expression<Func<TEntity, TResult>> eRead)
        {
            var readBody = ((MemberExpression) eRead.Body);
            ParameterExpression valueParameter = Expression.Parameter(readBody.Type, "value");
            var body = Expression.Assign(readBody, valueParameter);
            var eWrite = Expression.Lambda(body, eRead.Parameters[0], valueParameter);
            var eWriteCompiled = eWrite.Compile();
            return eWriteCompiled;
        }
    }

    public class TestClass
    {
        public string TestProperty { get; set; }
        public int TestProperty2 { get; set; }
        public TestClass TestSubProperty { get; set; }
    }

}