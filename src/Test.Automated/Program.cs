namespace DocumentAtom.Testing.Automated
{
    using System;
    using System.Threading.Tasks;
    using DocumentAtom.Testing.Shared;
    using Touchstone.Cli;

    /// <summary>
    /// Touchstone CLI runner for the DocumentAtom test suite. Executes every descriptor defined in
    /// <see cref="DocumentAtomSuites.All"/> and returns a CI-friendly exit code (0 = all passed,
    /// 1 = at least one failure).
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">
        /// Optional single argument: a path to which JSON results are written (for example "results.json").
        /// </param>
        /// <returns>Process exit code: 0 when all tests pass, otherwise 1.</returns>
        public static async Task<int> Main(string[] args)
        {
            string? resultsPath = args.Length >= 1 && !string.IsNullOrWhiteSpace(args[0]) ? args[0] : null;

            try
            {
                return await ConsoleRunner.RunAsync(DocumentAtomSuites.All, resultsPath: resultsPath).ConfigureAwait(false);
            }
            finally
            {
                Workspace.Cleanup();
            }
        }
    }
}
