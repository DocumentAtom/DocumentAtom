namespace DocumentAtom.Testing.Shared
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Runner-agnostic assertion helpers used by Touchstone test descriptors.
    /// A failed assertion throws <see cref="TouchstoneAssertException"/>, which every
    /// host (CLI, xUnit, NUnit) surfaces as a test failure. This keeps Test.Shared free
    /// of any test-framework dependency.
    /// </summary>
    public static class Check
    {
        /// <summary>
        /// Asserts that the supplied condition is true.
        /// </summary>
        public static void True(bool condition, string? message = null)
        {
            if (!condition) throw new TouchstoneAssertException(message ?? "Expected condition to be true.");
        }

        /// <summary>
        /// Asserts that the supplied condition is false.
        /// </summary>
        public static void False(bool condition, string? message = null)
        {
            if (condition) throw new TouchstoneAssertException(message ?? "Expected condition to be false.");
        }

        /// <summary>
        /// Asserts that two values are equal using the default equality comparer.
        /// </summary>
        public static void Equal<T>(T expected, T actual, string? message = null)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                throw new TouchstoneAssertException(message ?? $"Expected [{Fmt(expected)}] but got [{Fmt(actual)}].");
        }

        /// <summary>
        /// Asserts that two values are not equal using the default equality comparer.
        /// </summary>
        public static void NotEqual<T>(T notExpected, T actual, string? message = null)
        {
            if (EqualityComparer<T>.Default.Equals(notExpected, actual))
                throw new TouchstoneAssertException(message ?? $"Expected value to differ from [{Fmt(actual)}].");
        }

        /// <summary>
        /// Asserts that the supplied object reference is null.
        /// </summary>
        public static void Null(object? value, string? message = null)
        {
            if (value != null) throw new TouchstoneAssertException(message ?? $"Expected null but got [{Fmt(value)}].");
        }

        /// <summary>
        /// Asserts that the supplied object reference is not null.
        /// </summary>
        public static void NotNull(object? value, string? message = null)
        {
            if (value == null) throw new TouchstoneAssertException(message ?? "Expected non-null value.");
        }

        /// <summary>
        /// Asserts that the collection contains no elements.
        /// </summary>
        public static void Empty(IEnumerable collection, string? message = null)
        {
            NotNull(collection, message);
            foreach (object? _ in collection)
                throw new TouchstoneAssertException(message ?? "Expected empty collection.");
        }

        /// <summary>
        /// Asserts that the collection contains at least one element.
        /// </summary>
        public static void NotEmpty(IEnumerable collection, string? message = null)
        {
            NotNull(collection, message);
            foreach (object? _ in collection) return;
            throw new TouchstoneAssertException(message ?? "Expected non-empty collection.");
        }

        /// <summary>
        /// Asserts that the collection has exactly the specified number of elements.
        /// </summary>
        public static void Count<T>(int expected, IEnumerable<T> collection, string? message = null)
        {
            NotNull(collection, message);
            int actual = collection.Count();
            if (actual != expected)
                throw new TouchstoneAssertException(message ?? $"Expected {expected} element(s) but got {actual}.");
        }

        /// <summary>
        /// Asserts that the collection has exactly one element and returns it.
        /// </summary>
        public static T Single<T>(IEnumerable<T> collection, string? message = null)
        {
            NotNull(collection, message);
            List<T> list = collection.ToList();
            if (list.Count != 1)
                throw new TouchstoneAssertException(message ?? $"Expected exactly 1 element but got {list.Count}.");
            return list[0];
        }

        /// <summary>
        /// Asserts that the actual string contains the expected substring.
        /// </summary>
        public static void Contains(string expectedSubstring, string? actual, string? message = null)
        {
            NotNull(actual, message);
            if (!actual!.Contains(expectedSubstring, StringComparison.Ordinal))
                throw new TouchstoneAssertException(message ?? $"Expected string to contain [{expectedSubstring}] but was [{actual}].");
        }

        /// <summary>
        /// Asserts that the actual string does not contain the specified substring.
        /// </summary>
        public static void DoesNotContain(string unexpectedSubstring, string? actual, string? message = null)
        {
            NotNull(actual, message);
            if (actual!.Contains(unexpectedSubstring, StringComparison.Ordinal))
                throw new TouchstoneAssertException(message ?? $"Expected string not to contain [{unexpectedSubstring}] but was [{actual}].");
        }

        /// <summary>
        /// Asserts that the actual string starts with the expected prefix.
        /// </summary>
        public static void StartsWith(string expectedPrefix, string? actual, string? message = null)
        {
            NotNull(actual, message);
            if (!actual!.StartsWith(expectedPrefix, StringComparison.Ordinal))
                throw new TouchstoneAssertException(message ?? $"Expected string to start with [{expectedPrefix}] but was [{actual}].");
        }

        /// <summary>
        /// Asserts that invoking the action throws an exception of type <typeparamref name="TException"/>
        /// (or a subclass) and returns the thrown exception.
        /// </summary>
        public static TException Throws<TException>(Action action, string? message = null)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException ex)
            {
                return ex;
            }
            catch (Exception ex)
            {
                throw new TouchstoneAssertException(
                    message ?? $"Expected {typeof(TException).Name} but got {ex.GetType().Name}: {ex.Message}");
            }

            throw new TouchstoneAssertException(message ?? $"Expected {typeof(TException).Name} but no exception was thrown.");
        }

        /// <summary>
        /// Asserts that awaiting the async function throws an exception of type
        /// <typeparamref name="TException"/> (or a subclass) and returns the thrown exception.
        /// </summary>
        public static async Task<TException> ThrowsAsync<TException>(Func<Task> action, string? message = null)
            where TException : Exception
        {
            try
            {
                await action().ConfigureAwait(false);
            }
            catch (TException ex)
            {
                return ex;
            }
            catch (Exception ex)
            {
                throw new TouchstoneAssertException(
                    message ?? $"Expected {typeof(TException).Name} but got {ex.GetType().Name}: {ex.Message}");
            }

            throw new TouchstoneAssertException(message ?? $"Expected {typeof(TException).Name} but no exception was thrown.");
        }

        private static string Fmt(object? value)
        {
            if (value == null) return "null";
            return value.ToString() ?? "null";
        }
    }
}
