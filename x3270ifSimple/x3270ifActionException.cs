// <copyright file="x3270ifActionException.cs" company="Paul Mattes">
//     Copyright (c) Paul Mattes. All rights reserved.
// </copyright>

namespace X3270if
{
    /// <summary>
    /// The emulator returned an error response to the requested action.
    /// </summary>
    public class X3270ifActionException : X3270ifException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="X3270ifActionException"/> class.
        /// </summary>
        /// <param name="text">Description of exception</param>
        public X3270ifActionException(string text) : base(text)
        {
        }
    }
}
