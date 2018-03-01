// <copyright file="NewEmulator.cs" company="Paul Mattes">
//     Copyright (c) Paul Mattes. All rights reserved.
// </copyright>

namespace X3270if
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Simplified x3270 new emulator class (starts a new copy of ws3270).
    /// </summary>
    [ComVisible(true)]
    public class NewEmulator : X3270if, IDisposable
    {
        /// <summary>
        /// The minimum emulator version required.
        /// </summary>
        private const string MinVersion = "3.6.5";

        /// <summary>
        /// The name of s3270.
        /// </summary>
        private const string S3270 = "ws3270.exe";

        /// <summary>
        /// Started emulator process.
        /// </summary>
        private Process process = null;

        /// <summary>
        /// To detect redundant Dispose calls.
        /// </summary>
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewEmulator"/> class.
        /// </summary>
        public NewEmulator() : base()
        {
        }

        /// <summary>
        /// Gets or sets the extra options.
        /// </summary>
        [ComVisible(true)]
        public string ExtraOptions { get; set; }

        /// <summary>
        /// Start the s3270 process.
        /// </summary>
        [ComVisible(true)]
        public void Start()
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                // Find a free TCP port to use for communication.
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
                socket.Bind(new IPEndPoint(IPAddress.Any, 0));
                int port = ((IPEndPoint)socket.LocalEndPoint).Port;

                // Start with basic arguments:
                //  -minversion       make sure we have a new-enough version
                //  -utf8             UTF-8 mode
                //  -scriptport       specify a script listening port
                //  -scriptportonce   Make sure the emulator exists if the socket connection breaks
                var arguments = string.Format("-minversion {0} -utf8 -scriptport {1} -scriptportonce", MinVersion, port);

                // Add arbitrary extra options.
                if (!string.IsNullOrEmpty(this.ExtraOptions))
                {
                    arguments += " " + this.ExtraOptions;
                }

                // Check for argument overflow.
                // At some point, we could automatically put most arguments into a temporary session file, but for now,
                // we blow up, so arguments aren't silently ignored.
                var argsMax = (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1) ? 32699 : 2080;
                if (S3270.Length + 1 + arguments.Length > argsMax)
                {
                    throw new InvalidOperationException("Arguments too long");
                }

                // Start the process.
                var info = new ProcessStartInfo(S3270)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    Arguments = arguments
                };
                this.Log("NewEmulator Start: ProcessName '{0}', arguments '{1}'", S3270, info.Arguments);
                try
                {
                    this.process = Process.Start(info);
                }
                catch (Win32Exception e)
                {
                    throw new X3270ifException(S3270 + ": " + e.Message);
                }

                // Connect to it.
                this.Client = new TcpClient();
                try
                {
                    this.Client.Connect(new IPEndPoint(IPAddress.Loopback, port));
                }
                catch (SocketException)
                {
                    var s3270Error = this.process.StandardError.ReadToEnd();
                    if (!string.IsNullOrEmpty(s3270Error))
                    {
                        // ws3270 had something to say on the way down.
                        throw new X3270ifException("ws3270 failure: " + s3270Error.TrimEnd(Environment.NewLine.ToCharArray()));
                    }

                    throw;
                }

                // Set up the reader and writer. No need to switch encoding.
                this.SetupReaderWriter();
            }
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        [ComVisible(true)]
        public new void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
            base.Dispose();
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        /// <param name="disposing">True if disposing</param>
        protected virtual new void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    if (this.process != null)
                    {
                        try
                        {
                            this.process.Kill();
                        }
                        catch
                        {
                            // Might have already exited.
                        }

                        this.process.Dispose();
                        this.process = null;
                    }

                    if (this.Client != null)
                    {
                        this.Client.Client.Shutdown(SocketShutdown.Both);
                        this.Client.Close();
                        this.Client = null;
                    }
                }

                this.disposedValue = true;
            }
        }
    }
}
