[Setup]
AppName=Lavappies
AppVersion=1.0
DefaultDirName={pf}\Lavappies
DefaultGroupName=Lavappies
OutputDir=.
OutputBaseFilename=LavappiesSetup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
SetupIconFile=logo.ico
UninstallDisplayIcon={app}\Lavappies.exe

[Files]
Source: "bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\Lavappies"; Filename: "{app}\Lavappies.exe"
Name: "{group}\Uninstall Lavappies"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\Lavappies.exe"; Description: "Launch Lavappies"; Flags: postinstall nowait

[Code]
procedure InitializeWizard;
begin
  WizardForm.WelcomeLabel1.Caption := 'Welcome to the Lavappies Setup Wizard';
  WizardForm.WelcomeLabel2.Caption := 'This will install Lavappies on your computer.';
end;