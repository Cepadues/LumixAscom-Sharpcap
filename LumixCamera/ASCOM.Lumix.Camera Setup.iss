;
; Script generated by the ASCOM Driver Installer Script Generator 6.4.0.0
; Generated by robert hasson on 3/14/2019 (UTC)
;
[Setup]
AppID={{77b4d898-3116-40a5-91d4-4c41ff123d93}
AppName=ASCOM ASCOM.Lumix.Camera Camera Driver
AppVerName=ASCOM ASCOM.Lumix.Camera Camera Driver 7.0.5
AppVersion=7.0.5
AppPublisher=robert hasson <robert_hasson@yahoo.com>
AppPublisherURL=mailto:robert_hasson@yahoo.com
AppSupportURL=http://tech.groups.yahoo.com/group/ASCOM-Talk/
AppUpdatesURL=http://ascom-standards.org/
VersionInfoVersion=7.0.5
MinVersion=0,6.0
DefaultDirName="{cf}\ASCOM\Camera"
DisableDirPage=yes
DisableProgramGroupPage=yes
;OutputDir="."
OutputBaseFilename="ASCOM.Lumix.Camera Setup"
Compression=lzma
SolidCompression=yes
; Put there by Platform if Driver Installer Support selected
WizardImageFile="C:\Program Files (x86)\ASCOM\Platform 6 Developer Components\Installer Generator\Resources\WizardImage.bmp"
LicenseFile="C:\Program Files (x86)\ASCOM\Platform 6 Developer Components\Installer Generator\Resources\CreativeCommons.txt"
; {cf}\ASCOM\Uninstall\Camera folder created by Platform, always
UninstallFilesDir="{cf}\ASCOM\Uninstall\Camera\ASCOM.Lumix.Camera"
OutputDir="C:\Users\rober\source\repos\totoantibes\LumixCameraAscomDriver\"

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Dirs]
Name: "{cf}\ASCOM\Uninstall\Camera\ASCOM.Lumix.Camera"
; TODO: Add subfolders below {app} as needed (e.g. Name: "{app}\MyFolder")'
[Files]
Source: "C:\Users\rober\source\repos\totoantibes\LumixCameraAscomDriver\LumixCamera\bin\Release\ASCOM.Lumix.Camera.dll"; DestDir: "{app}"
Source: "C:\Users\rober\source\repos\totoantibes\LumixCameraAscomDriver\LumixCamera\bin\Release\Libraw.dll"; DestDir: "{app}"
Source: "C:\Users\rober\source\repos\totoantibes\LumixCameraAscomDriver\LumixCamera\bin\Release\Libraw32.dll"; DestDir: "{app}"
; Require a read-me HTML to appear after installation, maybe driver's Help doc
Source: "C:\Users\rober\source\repos\totoantibes\LumixCameraAscomDriver\readme.md"; DestDir: "{app}"; Flags: isreadme
; TODO: Add other files needed by your driver here (add subfolders above)
Source: "C:\Users\rober\source\repos\totoantibes\LumixCameraAscomDriver\readme_files\*"; DestDir: "{app}\readme_files\"; 


; Only if driver is .NET
[Run]
; Only for .NET assembly/in-proc drivers
Filename: "{dotnet4032}\regasm.exe"; Parameters: "/codebase ""{app}\ASCOM.Lumix.Camera.dll"""; Flags: runhidden 32bit
Filename: "{dotnet4064}\regasm.exe"; Parameters: "/codebase ""{app}\ASCOM.Lumix.Camera.dll"""; Flags: runhidden 64bit; Check: IsWin64
Filename: "{dotnet4064}\regasm.exe"; Parameters: "/codebase ""{app}\libraw32.dll""";Flags: runhidden 32bit
Filename: "{dotnet4064}\regasm.exe"; Parameters: "/codebase ""{app}\libraw.dll""";Flags: runhidden 64bit; Check: IsWin64


; Only if driver is .NET
[UninstallRun]
; Only for .NET assembly/in-proc drivers
Filename: "{dotnet4032}\regasm.exe"; Parameters: "-u ""{app}\ASCOM.Lumix.Camera.dll"""; Flags: runhidden 32bit
; This helps to give a clean uninstall
Filename: "{dotnet4064}\regasm.exe"; Parameters: "/codebase ""{app}\ASCOM.Lumix.Camera.dll"""; Flags: runhidden 64bit; Check: IsWin64
Filename: "{dotnet4064}\regasm.exe"; Parameters: "-u ""{app}\ASCOM.Lumix.Camera.dll"""; Flags: runhidden 64bit; Check: IsWin64
   

[Code]
const
   REQUIRED_PLATFORM_VERSION = 6.2;    // Set this to the minimum required ASCOM Platform version for this application

//
// Function to return the ASCOM Platform's version number as a double.
//
function PlatformVersion(): Double;
var
   PlatVerString : String;
begin
   Result := 0.0;  // Initialise the return value in case we can't read the registry
   try
      if RegQueryStringValue(HKEY_LOCAL_MACHINE_32, 'Software\ASCOM','PlatformVersion', PlatVerString) then 
      begin // Successfully read the value from the registry
         Result := StrToFloat(PlatVerString); // Create a double from the X.Y Platform version string
      end;
   except                                                                   
      ShowExceptionMessage;
      Result:= -1.0; // Indicate in the return value that an exception was generated
   end;
end;

//
// Before the installer UI appears, verify that the required ASCOM Platform version is installed.
//
function InitializeSetup(): Boolean;
var
   PlatformVersionNumber : double;
 begin
   Result := FALSE;  // Assume failure
   PlatformVersionNumber := PlatformVersion(); // Get the installed Platform version as a double
   If PlatformVersionNumber >= REQUIRED_PLATFORM_VERSION then	// Check whether we have the minimum required Platform or newer
      Result := TRUE
   else
      if PlatformVersionNumber = 0.0 then
         MsgBox('No ASCOM Platform is installed. Please install Platform ' + Format('%3.1f', [REQUIRED_PLATFORM_VERSION]) + ' or later from http://www.ascom-standards.org', mbCriticalError, MB_OK)
      else 
         MsgBox('ASCOM Platform ' + Format('%3.1f', [REQUIRED_PLATFORM_VERSION]) + ' or later is required, but Platform '+ Format('%3.1f', [PlatformVersionNumber]) + ' is installed. Please install the latest Platform before continuing; you will find it at http://www.ascom-standards.org', mbCriticalError, MB_OK);
end;

// Code to enable the installer to uninstall previous versions of itself when a new version is installed
procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  UninstallExe: String;
  UninstallRegistry: String;
begin
  if (CurStep = ssInstall) then // Install step has started
	begin
      // Create the correct registry location name, which is based on the AppId
      UninstallRegistry := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#SetupSetting("AppId")}' + '_is1');
      // Check whether an extry exists
      if RegQueryStringValue(HKLM, UninstallRegistry, 'UninstallString', UninstallExe) then
        begin // Entry exists and previous version is installed so run its uninstaller quietly after informing the user
          MsgBox('Setup will now remove the previous version.', mbInformation, MB_OK);
          Exec(RemoveQuotes(UninstallExe), ' /SILENT', '', SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode);
          sleep(1000);    //Give enough time for the install screen to be repainted before continuing
        end
  end;
end;

