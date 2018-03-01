// <copyright file="x3270ifException.cs" company="Paul Mattes">
//     Copyright (c) Paul Mattes. All rights reserved.
// </copyright>

namespace X3270if
{
    using System;

    /// <summary>
    /// An exception specific to the <see cref="X3270if"/> class.
    /// </summary>
    public class X3270ifException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="X3270ifException"/> class.
        /// </summary>
        /// <param name="text">Description of exception</param>
        public X3270ifException(string text) : base(text)
        {
        }
    }
}
