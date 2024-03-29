; Script generated by the HM NIS Edit Script Wizard.

; HM NIS Edit Wizard helper defines
!define PRODUCT_NAME "Fnordbot"
!define PRODUCT_VERSION "1.0"
!define PRODUCT_PUBLISHER "Niels Rask"
!define PRODUCT_WEB_SITE "http://www.nielsrask.dk"
!define PRODUCT_DIR_REGKEY "Software\Microsoft\Windows\CurrentVersion\App Paths\FnordBotService.exe"
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
!define PRODUCT_UNINST_ROOT_KEY "HKLM"

SetCompressor /FINAL /SOLID lzma
SetCompressorDictSize 64

; MUI 1.67 compatible ------
!include "MUI.nsh"

; MUI Settings
!define MUI_ABORTWARNING
!define MUI_ICON "${NSISDIR}\Contrib\Graphics\Icons\modern-install.ico"
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"

; Welcome page
!insertmacro MUI_PAGE_WELCOME
; License page
;!insertmacro MUI_PAGE_LICENSE "..\..\..\..\..\path\to\licence\YourSoftwareLicence.txt"
; Directory page
!insertmacro MUI_PAGE_DIRECTORY
; Instfiles page
!insertmacro MUI_PAGE_INSTFILES
; Finish page
;!define MUI_FINISHPAGE_RUN "$INSTDIR\FnordBotService.exe"
;!insertmacro MUI_PAGE_FINISH

; Uninstaller pages
!insertmacro MUI_UNPAGE_INSTFILES

; Language files
!insertmacro MUI_LANGUAGE "English"

; MUI end ------

!include "servicelib.nsh"

Name "${PRODUCT_NAME} ${PRODUCT_VERSION}"
OutFile "Fnordbot Setup.exe"
InstallDir "$PROGRAMFILES\NielsRask\Fnordbot"
InstallDirRegKey HKLM "${PRODUCT_DIR_REGKEY}" ""
ShowInstDetails show
ShowUnInstDetails show

Section "Base" SEC01
  SetOutPath "$INSTDIR"
  SetOverwrite ifnewer
;  File "FnordBot Service\bin\Debug\log4net.dll"
;  File "FnordBot Service\bin\Debug\log4net.xml"
  File "log4net.dll"
  File "log4net.xml"
  File "FnordBot Service\bin\Debug\FnordBotService.pdb"
  File "FnordBot Service\bin\Debug\FnordBotService.exe.config"
  File "FnordBot Service\bin\Debug\FnordBotService.exe"
  File "LibIrc2\bin\Debug\LibIrc2.pdb"
  File "LibIrc2\bin\Debug\LibIrc2.dll"
  File "LibIrc2\bin\Debug\NielsRask.LibIrc.xml"
  File "FnordBot\bin\Debug\FnordBot.pdb"
  File "FnordBot\bin\Debug\FnordBot.dll"
  File "FnordBot\bin\Debug\NielsRask.Fnordbot.xml"
SectionEnd

Section "Plugins" SEC02
  SetOutPath "$INSTDIR\Plugins\Logger"
  File "Logger\bin\Debug\Logger.dll"
  File "Logger\bin\Debug\Logger.pdb"
  SetOutPath "$INSTDIR\Plugins\SortSnak"
  File "SortSnak\bin\Debug\SortSnak.dll"
  File "SortSnak\bin\Debug\SortSnak.pdb"
  SetOutPath "$INSTDIR\Plugins\Stat"
  File "Stat\bin\Debug\Stat.dll"
  File "Stat\bin\Debug\Stat.pdb"
  SetOutPath "$INSTDIR\Plugins\Wordgame"
  File "Wordgame\bin\Debug\Wordgame.dll"
  File "Wordgame\bin\Debug\Wordgame.pdb"
  File "Wordgame\wordlist.dat"
  SetOutPath "$INSTDIR\Plugins\Voter"
  File "Voter\bin\Debug\Voter.dll"
  File "Voter\bin\Debug\Voter.pdb"
SectionEnd

