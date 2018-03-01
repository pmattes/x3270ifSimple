// <copyright file="x3270if.cs" company="Paul Mattes">
//     Copyright (c) Paul Mattes. All rights reserved.
// </copyright>

namespace X3270if
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// Simplified x3270 interface class, usable as a DLL or via COM.
    /// </summary>
    /// <remarks>Abstract class, cannot be constructed directly.</remarks>
    public abstract class X3270if : IDisposable
    {
        /// <summary>
        /// The prefix added to all output lines.
        /// </summary>
        private const string DataPrefix = "data: ";

        /// <summary>
        /// The success prompt.
        /// </summary>
        private const string OkPrompt = "\nok\n";

        /// <summary>
        /// The failure prompt.
        /// </summary>
        private const string ErrorPrompt = "\nerror\n";

        /// <summary>
        /// The encoding that the emulator is using.
        /// </summary>
        private Encoding encoding = new UTF8Encoding(false);

        /// <summary>
        /// The read stream.
        /// </summary>
        private StreamReader reader;

        /// <summary>
        /// The write stream.
        /// </summary>
        private StreamWriter writer;

        /// <summary>
        /// The debug string.
        /// </summary>
        private StringBuilder debugString;

        /// <summary>
        /// To detect redundant Dispose calls.
        /// </summary>
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="X3270if"/> class.
        /// </summary>
        public X3270if()
        {
            this.debugString = new StringBuilder();
        }

        /// <summary>
        /// Gets the most recent status line.
        /// </summary>
        [ComVisible(true)]
        public string StatusLine { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether to log debug messages to <see cref="Console.Error"/>.
        /// </summary>
        [ComVisible(true)]
        public bool Debug { get; set; }

        /// <summary>
        /// Gets debug output for the last action.
        /// </summary>
        [ComVisible(true)]
        public string DebugOutput
        {
            get
            {
                return this.debugString.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the TCP session to the emulator.
        /// </summary>
        protected TcpClient Client { get; set; }

        /// <summary>
        /// Quote a parameter appropriately for the emulator.
        /// </summary>
        /// <param name="param">Parameter to quote</param>
        /// <returns>Quoted parameter</returns>
        /// <remarks>
        /// From the original Xt Intrinsics manual:
        /// A quoted string may contain an embedded quotation mark if the quotation mark is preceded by a single backslash (\).
        /// The three-character sequence ‘‘\\"’’ is interpreted as ‘‘single backslash followed by end-of-string’’.
        /// This is simpler (and more subtle) than it seems.
        /// </remarks>
        [ComVisible(true)]
        public static string Quote(string param)
        {
            // Characters that always trigger quoting. The left paren is not techinically necessary, but things are less
            // confusing if it is quoted. A double quote at the beginning of the string is also a trigger.
            // Note that neither a double quote elsewhere nor a backslash trigger quoting.
            const string QuotedChars = " ,()";

            // We translate empty strings to empty quoted strings, though this is also not technically required.
            if (param.Equals(string.Empty))
            {
                return "\"\"";
            }

            if (!param.Any(c => QuotedChars.Contains(c)) && !param.StartsWith("\""))
            {
                return param;
            }

            var content = param.Replace("\"", "\\\"");
            if (content.EndsWith(@"\"))
            {
                content += @"\";
            }

            return "\"" + content + "\"";
        }

        /// <summary>
        /// Basic emulator I/O function.
        /// Given a correctly-formatted action and argument string, send it and return the reply.
        /// </summary>
        /// <param name="text">The action name and arguments to pass to the emulator.
        /// Must be formatted correctly. This method does no translation.</param>
        /// <returns>Result string, multiple lines separated by <see cref="Environment.NewLine"/></returns>
        /// <exception cref="ArgumentException">Control characters in text</exception>
        /// <exception cref="InvalidOperationException">Object in wrong state</exception>
        /// <exception cref="X3270ifActionException">Action failed: emulator returned an error</exception>
        /// <exception cref="X3270ifDisconnectException">Lost emulator connection</exception>
        /// <remarks>
        /// The emulator prompt is saved in <see cref="StatusLine"/>.
        /// Debug output (exact strings sent and received) is saved in <see cref="DebugOutput"/>.
        /// </remarks>
        /// <example>
        /// <code language="cs">
        /// // C#
        /// using X3270if;
        /// // ...
        /// // Set up the connection.
        /// var worker = new WorkerConnection();
        /// 
        /// // Query the cursor position.
        /// var cursor = worker.RunRaw("Query(Cursor)"));
        /// var cursorSplit = cursor.Split(' ');
        /// var row = int.Parse(cursorSplit[0]);
        /// var column = int.Parse(cursorSplit[1]);
        /// 
        /// // Move the cursor over one column, and down one row.
        /// worker.RunRaw(string.Format("MoveCursor({0},{1})", row + 1, column + 1);
        /// </code>
        /// <code language="PowerShell">
        /// # PowerShell
        /// # Load the x3270 interface DLL.
        /// Add-Type -Path "C:\Program Files\x3270ifSimple\x3270ifSimple.dll"
        /// # Set up the connection.
        /// $worker = New-Object -TypeName x3270if.WorkerConnection
        /// 
        /// # Query the cursor position.
        /// $cursor = $worker.RunRaw("Query(Cursor)")
        /// $cursorSplit = $cursor.Split(" ")
        /// $row = $cursorSplit[0] -as [int]
        /// $column = $cursorSplit[1] -as [int]
        /// 
        /// # Move the cursor over one column, and down one row.
        /// $worker.RunRaw([string]::Format("MoveCursor({0},{1})", $row + 1, $column + 1))
        /// </code>
        /// <code language="VBScript">
        /// ' VBScript
        /// ' Create the connection to the emulator.
        /// set worker = CreateObject("x3270if.WorkerConnection")
        /// 
        /// ' Query the cursor position.
        /// cursor = worker.RunRaw("Query(Cursor)")
        /// cursorSplit = Split(cursor, " ")
        /// row = CInt(cursorSplit(0))
        /// column = CInt(cursorSplit(1))
        /// 
        /// ' Move the cursor over one column, and down one row.
        /// r = worker.RunRaw("MoveCursor(" &amp; CStr(row + 1) &amp; "," &amp; CStr(column + 1) &amp; ")")
        /// </code>
        /// <code language="JScript">
        /// // JScript
        /// // Create the connection to the emulator.
        /// var worker = new ActiveXObject("x3270if.WorkerConnection");
        /// 
        /// // Query the cursor position.
        /// var cursor = worker.RunJScriptArray("Query", new Array("Cursor"));
        /// var cursorSplit = cursor.split(" ");
        /// var row = parseInt(cursorSplit[0]);
        /// var column = parseInt(cursorSplit[1]);
        /// 
        /// // Move the cursor over one column, and down one row.
        /// worker.RunRaw("MoveCursor(" + (row + 1) + "," + (column + 1) + ")");
        /// </code>
        /// </example>
        [ComVisible(true)]
        public string RunRaw(string text)
        {
            // Start with clear debug output.
            this.debugString.Clear();

            // Control characters are verboten.
            if (text.Any(c => char.IsControl(c)))
            {
                throw new ArgumentException("Text contains control character(s)");
            }

            this.Log("Run: sending '{0}'", text);

            if (this.writer == null)
            {
                throw new InvalidOperationException("No output stream -- missing Start()?");
            }

            var reply = new StringBuilder();
            var failed = false;
            try
            {
                // Write out the action and arguments.
                this.writer.Write(text + "\n");
                this.writer.Flush();

                // Read until we get a prompt.
                var buf = new char[1024];
                while (true)
                {
                    var nr = this.reader.Read(buf, 0, buf.Length);
                    if (nr == 0)
                    {
                        throw new X3270ifDisconnectException("Emulator disconnected");
                    }

                    reply.Append(buf, 0, nr);
                    if (reply.ToString().EndsWith(OkPrompt))
                    {
                        break;
                    }

                    if (reply.ToString().EndsWith(ErrorPrompt))
                    {
                        failed = true;
                        break;
                    }
                }
            }
            catch (IOException e)
            {
                throw new X3270ifDisconnectException(e.Message);
            }

            // Break into lines. The array looks like:
            //  data: xxx
            //  data: ...
            //  status line
            //  ok or error
            //  (empty line)
            var replyLines = reply.ToString().Split('\n');
            this.Log("Run: got '{0}'", string.Join("<\\n>", replyLines));

            // Remember the status.
            this.StatusLine = replyLines[replyLines.Length - 3];

            // Format the reply data.
            var result = string.Join(
                Environment.NewLine,
                replyLines.Take(replyLines.Length - 3).Select(line => line.StartsWith(DataPrefix) ? line.Substring(DataPrefix.Length) : line));

            // If the action failed, throw an exception.
            if (failed)
            {
                throw new X3270ifActionException(result);
            }

            // Return the result.
            return result;
        }

        /// <summary>
        /// Run an action, transparently formatting the arguments.
        /// </summary>
        /// <param name="action">Action name</param>
        /// <param name="args">Action arguments</param>
        /// <returns>Result string, multiple lines separated by <see cref="Environment.NewLine"/></returns>
        /// <exception cref="ArgumentException">Control characters in text</exception>
        /// <exception cref="InvalidOperationException">Object in wrong state</exception>
        /// <exception cref="X3270ifActionException">Action failed: emulator returned an error</exception>
        /// <exception cref="X3270ifDisconnectException">Lost emulator connection</exception>
        /// <remarks>
        /// The emulator prompt is saved in <see cref="StatusLine"/>.
        /// Debug output (exact strings sent and received) is saved in <see cref="DebugOutput"/>.
        /// </remarks>
        /// <example>
        /// <code language="cs">
        /// // C#
        /// using X3270if;
        /// // ...
        /// // Set up the connection.
        /// var worker = new WorkerConnection();
        /// 
        /// // Query the cursor position.
        /// var cursor = worker.Run("Query", "Cursor");
        /// var cursorSplit = cursor.Split(' ');
        /// var row = int.Parse(cursorSplit[0]);
        /// var column = int.Parse(cursorSplit[1]);
        /// 
        /// // Move the cursor over one column, and down one row.
        /// worker.Run("MoveCursor", (row + 1).ToString(), (column + 1).ToString())
        /// </code>
        /// <code language="PowerShell">
        /// # PowerShell
        /// # Load the x3270 interface DLL.
        /// Add-Type -Path "C:\Program Files\x3270ifSimple\x3270ifSimple.dll"
        /// # Set up the connection.
        /// $worker = New-Object -TypeName x3270if.WorkerConnection
        /// 
        /// # Query the cursor position.
        /// $cursor = $worker.Run("Query", "Cursor")
        /// $cursorSplit = $cursor.Split(" ")
        /// $row = $cursorSplit[0] -as [int]
        /// $column = $cursorSplit[1] -as [int]
        /// 
        /// # Move the cursor over one column, and down one row.
        /// $worker.Run("MoveCursor", ($row + 1).ToString(), ($column + 1).ToString())
        /// </code>
        /// </example>
        [ComVisible(true)]
        public string Run(string action, params string[] args)
        {
            return this.Run(action, (IEnumerable<string>)args);
        }

        /// <summary>
        /// Run an action, transparently formatting the arguments.
        /// </summary>
        /// <param name="action">Action name</param>
        /// <param name="args">Action arguments</param>
        /// <returns>Result string, multiple lines separated by <see cref="Environment.NewLine"/></returns>
        /// <exception cref="ArgumentException">Control characters in text</exception>
        /// <exception cref="InvalidOperationException">Object in wrong state</exception>
        /// <exception cref="X3270ifActionException">Action failed: emulator returned an error</exception>
        /// <exception cref="X3270ifDisconnectException">Lost emulator connection</exception>
        /// <remarks>
        /// The emulator prompt is saved in <see cref="StatusLine"/>.
        /// Debug output (exact strings sent and received) is saved in <see cref="DebugOutput"/>.
        /// </remarks>
        /// <example>
        /// <code>
        /// // C#
        /// using System.Linq;
        /// using X3270if;
        /// // ...
        /// // Set up the connection.
        /// var worker = new WorkerConnection();
        /// // Query the cursor position.
        /// var cursor = worker.Run("Query", "Cursor");
        /// 
        /// // Move the cursor over one column, and down one row.
        /// var cursorSplit = cursor.Split(' ');
        /// var arr = new string[2];
        /// arr[0] = (int.Parse(cursorSplit[0]) + 1).ToString();
        /// arr[1] = (int.Parse(cursorSplit[1]) + 1).ToString();
        /// worker.Run("MoveCursor", arr);
        /// 
        /// // Move the cursor over one column, and down one row (too cleverly).
        /// worker.Run("MoveCursor", cursor.Split(' ').Select(c => (int.Parse(c) + 1).ToString()))
        /// </code>
        /// </example>
        [ComVisible(true)]
        public string Run(string action, IEnumerable<string> args)
        {
            return this.RunRaw(action + "(" + string.Join(",", args.Select(arg => Quote(arg))) + ")");
        }

        /// <summary>
        /// Run an action, transparently formatting the arguments. For PowerShell, which uses untyped arrays by default.
        /// </summary>
        /// <param name="action">Action name</param>
        /// <param name="args">Action arguments</param>
        /// <returns>Result string, multiple lines separated by <see cref="Environment.NewLine"/></returns>
        /// <exception cref="ArgumentException">Control characters in text</exception>
        /// <exception cref="InvalidOperationException">Object in wrong state</exception>
        /// <exception cref="X3270ifActionException">Action failed: emulator returned an error</exception>
        /// <exception cref="X3270ifDisconnectException">Lost emulator connection</exception>
        /// <remarks>
        /// The emulator prompt is saved in <see cref="StatusLine"/>.
        /// Debug output (exact strings sent and received) is saved in <see cref="DebugOutput"/>.
        /// </remarks>
        /// <example>
        /// <code language="PowerShell">
        /// # PowerShell
        /// # Load the x3270 interface DLL.
        /// Add-Type -Path "C:\Program Files\x3270ifSimple\x3270ifSimple.dll"
        /// # Set up the connection.
        /// $worker = New-Object -TypeName x3270if.WorkerConnection
        /// 
        /// # Query the cursor position.
        /// $cursor = $worker.Run("Query", "Cursor")
        /// $cursorSplit = $cursor.Split(" ")
        /// $row = $cursorSplit[0] -as [int]
        /// $column = $cursorSplit[1] -as [int]
        /// 
        /// # Move the cursor over one column, and down one row.
        /// $a = @()
        /// $a += $row + 1
        /// $a += $column + 1
        /// $worker.Run("MoveCursor", $a)
        /// </code>
        /// </example>
        [ComVisible(true)]
        public string Run(string action, object[] args)
        {
            return this.RunRaw(action + "(" + string.Join(",", args.Select(arg => Quote(arg.ToString()))) + ")");
        }

        /// <summary>
        /// Run an action, transparently formatting the arguments. For VBScript, which uses untyped (variant) arrays, and doesn't understand method overloading
        /// or variable/optional arguments, so it cannot use any of the forms of Run.
        /// </summary>
        /// <param name="action">Action name</param>
        /// <param name="args">Action arguments</param>
        /// <returns>Result string, multiple lines separated by <see cref="Environment.NewLine"/></returns>
        /// <exception cref="ArgumentException">Control characters in text</exception>
        /// <exception cref="InvalidOperationException">Object in wrong state</exception>
        /// <exception cref="X3270ifActionException">Action failed: emulator returned an error</exception>
        /// <exception cref="X3270ifDisconnectException">Lost emulator connection</exception>
        /// <remarks>
        /// The emulator prompt is saved in <see cref="StatusLine"/>.
        /// Debug output (exact strings sent and received) is saved in <see cref="DebugOutput"/>.
        /// </remarks>
        /// <example>
        /// <code language="vbscript">
        /// ' VBScript
        /// ' Create the connection to the emulator.
        /// set worker = CreateObject("x3270if.WorkerConnection")
        /// 
        /// ' Query the cursor position.
        /// cursor = worker.RunSafeArray("Query", Array("Cursor"))
        /// cursorSplit = Split(cursor, " ")
        /// row = int(cursorSplit(0))
        /// column = int(cursorSplit(1))
        /// 
        /// ' Move the cursor over one column and down one row.
        /// r = worker.RunSafeArray("MoveCursor", Array(row + 1, column + 1))
        /// </code>
        /// </example>
        [ComVisible(true)]
        public string RunSafeArray(string action, object[] args)
        {
            return this.Run(action, args);
        }

        /// <summary>
        /// Run an action, transparently formatting the arguments. For JScript, which uses its own form of arrays and doesn't understand method overloading or
        /// variable/optional arguments.
        /// </summary>
        /// <param name="action">Action name</param>
        /// <param name="args">JScript array of arguments</param>
        /// <returns>Result string, multiple lines separated by <see cref="Environment.NewLine"/></returns>
        /// <exception cref="ArgumentException">Control characters in text</exception>
        /// <exception cref="InvalidOperationException">Object in wrong state</exception>
        /// <exception cref="X3270ifActionException">Action failed: emulator returned an error</exception>
        /// <exception cref="X3270ifDisconnectException">Lost emulator connection</exception>
        /// <remarks>
        /// The emulator prompt is saved in <see cref="StatusLine"/>.
        /// Debug output (exact strings sent and received) is saved in <see cref="DebugOutput"/>.
        /// </remarks>
        /// /// <example>
        /// <code language="jscript">
        /// // JScript
        /// // Create the connection to the emulator.
        /// var worker = new ActiveXObject("x3270if.WorkerConnection");
        /// 
        /// // Query the cursor position.
        /// var cursor = worker.RunJScriptArray("Query", new Array("Cursor"));
        /// var cursorSplit = cursor.split(" ");
        /// var row = parseInt(cursorSplit[0]);
        /// var column = parseInt(cursorSplit[1]);
        /// 
        /// // Then move the cursor back over one column and down one row.
        /// worker.RunJScriptArray("MoveCursor", new Array(row + 1, column + 1));
        /// </code>
        /// </example>
        [ComVisible(true)]
        public string RunJScriptArray(string action, object args)
        {
            dynamic dynArray = args;
            int length = dynArray.length;
            return this.RunRaw(action + "(" + string.Join(",", Enumerable.Range(0, length).Select(i => Quote(GetAt(args, i).ToString()))) + ")");
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        [ComVisible(true)]
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Set up the reader and writer.
        /// </summary>
        protected void SetupReaderWriter()
        {
            // Create temporary input and output streams.
            this.reader = new StreamReader(this.Client.GetStream(), this.encoding, false, 1024, leaveOpen: true);
            this.writer = new StreamWriter(this.Client.GetStream(), this.encoding, 1024, leaveOpen: true);
        }

        /// <summary>
        /// Set up the encoding dynamically.
        /// </summary>
        protected void SetupEncoding()
        {
            var encodingString = this.RunRaw("Query(LocalEncoding)");
            if (!encodingString.Equals("UTF-8", StringComparison.InvariantCultureIgnoreCase))
            {
                // Not UTF-8. Switch it.
                if (encodingString.StartsWith("CP"))
                {
                    this.encoding = Encoding.GetEncoding(int.Parse(encodingString.Substring(2)));
                }
                else
                {
                    this.encoding = Encoding.GetEncoding(encodingString);
                }

                // Re-open the input and output streams with the proper encoding.
                this.reader.Dispose();
                this.reader = new StreamReader(this.Client.GetStream(), this.encoding, false, 1024, leaveOpen: true);
                this.writer.Dispose();
                this.writer = new StreamWriter(this.Client.GetStream(), this.encoding, 1024, leaveOpen: true);
            }
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        /// <param name="disposing">True if disposing</param>
        [ComVisible(true)]
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    if (this.reader != null)
                    {
                        this.reader.Dispose();
                        this.reader = null;
                    }

                    if (this.writer != null)
                    {
                        this.writer.Dispose();
                        this.writer = null;
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

        /// <summary>
        /// Log debugging information.
        /// </summary>
        /// <param name="format">String format</param>
        /// <param name="args">Message parameters</param>
        protected void Log(string format, params object[] args)
        {
            var text = string.Format(format, args);
            if (this.Debug)
            {
                var foreground = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Error.WriteLine(text);
                Console.ForegroundColor = foreground;
            }

            if (this.debugString.Length > 0)
            {
                this.debugString.AppendLine();
            }

            this.debugString.Append(text);
        }

        /// <summary>
        /// Get a dynamic array object.
        /// </summary>
        /// <param name="array">Array to fetch from</param>
        /// <param name="index">Object \iIndex</param>
        /// <returns>Indexed element</returns>
        private static object GetAt(object array, int index)
        {
            return array.GetType().InvokeMember(
                index.ToString(),
                BindingFlags.Instance | BindingFlags.GetProperty,
                null,
                array,
                new object[] { });
        }
    }
}
