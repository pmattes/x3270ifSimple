// <copyright file="x3270isDisconnectException.cs" company="Paul Mattes">
//     Copyright (c) Paul Mattes. All rights reserved.
// </copyright>

namespace X3270is
{
    /// <summary>
    /// The connection to the emulator was lost.
    /// </summary>
    public class X3270isDisconnectException : X3270isException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="X3270isDisconnectException"/> class.
        /// </summary>
        /// <param name="text">Description of exception.</param>
        public X3270isDisconnectException(string text)
            : base(text)
        {
        }
    }
}