Section -Post
  WriteUninstaller "$INSTDIR\uninst.exe"
  !insertmacro SERVICE "create" "FnordBot" "path=$INSTDIR\FnordBotService.exe;autostart=1;interact=0;display=FnordBot Service;"
  ;WriteRegStr HKLM "${PRODUCT_DIR_REGKEY}" "" "$INSTDIR\FnordBotService.exe"
  WriteRegStr HKLM "Software\NielsRask\FnordBot" "InstallationFolderPath" "$INSTDIR"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayName" "$(^Name)"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninst.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayIcon" "$INSTDIR\FnordBotService.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
  IfFileExists "$INSTDIR\Config.xml" fin 0
  ;IfFileExists "$INSTDIR\Config.xml" 0 0
  FileOpen $9 "$INSTDIR\Config.xml" w ;Opens a Empty File an fills it 
  FileWrite $9 "<?xml version=$\"1.0$\" encoding=$\"iso-8859-1$\" ?> $\r$\n"
  FileWrite $9 "<config>$\r$\n"
  FileWrite $9 "	<client>$\r$\n"
  FileWrite $9 "		<server port=$\"6667$\">10.0.0.101</server>$\r$\n"
  FileWrite $9 "		<nickname>BimseBot</nickname>$\r$\n"
  FileWrite $9 "		<altnick>BimmerBot</altnick>$\r$\n"
  FileWrite $9 "		<realname>B. Imse</realname>$\r$\n"
  FileWrite $9 "		<username>bimmerfooo</username>$\r$\n"
  FileWrite $9 "		<channels>$\r$\n"
  FileWrite $9 "			<channel>$\r$\n"
  FileWrite $9 "				<name>#craYon</name>$\r$\n"
  FileWrite $9 "				<messagerate messages=$\"5$\" minutes=$\"15$\"/>$\r$\n"
  FileWrite $9 "			</channel>$\r$\n"
  FileWrite $9 "		</channels>$\r$\n"
  FileWrite $9 "	</client>$\r$\n"
  FileWrite $9 "	<plugins>$\r$\n"
  FileWrite $9 "		<plugin typename=$\"NielsRask.SortSnak.Plugin$\" path=$\"plugins\sortsnak\sortsnak.dll$\" >$\r$\n"
  FileWrite $9 "			<settings>$\r$\n"
  FileWrite $9 "				<vocabularyfilepath>plugins\sortsnak\vocabulary.dat</vocabularyfilepath>$\r$\n"
  FileWrite $9 "				<answerchance>15</answerchance>$\r$\n"
  FileWrite $9 "				<minimumoverlap>3</minimumoverlap>$\r$\n"
  FileWrite $9 "				<simplechance>35</simplechance>$\r$\n"
  FileWrite $9 "				<ambientsimplechance>10</ambientsimplechance>$\r$\n"
  FileWrite $9 "				<autosaving>5</autosaving>$\r$\n"
  FileWrite $9 "			</settings>$\r$\n"
  FileWrite $9 "			<permissions>$\r$\n"
  FileWrite $9 "				<permission name=$\"CanOverrideSendToChannel$\" value=$\"False$\" />$\r$\n"
  FileWrite $9 "			</permissions>$\r$\n"
  FileWrite $9 "		</plugin> $\r$\n"
  FileWrite $9 "		<plugin typename=$\"NielsRask.Wordgame.Plugin$\" path=$\"plugins\wordgame\wordgame.dll$\" >$\r$\n"
  FileWrite $9 "			<settings>$\r$\n"
  FileWrite $9 "				<wordlist>plugins\wordgame\wordlist.dat</wordlist>$\r$\n"
  FileWrite $9 "			</settings>$\r$\n"
  FileWrite $9 "			<permissions>$\r$\n"
  FileWrite $9 "				<permission name=$\"CanOverrideSendToChannel$\" value=$\"True$\" />$\r$\n"
  FileWrite $9 "			</permissions>$\r$\n"
  FileWrite $9 "		</plugin>$\r$\n"
  FileWrite $9 "		<plugin typename=$\"NielsRask.Logger.Plugin$\" path=$\"plugins\logger\logger.dll$\" >$\r$\n"
  FileWrite $9 "			<settings>$\r$\n"
  FileWrite $9 "				<logfolderpath>plugins\logger\logs</logfolderpath>$\r$\n"
  FileWrite $9 "			</settings>$\r$\n"
  FileWrite $9 "			<permissions>$\r$\n"
  FileWrite $9 "				<permission name=$\"CanOverrideSendToChannel$\" value=$\"False$\" />$\r$\n"
  FileWrite $9 "			</permissions>$\r$\n"
  FileWrite $9 "		</plugin>$\r$\n"
  FileWrite $9 "		<plugin typename=$\"NielsRask.Stat.StatPlugin$\" path=$\"plugins\stat\stat.dll$\" >$\r$\n"
  FileWrite $9 "			<settings />$\r$\n"
  FileWrite $9 "			<permissions>$\r$\n"
  FileWrite $9 "				<permission name=$\"CanOverrideSendToChannel$\" value=$\"True$\" />$\r$\n"
  FileWrite $9 "			</permissions>$\r$\n"
  FileWrite $9 "		</plugin>$\r$\n"
  FileWrite $9 "		<plugin typename=$\"NielsRask.Voter.Voter$\" path=$\"plugins\Voter\Voter.dll$\" >$\r$\n"
  FileWrite $9 "			<settings />$\r$\n"
  FileWrite $9 "			<permissions>$\r$\n"
  FileWrite $9 "				<permission name=$\"CanOverrideSendToChannel$\" value=$\"True$\" />$\r$\n"
  FileWrite $9 "			</permissions>$\r$\n"
  FileWrite $9 "		</plugin>$\r$\n"
  FileWrite $9 "	</plugins>$\r$\n"
  FileWrite $9 "</config>$\r$\n"
  FileClose $9 ;Closes the filled file
  fin:
