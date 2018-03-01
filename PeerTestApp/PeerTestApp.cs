using System;
using System.IO;
using X3270if;

namespace PeerTestApp
{
    class PeerTestApp
    {
        static void Main(string[] args)
        {
            var f = new StreamWriter("foo.txt");
            f.AutoFlush = true;
            f.WriteLine("Started");

            var x = new NewEmulator();
            f.WriteLine("Peer created");
            f.WriteLine("Debug:" + x.DebugOutput);

            x.Start();
            f.WriteLine("Peer started");
            f.WriteLine("Debug:" + x.DebugOutput);

            var y = x.Run("Query", "Cursor");
            f.WriteLine("Cursor is {0}", y);
            f.WriteLine("Debug:" + x.DebugOutput);

            x.Dispose();

            Environment.Exit(0);
        }
    }
}
