// <copyright file="HostSpecification.cs" company="Paul Mattes">
//     Copyright (c) Paul Mattes. All rights reserved.
// </copyright>

namespace X3270if
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// An x3270 host specification. Constructs all of the various options in the right order.
    /// </summary>
    public class HostSpecification
    {
        /// <summary>
        /// Invalid host name characters.
        /// </summary>
        private const string InvalidHostCharacters = "@,[]=";

        /// <summary>
        /// Valid logical unit characters.
        /// </summary>
        private const string ValidLuCharacters = "ABCDEFGHIJKLMNOPQRSTUVWYZabcdefghijklmnopqrstuvwxyz0123456789_-";

        /// <summary>
        /// Invalid accept name characters.
        /// </summary>
        private const string InvalidAcceptCharacters = InvalidHostCharacters + ":";

        /// <summary>
        /// The default port number.
        /// </summary>
        private const int DefaultPort = 23;

        /// <summary>
        /// Host name.
        /// </summary>
        private string hostName;

        /// <summary>
        /// Backing field for <see cref="Port"/>.
        /// </summary>
        private int port = DefaultPort;

        /// <summary>
        /// The list of logical units.
        /// </summary>
        private List<string> logicalUnits = new List<string>();

        /// <summary>
        /// TLS host certificate validation flag.
        /// </summary>
        private bool validateHostCertificate = true;

        /// <summary>
        /// The host TLS certificate accept name.
        /// </summary>
        private string acceptName;

        /// <summary>
        /// Initializes a new instance of the <see cref="HostSpecification"/> class.
        /// </summary>
        public HostSpecification()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HostSpecification"/> class.
        /// </summary>
        /// <param name="hostName">Host name</param>
        public HostSpecification(string hostName)
        {
            this.HostName = hostName;
        }

        /// <summary>
        /// Gets or sets the host name.
        /// </summary>
        public string HostName
        {
            get
            {
                return this.hostName;
            }

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Is null or empty");
                }

                if (value.ToCharArray().Any(c => InvalidHostCharacters.Contains(c)))
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Contains invalid character(s)");
                }

                this.hostName = value.Trim();
            }
        }

        /// <summary>
        /// Gets or sets the port number.
        /// </summary>
        public int Port
        {
            get
            {
                return this.port;
            }

            set
            {
                if (value < 1 || (uint)value > ushort.MaxValue)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Must be non-zero and fit in 16 bits");
                }

                this.port = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to set up a TLS tunnel. The default is false.
        /// </summary>
        public bool TlsTunnel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to validate the host TLS certificate. The default is true.
        /// </summary>
        public bool ValidateHostCertificate
        {
            get { return this.validateHostCertificate; }
            set { this.validateHostCertificate = value; }
        }

        /// <summary>
        /// Gets or sets the logical unit names.
        /// </summary>
        public IEnumerable<string> LogicalUnits
        {
            get
            {
                return this.logicalUnits;
            }

            set
            {
                this.logicalUnits.Clear();
                foreach (var lu in value)
                {
                    this.AddLogicalUnitName(lu);
                }
            }
        }

        /// <summary>
        /// Gets or sets the host TLS certificate accept name.
        /// </summary>
        public string AcceptName
        {
            get
            {
                return this.acceptName;
            }

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Is null or empty");
                }

                if (value.ToCharArray().Any(c => InvalidAcceptCharacters.Contains(c)))
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Contains invalid character(s)");
                }

                this.acceptName = value;
            }
        }

        /// <summary>
        /// Add a logical unit (LU) name.
        /// </summary>
        /// <param name="logicalUnitName">LU name</param>
        public void AddLogicalUnitName(string logicalUnitName)
        {
            if (string.IsNullOrWhiteSpace(logicalUnitName))
            {
                throw new ArgumentOutOfRangeException(nameof(logicalUnitName), "Is null or empty");
            }

            if (logicalUnitName.ToCharArray().Any(c => !ValidLuCharacters.Contains(c)))
            {
                throw new ArgumentOutOfRangeException(nameof(logicalUnitName), "Contains invalid character(s)");
            }

            this.logicalUnits.Add(logicalUnitName.Trim());
        }

        /// <summary>
        /// Construct the string representation.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            if (this.hostName == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            if (this.TlsTunnel)
            {
                sb.Append("L:");
            }

            if (!this.ValidateHostCertificate)
            {
                sb.Append("Y:");
            }

            if (this.logicalUnits.Count > 0)
            {
                sb.Append(string.Join(",", this.logicalUnits) + "@");
            }

            if (this.hostName.Contains(":"))
            {
                sb.Append("[" + this.hostName + "]");
            }
            else
            {
                sb.Append(this.hostName);
            }

            if (this.Port != DefaultPort)
            {
                sb.Append(":" + this.Port);
            }

            if (this.AcceptName != null)
            {
                sb.Append("=" + this.AcceptName);
            }

            return sb.ToString();
        }
    }
}
