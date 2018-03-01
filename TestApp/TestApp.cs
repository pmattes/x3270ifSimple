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
