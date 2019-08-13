// <copyright file="TestApp.cs" company="Paul Mattes">
//     Copyright (c) Paul Mattes. All rights reserved.
// </copyright>

namespace PeerTestApp
{
    using System;
    using System.IO;
    using X3270is;

    /// <summary>
    /// Test application for a child script (runs under wc3270 or wx3270).
    /// </summary>
    public class TestApp
    {
        /// <summary>
        /// Main procedure.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        public static void Main(string[] args)
        {
            var f = new StreamWriter("foo.txt");
            f.WriteLine("Started");
            f.Flush();

            var x = new WorkerConnection();
            f.WriteLine("WorkerConnection created");
            f.Flush();

            var y = x.Run("Query", "Cursor");
            f.WriteLine("Cursor is {0}", y);
            f.Flush();

            Environment.Exit(0);
        }
    }
}
