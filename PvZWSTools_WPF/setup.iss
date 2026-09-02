; PvZWSTools Inno Setup 安装脚本
; 用法: ISCC.exe setup.iss

#define MyAppName "PvZWSTools"
#define MyAppVersion "2026.09.02-fix1"
#define MyAppPublisher "AmourLing"
#define MyAppExeName "PvZWSTools.exe"

[Setup]
AppId={{B8E9D4E5-3A7C-4D5E-8F2A-1B3C5D7E9F0A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputBaseFilename=PvZWSTools_windows_setup
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
DisableProgramGroupPage=yes
; 这个安装包是独立分发，不走自动更新
UninstallDisplayIcon={app}\{#MyAppExeName}
;SetupIconFile=..\Resources\icon.ico
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

[Languages]
Name: "chinesetraditional"; MessagesFile: "compiler:Languages\ChineseTraditional.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式"; GroupDescription: "附加图标:"

[Files]
Source: "..\publish\win-self-contained\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "运行 {#MyAppName}"; Flags: nowait postinstall skipifsilent
