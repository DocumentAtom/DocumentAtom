namespace DocumentAtom.Testing.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Touchstone.Core;

    /// <summary>
    /// Small fluent helper for assembling a <see cref="TestSuiteDescriptor"/> from synchronous or
    /// asynchronous test bodies without repeating the descriptor boilerplate.
    /// </summary>
    internal sealed class SuiteBuilder
    {
        private readonly string _SuiteId;
        private readonly List<TestCaseDescriptor> _Cases = new List<TestCaseDescriptor>();

        internal SuiteBuilder(string suiteId)
        {
            _SuiteId = suiteId;
        }

        /// <summary>
        /// Adds a synchronous test case. The body signals failure by throwing.
        /// </summary>
        internal SuiteBuilder Case(string caseId, string displayName, Action body)
        {
            _Cases.Add(new TestCaseDescriptor(
                _SuiteId,
                caseId,
                displayName,
                ct =>
                {
                    body();
                    return Task.CompletedTask;
                }));
            return this;
        }

        /// <summary>
        /// Adds an asynchronous test case. The body signals failure by throwing.
        /// </summary>
        internal SuiteBuilder CaseAsync(string caseId, string displayName, Func<CancellationToken, Task> body)
        {
            _Cases.Add(new TestCaseDescriptor(_SuiteId, caseId, displayName, body));
            return this;
        }

        /// <summary>
        /// Builds the suite descriptor with the supplied human-readable display name.
        /// </summary>
        internal TestSuiteDescriptor Build(string displayName)
        {
            return new TestSuiteDescriptor(_SuiteId, displayName, _Cases);
        }
    }
}
