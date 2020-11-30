# x3270is
Simple .NET DLL to interface to x3270 emulators

The x3270is DLL is an interface between an application and a member of the x3270 emulator family (wc3270, s3270, wx3270).
It can be used in one of two modes:
- As a worker process, started by the wc3270/wx3270 **Script()** action to perform some action for an interactive login session
- As a headless screen-scraping application that starts its own copy of s3270 and manages all of the interactions with the host

x3270is can be used directly by .NET applications, and can also be registered with COM, so it can be used by scripting languages
like VBScript and JScript.
