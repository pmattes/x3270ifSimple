// <copyright file="x3270isActionException.cs" company="Paul Mattes">
//     Copyright (c) Paul Mattes. All rights reserved.
// </copyright>

namespace X3270is
{
    /// <summary>
    /// The emulator returned an error response to the requested action.
    /// </summary>
    public class X3270isActionException : X3270isException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="X3270isActionException"/> class.
        /// </summary>
        /// <param name="text">Description of exception.</param>
        public X3270isActionException(string text)
            : base(text)
        {
        }
    }
}
