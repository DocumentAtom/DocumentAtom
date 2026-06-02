namespace DocumentAtom.Server
{
    /// <summary>
    /// DocumentAtom server entry point.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main.
        /// </summary>
        /// <param name="args">Arguments.</param>
        public static void Main(string[] args)
        {
            ServerRuntime.Run(args);
        }
    }
}
