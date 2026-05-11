using System;
using System.Linq;
using System.Reflection;

namespace OnTopReplica.Tests {

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TestAttribute : Attribute { }

    public static class Assert {
        public static void AreEqual<T>(T expected, T actual, string message = null) {
            if (!object.Equals(expected, actual))
                throw new TestFailedException(string.Format("Expected <{0}>, got <{1}>. {2}", expected, actual, message));
        }
        public static void AreClose(double expected, double actual, double tolerance, string message = null) {
            if (Math.Abs(expected - actual) > tolerance)
                throw new TestFailedException(string.Format("Expected ~{0} (±{1}), got {2}. {3}", expected, tolerance, actual, message));
        }
        public static void IsTrue(bool condition, string message = null) {
            if (!condition) throw new TestFailedException("Expected true. " + message);
        }
        public static void Throws<TException>(Action action) where TException : Exception {
            try { action(); }
            catch (TException) { return; }
            catch (Exception ex) {
                throw new TestFailedException("Expected " + typeof(TException).Name + " but got " + ex.GetType().Name);
            }
            throw new TestFailedException("Expected " + typeof(TException).Name + " but no exception was thrown");
        }
    }

    public class TestFailedException : Exception {
        public TestFailedException(string message) : base(message) { }
    }

    public static class TestRunner {
        public static int Main(string[] args) {
            int passed = 0, failed = 0;
            var asm = Assembly.GetExecutingAssembly();
            var testClasses = asm.GetTypes()
                .Where(t => t.GetMethods().Any(m => m.GetCustomAttribute<TestAttribute>() != null))
                .OrderBy(t => t.FullName);

            foreach (var t in testClasses) {
                Console.WriteLine("== " + t.Name);
                var instance = Activator.CreateInstance(t);
                foreach (var m in t.GetMethods().Where(x => x.GetCustomAttribute<TestAttribute>() != null).OrderBy(x => x.Name)) {
                    try {
                        m.Invoke(instance, null);
                        Console.WriteLine("  [PASS] " + m.Name);
                        passed++;
                    }
                    catch (Exception ex) {
                        var inner = ex is TargetInvocationException ? ex.InnerException : ex;
                        Console.WriteLine("  [FAIL] " + m.Name + " -- " + inner.Message);
                        failed++;
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine(string.Format("Result: {0} passed, {1} failed", passed, failed));
            return failed == 0 ? 0 : 1;
        }
    }
}
