namespace DocumentAtom.Testing.Xunit
{
    using System.Threading;
    using System.Threading.Tasks;
    using DocumentAtom.Testing.Shared;
    using global::Xunit;
    using Touchstone.Core;

    /// <summary>
    /// Runs the shared Touchstone test descriptors under xUnit, one xUnit test per descriptor.
    /// The descriptors themselves live in Test.Shared; this class only adapts them to the runner.
    /// </summary>
    public sealed class DocumentAtomTheoryTests
    {
        /// <summary>
        /// Provides one theory row per non-skipped test case across every shared suite.
        /// </summary>
        public static TheoryData<TestCaseDescriptor> Cases()
        {
            TheoryData<TestCaseDescriptor> data = new TheoryData<TestCaseDescriptor>();

            foreach (TestSuiteDescriptor suite in DocumentAtomSuites.All)
            {
                foreach (TestCaseDescriptor testCase in suite.Cases)
                {
                    if (!testCase.Skip) data.Add(testCase);
                }
            }

            return data;
        }

        /// <summary>
        /// Executes a single shared test descriptor.
        /// </summary>
        /// <param name="testCase">The descriptor to execute.</param>
        [Theory]
        [MemberData(nameof(Cases))]
        public async Task Run(TestCaseDescriptor testCase)
        {
            await testCase.ExecuteAsync(CancellationToken.None);
        }
    }
}
