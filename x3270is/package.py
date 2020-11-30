# Python script to build and package x3270is

import subprocess
import sys

verbose = len(sys.argv) > 1 and sys.argv[1] == "-v"

if verbose:
    msbuild_verbose = "/v:n"
    signtool_verbose = "/v"
    inno_verbose = ""
else:
    msbuild_verbose = "/v:q"
    signtool_verbose = "/q"
    inno_verbose = "/Qp"

# Get the certificate file and password.
cert = input("Certificate path: ")
password = input("Password: ")

# Build the code.
print("***** Building x86 code *****")
subprocess.run(["msbuild", "/t:Rebuild", "/p:Configuration=Release;Platform=x86", "/nologo", msbuild_verbose], check=True)
print("***** Building x64 code *****")
subprocess.run(["msbuild", "/t:Rebuild", "/p:Configuration=Release;Platform=x64", "/nologo", msbuild_verbose], check=True)

# Sign it.
print("***** Signing DLLs and EXEs *****")
subprocess.run(["signtool.exe", "sign", "/a", signtool_verbose, "/f", cert, "/p", password, "/t", "http://timestamp.comodoca.com/authenticode", "bin\\x86\\Release\\x3270is.dll", "bin\\x64\\Release\\x3270is.dll", "bin\\x86\\Release\\Import\\x86\\s3270.exe", "bin\\x64\\Release\\Import\\x64\\s3270.exe"], check=True)

# Package it.
print("***** Building installer *****")
inno_cmd = ["C:\\Program Files (x86)\\Inno Setup 5\\iscc.exe"];
if inno_verbose != "": inno_cmd += [ inno_verbose ]
inno_cmd += ["/ssigntool=signtool.exe sign " + signtool_verbose + " /a /f $q" + cert + "$q /t http://timestamp.comodoca.com/authenticode /p " + password + " $f", "x3270is.iss"]
subprocess.run(inno_cmd, check=True)
