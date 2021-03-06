﻿// <copyright file="PeerTestApp.cs" company="Paul Mattes">
//     Copyright (c) Paul Mattes. All rights reserved.
// </copyright>

namespace PeerTestApp
{
    using System;
    using System.IO;
    using X3270is;

    /// <summary>
    /// Test application for a screen scraping app (not interactive).
    /// </summary>
    public class PeerTestApp
    {
        /// <summary>
        /// Main procedure.
        /// </summary>
        public static void Main()
        {
            using (var f = new StreamWriter("foo.txt"))
            {
                f.AutoFlush = true;
                f.WriteLine("Started");

                using (var x = new NewEmulator())
                {
                    f.WriteLine("Peer created");
                    f.WriteLine("Debug:" + x.DebugOutput);

                    x.Start();
                    f.WriteLine("Peer started");
                    f.WriteLine("Debug:" + x.DebugOutput);

                    var y = x.Run("Query", "Cursor");
                    f.WriteLine("Cursor is {0}", y);
                    f.WriteLine("Debug:" + x.DebugOutput);
                }
            }

            Environment.Exit(0);
        }
    }
}
