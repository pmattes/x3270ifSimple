# Python script to build and package x3270ifSimple

import subprocess

# Get the certificate file and password.
cert = input("Certificate path: ")
password = input("Password: ")

# Build the code.
print("Building x86 code.")
subprocess.run(["msbuild", "/t:Rebuild", "/p:Configuration=Release;Platform=x86", "/nologo", "/v:q"], check=True)
print("Building x64 code.")
subprocess.run(["msbuild", "/t:Rebuild", "/p:Configuration=Release;Platform=x64", "/nologo", "/v:q"], check=True)

# Sign it.
print("Signing DLLs.")
subprocess.run(["signtool.exe", "sign", "/q", "/a", "/f", cert, "/p", password, "bin\\x86\\Release\\x3270ifSimple.dll", "bin\\x64\\Release\\x3270ifSimple.dll"], check=True)

# Package it.
print("Building installer.")
subprocess.run(["C:\\Program Files (x86)\\Inno Setup 5\\iscc.exe", "/Qp", "/ssigntool=signtool.exe sign /q /a /f $q" + cert + "$q /t http://timestamp.comodoca.com/authenticode /p " + password + " $f", "x3270ifSimple.iss"], check=True)