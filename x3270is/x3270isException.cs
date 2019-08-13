// <copyright file="x3270isException.cs" company="Paul Mattes">
//     Copyright (c) Paul Mattes. All rights reserved.
// </copyright>

namespace X3270is
{
    using System;

    /// <summary>
    /// An exception specific to the <see cref="X3270is"/> class.
    /// </summary>
    public class X3270isException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="X3270isException"/> class.
        /// </summary>
        /// <param name="text">Description of exception.</param>
        public X3270isException(string text)
            : base(text)
        {
        }
    }
}
