// <copyright file="WorkerConnection.cs" company="Paul Mattes">
//     Copyright (c) Paul Mattes. All rights reserved.
// </copyright>

namespace X3270is
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Simplified x3270 worker interface class (for scripts started via Script()), usable as a DLL or via COM.
    /// </summary>
    [ComVisible(true)]
    public class WorkerConnection : X3270is, IDisposable
    {
        /// <summary>
        /// The name of the environment variable giving the port.
        /// </summary>
        private const string PortName = "X3270PORT";

        /// <summary>
        /// To detect redundant Dispose calls.
        /// </summary>
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkerConnection"/> class.
        /// </summary>
        public WorkerConnection()
            : base()
        {
            // Get the port from the environment.
            var portString = Environment.GetEnvironmentVariable(PortName);
            if (portString == null)
            {
                throw new X3270isException(string.Format("{0} not found in the environment", PortName));
            }

            // Parse it.
            if (!ushort.TryParse(portString, out ushort port16))
            {
                throw new X3270isException(string.Format("Invalid {0} '{1}'", PortName, portString));
            }

            // Connect to it.
            this.Client = new TcpClient();
            this.Client.Connect(new IPEndPoint(IPAddress.Loopback, port16));

            // Set up the reader and writer, and set the encoding if needed.
            this.SetupReaderWriter();
            this.SetupEncoding();
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
        /// <param name="disposing">True if disposing.</param>
        protected virtual new void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed resources here.
                }

                this.disposedValue = true;
            }
        }
    }
}
