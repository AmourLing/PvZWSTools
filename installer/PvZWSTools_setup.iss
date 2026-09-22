#define RepoRoot "D:\PvZWSTools"
#define MyAppName "PvZWSTools"
#ifndef MyAppVersion
  #define MyAppVersion "2026.09.02-fix1"
#endif
#ifndef MyAppId
  #define MyAppId "{{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}}"
#endif
#ifndef SourcePath
  #define SourcePath RepoRoot + "\publish\win-self-contained"
#endif
; 发布目录是「根放入口 exe + app\ 主程序」两层，图标只能按仓库源文件取。
; {app}\PvZWSTools.exe 是入口，不是主程序本体，[Icons]/[Run] 就指向它。
#ifndef MyIconFile
  #define MyIconFile RepoRoot + "\PvZWSTools_WPF\Resources\icon.ico"
#endif

#define MyAppPublisher "AmourLing"
#define MyAppURL ""
#define MyAppExeName "PvZWSTools.exe"

[Setup]
Compression=lzma2/ultra64
InternalCompressLevel=ultra
SolidCompression=yes
LZMAUseSeparateProcess=yes
LZMANumFastBytes=273
LZMADictionarySize=262144
AppId={#MyAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={localappdata}\{#MyAppName}
UsePreviousAppDir=yes
DisableDirPage=no
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
PrivilegesRequired=lowest
OutputDir=Output
OutputBaseFilename=PvZWSTools_Setup
SetupIconFile="{#MyIconFile}"
WizardStyle=modern
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "chinesesimplified"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: checkablealone
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
Source: "{#SourcePath}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup: Boolean;
begin
  Result := True;
end;
