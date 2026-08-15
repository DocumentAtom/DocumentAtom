namespace DocumentAtom.Testing.Shared
{
    using System;

    /// <summary>
    /// Exception thrown when a <see cref="Check"/> assertion fails inside a Touchstone test descriptor.
    /// </summary>
    public class TouchstoneAssertException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TouchstoneAssertException"/> class.
        /// </summary>
        /// <param name="message">The assertion failure message.</param>
        public TouchstoneAssertException(string message)
            : base(message)
        {
        }
    }
}