SectionEnd


Function un.onUninstSuccess
  HideWindow
  MessageBox MB_ICONINFORMATION|MB_OK "$(^Name) was successfully removed from your computer."
FunctionEnd

Function un.onInit
  MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 "Are you sure you want to completely remove $(^Name) and all of its components?" IDYES +2
  Abort
FunctionEnd

Section Uninstall
  !undef UN
  !define UN "un."
  ;!insertmacro SERVICE "stop" "FnordBot" ""
  !insertmacro SERVICE "delete" "FnordBot" ""
  Delete "$INSTDIR\uninst.exe"
  Delete "$INSTDIR\Plugins\Wordgame\wordlist.dat"
  Delete "$INSTDIR\Plugins\Wordgame\Wordgame.pdb"
  Delete "$INSTDIR\Plugins\Wordgame\Wordgame.dll"
  Delete "$INSTDIR\Plugins\Stat\Stat.pdb"
  Delete "$INSTDIR\Plugins\Stat\Stat.dll"
  Delete "$INSTDIR\Plugins\SortSnak\SortSnak.pdb"
  Delete "$INSTDIR\Plugins\SortSnak\SortSnak.dll"
  Delete "$INSTDIR\Plugins\Logger\Logger.pdb"
  Delete "$INSTDIR\Plugins\Logger\Logger.dll" 
  Delete "$INSTDIR\Plugins\Voter\Voter.pdb"
  Delete "$INSTDIR\Plugins\Voter\Voter.dll"
  Delete "$INSTDIR\NielsRask.Fnordbot.xml"
  Delete "$INSTDIR\FnordBot.dll"
  Delete "$INSTDIR\FnordBot.pdb"
  Delete "$INSTDIR\NielsRask.LibIrc.xml"
  Delete "$INSTDIR\LibIrc2.dll"
  Delete "$INSTDIR\LibIrc2.pdb"
  Delete "$INSTDIR\log4net.xml"
  Delete "$INSTDIR\FnordBotService.exe"
  Delete "$INSTDIR\FnordBotService.exe.config"
  Delete "$INSTDIR\FnordBotService.pdb"
  Delete "$INSTDIR\log4net.dll"

  RMDir "$INSTDIR\Plugins\Wordgame"
  RMDir "$INSTDIR\Plugins\Stat"
  RMDir "$INSTDIR\Plugins\SortSnak"
  RMDir "$INSTDIR\Plugins\Logger"
  RMDir "$INSTDIR\Plugins\Voter"
  RMDir "$INSTDIR"

  DeleteRegKey ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}"
  DeleteRegKey HKLM "${PRODUCT_DIR_REGKEY}"
  DeleteRegKey HKLM "Software/NielsRask"
  SetAutoClose true
SectionEnd