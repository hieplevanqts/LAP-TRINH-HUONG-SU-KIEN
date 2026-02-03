; Inno Setup Script - Hotel Manager
#define MyAppName "Hotel Manager"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Hotel Manager"
#define MyAppExeName "HotelManager.WinForms.exe"

[Setup]
AppId={{F3C8A8B2-9D9B-4E55-AE8D-53B5C20A0F3A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputBaseFilename=HotelManagerSetup
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Files]
Source: "..\src\HotelManager.WinForms\bin\Release\net10.0-windows\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\installer\ThirdParty\SqlLocalDB.msi"; DestDir: "{tmp}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"

[Run]
Filename: "msiexec.exe"; Parameters: "/i ""{tmp}\SqlLocalDB.msi"" IAcceptSqlLocalDBLicenseTerms=YES /qn"; StatusMsg: "Đang cài SQL Server LocalDB..."; Flags: waituntilterminated; Check: ShouldInstallLocalDb
Filename: "{app}\{#MyAppExeName}"; Description: "Chạy {#MyAppName}"; Flags: nowait postinstall skipifsilent

[Code]
function ShouldInstallLocalDb(): Boolean;
begin
  { LocalDB 2019 writes version under: HKLM\SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions\15.0 }
  Result := not RegKeyExists(HKLM, 'SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions\15.0');
end;
