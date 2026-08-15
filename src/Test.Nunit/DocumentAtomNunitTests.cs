namespace DocumentAtom.Testing.Nunit
{
    using System.Collections;
    using System.Threading;
    using System.Threading.Tasks;
    using DocumentAtom.Testing.Shared;
    using global::NUnit.Framework;
    using Touchstone.Core;
    using Touchstone.NunitAdapter;

    /// <summary>
    /// Runs the shared Touchstone test descriptors under NUnit, one NUnit test per descriptor.
    /// The descriptors themselves live in Test.Shared; this fixture only adapts them to the runner.
    /// </summary>
    [TestFixture]
    public sealed class DocumentAtomNunitTests
    {
        private static IEnumerable Cases()
        {
            return new TouchstoneTestCaseSource(DocumentAtomSuites.All);
        }

        /// <summary>
        /// Executes a single shared test descriptor.
        /// </summary>
        /// <param name="testCase">The descriptor to execute.</param>
        [Test]
        [TestCaseSource(nameof(Cases))]
        public async Task Run(TestCaseDescriptor testCase)
        {
            await testCase.ExecuteAsync(CancellationToken.None);
        }
    }
}
