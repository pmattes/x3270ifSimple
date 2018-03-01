// <copyright file="x3270ifDisconnectException.cs" company="Paul Mattes">
//     Copyright (c) Paul Mattes. All rights reserved.
// </copyright>

namespace X3270if
{
    /// <summary>
    /// The connection to the emulator was lost.
    /// </summary>
    public class X3270ifDisconnectException : X3270ifException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="X3270ifDisconnectException"/> class.
        /// </summary>
        /// <param name="text">Description of exception</param>
        public X3270ifDisconnectException(string text) : base(text)
        {
        }
    }
}
