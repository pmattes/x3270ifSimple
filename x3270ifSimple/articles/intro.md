
# X3270ifSimple Introduction
The x3270ifSimple library is a simple interface between applications and the x3270 emulator family. It can be used in one of two ways:
- To form the glue between an interactive wx3270 or wc3270 session and a scripting process started by the **Script()** action.
- To start a copy of ws3270 to completely control a host session.

## Script() Worker Process
wx3270 and wc3270 support a **Script()** action that allows running sophisticated scripts to control the emulator session. For example,
a script could read the contents of a file and paste it into fields on the host display, it could take data from the host display and
generate a report, or it could implement features not present in the emulator, such as the 5250 Field Exit key. (The key difference
between the **Script()** and **Source()** actions is that **Source()** blindly reads a set of actions from a file and executes them
one by one. **Script()** invokes a program that can perform different actions based on the state of the emulator and the result of
previous actions, and it can access outside resources.)

The **Script()** action takes one or more arguments.  The emulator will start up a copy of the program named by the first argument,
and will pass the remaining arguments as command-line parameters
to it. For example, to run a Powershell script, a typical invocation would be:
```
  Script(powershell.exe,-File,MyScript.ps1)
```
That would cause the emulator to start a copy of `powershell.exe` and pass it `-File` and `MyScript.ps1` as parameters.

Before starting the program, the emulator sets up a socket to accept connections to process commands from the script. The port number is
passed to the program in the environment variable `X3270PORT`. The socket communication protocol between the script and emulator is
simple but quite strict. The [WorkerConnection](../api/X3270if.WorkerConnection.html) class in the x3270ifSimple library handles
all of the details (setting up a socket, formatting and sending actions and decoding the results) so the script can focus on
doing its work.
## Pure Screen Scraping (New Host Session)
The x3270ifSimple library also supports pure screen scraping. A screen-scraping application starts a copy of ws3270 and uses it
to connect to a host. It then controls all aspects of the session, such as logging in, entering data and interpreting the results.
The [NewEmulator](../api/X3270if.NewEmulator.html) class in the x3270ifSimple library handles the details of starting
ws3270 and connecting to it, and uses the same methods as `WorkerConnection` to send actions and decode the results.
# Supported Clients
x3270ifSimple is a DLL that can be used by any .NET application, such as PowerShell or user-generated C# or VB.NET code. It can
also be registed with COM during installation of wx3270 or wc3270, so it can be used with VBScript and JScript.