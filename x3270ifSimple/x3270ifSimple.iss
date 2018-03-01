; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

[Setup]
AllowNoIcons=yes
AppCopyright=Copyright (C) 2017-2018 by Paul Mattes
AppName=x3270ifSimple
AppPublisher=Paul Mattes
AppPublisherURL=http://x3270.bgp.nu
AppSupportURL=http://x3270.bgp.nu
AppUpdatesURL=http://x3270.bgp.nu
AppVerName=x3270ifSimple 1.0
ArchitecturesInstallIn64BitMode=x64
ChangesAssociations=yes
Compression=lzma
DefaultDirName={pf}\x3270ifSimple
DefaultGroupName=x3270ifSimple
DisableDirPage=no
MinVersion=0,5.1
OutputBaseFilename=x3270ifSimple-setup
OutputDir=.
SolidCompression=yes
WizardSmallImageFile=x3270ifSimple.bmp
SignTool=signtool

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "bin\x86\Release\x3270ifSimple.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: not Is64BitInstallMode
Source: "bin\x64\Release\x3270ifSimple.dll"; DestDir: "{app}"; Flags: ignoreversion; Check: Is64BitInstallMode

[Tasks]
Name: "com"; Description: "Register COM object"

[Run]
Filename: "{dotnet40}\RegAsm.exe"; Parameters: /codebase {app}\x3270ifSimple.dll; WorkingDir: {app}; StatusMsg: "Registering Controls..."; Flags: runminimized; Tasks: com

[UninstallDelete]
Type: dirifempty; Name: "{app}"
