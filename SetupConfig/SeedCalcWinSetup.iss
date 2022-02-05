; Copyright 2021-2022 The SeedV Lab.
;
; Licensed under the Apache License, Version 2.0 (the "License");
; you may not use this file except in compliance with the License.
; You may obtain a copy of the License at
;
;     http://www.apache.org/licenses/LICENSE-2.0
;
; Unless required by applicable law or agreed to in writing, software
; distributed under the License is distributed on an "AS IS" BASIS,
; WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
; See the License for the specific language governing permissions and
; limitations under the License.

; Inno Setup config script for the Windows release of SeedCalc.

#define SeedCalc "SeedCalc"
#define SeedCalcVersion "v1.0.0+3"
#define SeedCalcVersionNumber StringChange(StringChange(SeedCalcVersion, "v", ""), "+", ".")
#define SeedVLab "The SeedV Lab"
#define SeedCalcCopyright "Copyright © 2021-2022 The SeedV Lab"
#define SeedCalcURL "https://www.seedv.cn/products/seedcalc/"
#define SeedCalcUnityOutputDir ".\SeedCalcWin"
#define SeedCalcExeName "SeedCalc.exe"

[Setup]
AppId={{75307DE4-8B22-47DB-ACC9-A97AD1ED79DF}
AppName={cm:DisplayAppName}
AppVersion={#SeedCalcVersion}
AppPublisher={#SeedVLab}
AppPublisherURL={#SeedCalcURL}
AppSupportURL={#SeedCalcURL}
AppUpdatesURL={#SeedCalcURL}
AppCopyright={#SeedCalcCopyright}
DefaultDirName={autopf}\{#SeedCalc}
DisableProgramGroupPage=yes
PrivilegesRequiredOverridesAllowed=dialog
OutputDir=.
OutputBaseFilename=SeedCalcWinSetup_v{#SeedCalcVersionNumber}
Compression=lzma
SolidCompression=yes
VersionInfoDescription={#SeedCalc} Setup
VersionInfoProductName={#SeedCalc}
VersionInfoVersion={#SeedCalcVersionNumber}
WizardStyle=modern

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "zh"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"

[CustomMessages]
en.DisplayAppName=SeedCalc - A Wonder Calculator
zh.DisplayAppName=看得见的计算器

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SeedCalcUnityOutputDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{cm:DisplayAppName}"; Filename: "{app}\{#SeedCalcExeName}"
Name: "{autodesktop}\{cm:DisplayAppName}"; Filename: "{app}\{#SeedCalcExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#SeedCalcExeName}"; Description: "{cm:LaunchProgram, {cm:DisplayAppName}}"; Flags: nowait postinstall skipifsilent
