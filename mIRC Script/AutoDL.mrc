;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;;;;;;;;;;;;;;;;Start of Auto-DL;;;;;;;;;;;;;;;;;;
;;;;;;;;;;;;;;;;;;;;;;v1.5.0;;;;;;;;;;;;;;;;;;;;;;;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;--------------------------------------------------
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;;;;;;;;;;;;;;;;;;Core Scripts;;;;;;;;;;;;;;;;;;;;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

;;;
;;;Causes mIRC to load the dll for the first time
;;;and setup some internals.
;;;
alias /AutoDLOpen {
  set -e %DLLName " $+ $nofile($mircexe) $+ AutoDL\Service\mIRCWrapper.dll $+ "
  dll %DLLName AutoDL_Start
}

;;;
;;;Cleans up.
;;;
alias /AutoDLClose {
  dll -u %DLLName
  unset %DLLName
}

;;;
;;;Start Download (called externally only)
;;;
alias /AutoDL_StartDL {
  .dcc trust $$1 $+ !*@*
  .auser AutoDLTrustedSources $$1
  msg $$1 xdcc send $$2
}

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;;;;;;;;;;;;;;;;;;Event Triggers;;;;;;;;;;;;;;;;;;;;;;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

on AutoDLTrustedSources:FILERCVD:*.*:{
  .dcc trust -r $nick $+ !*@*
  .ruser AutoDLTrustedSources $nick
  dll %DLLName RequestNextDownload true
}

on AutoDLTrustedSources:GETFAIL:*.*:{
  msg $nick xdcc cancel
  .dcc trust -r $nick $+ !*@*
  .ruser AutoDLTrustedSources $nick
  dll %DLLName RequestNextDownload false
}

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;;;;;;;;;;;;;;;;;;;;;GUI Tables;;;;;;;;;;;;;;;;;;;;;;;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

dialog -l AutoDLTable {
  title "Auto-Downloader"
  size -1 -1 200 180
  option dbu

  menu "&File", 50
  menu "&Settings", 51, 50
  item "Notifications", 101, 51
  item "Load On Startup", 104, 51
  item break, 63, 51
  item "Retry Failed Download", 102, 51
  item "Download Delay", 103, 51
  menu "&Aliases", 56, 50
  item "Add Alias...", 55, 56
  item "View/Remove Alias...", 57, 56
  item "Clear All", 58, 56
  item break, 52, 50
  item "&Save", 59, 50
  item "&Load", 62, 50
  item break, 61, 50
  item "&Exit", 53, 50
  menu "&Help", 60
  item "&About", 54, 60

  box "Download Controls", 7, 110 35 85 70
  text "Bot:", 8, 115 45 10 10
  edit "", 9, 128 45 60 10, autohs
  text "Packet(s):", 10, 115 60 25 10
  edit "", 11, 143 60 45 10, autohs
  button "Add", 12, 168 72 20 10
  button "Start Download", 19, 115 85 75 15

  box "Download Queue", 1, 5 5 100 165
  icon 17, 90 10 10 10, " $+ $nofile($mircexe) $+ AutoDL\Images\Delete.png $+ ", small
  list 2, 10 20 90 120, hsbar vsbar multsel
  list 18, 10 135 90 27, vsbar
  button "Clear Status", 20, 60 157 40 10

  box "Queue Controls", 13, 110 110 85 48 
  button "Clear", 3, 115 120 30 34
  button "Save", 14, 150 120 35 10
  button "Load", 15, 150 132 35 10
  button "Clear Saved", 21, 150 144 35 10

  box "Join Channel", 4, 110 5 85 22
  edit "", 5, 115 13 58 10, autohs
  button "Join", 6, 175 13 15 10

  button "Exit", 16, 160 160 30 15, ok
}

dialog -l HelpDLTable {
  title "Auto-Download Help"
  size -1 -1 150 20
  option dbu

  text "See Wiki @", 1, 3 3 30 10
  link "http://github.com/Rinsho/mIRCAutoDL/wiki", 2, 35 3 110 10
}

dialog -l AddAliasTable {
  title "Add Alias"
  size -1 -1 80 45
  option dbu

  text "Bot:", 1, 3 4 10 10
  edit "", 2, 15 3 60 10, autohs %Bot
  text "Alias:", 3, 3 16 20 10
  edit "", 4, 30 15 45 10, autohs %Alias

  button "Ok", 5, 28 30 20 10, ok result
  button "Cancel", 6, 50 30 25 10, cancel
}

dialog -l DeleteAliasTable {
  title "Delete Alias"
  size -1 -1 100 80
  option dbu

  box "Alias : Bot", 1, 5 3 90 60
  list 2, 10 12 80 50, vsbar hsbar

  button "Ok", 3, 48 65 20 10, ok result
  button "Cancel", 4, 70 65 25 10, cancel
}

dialog -l DownloadDelayTable {
  title "Download Delay"
  size -1 -1 85 35
  option dbu

  text "Delay (seconds):", 1, 5 5 50 10
  edit "", 2, 60 3 20 10, result

  button "Ok", 3, 38 20 20 10, ok
  button "Cancel", 4, 60 20 20 10, cancel
}

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;;;;;;;;;;;;;;;;;;;GUI Scripts;;;;;;;;;;;;;;;;;;;;;;;;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

;;;
;;;Used to start the GUI.
;;;
alias /AutoDLGUI {
  if ($1 == DesktopMode) {
    dialog -md AutoDL AutoDLTable
  }
  else {
    dialog -m AutoDL AutoDLTable
  }
}

;;;
;;;Used to update the UI download list
;;;
alias -l /AutoDL_UI {
  if ($$1 == Add) {
    var %Bot = $$2
    var %Packet = $$3
    did -az AutoDL 2 $+(%Bot, $chr(32), $chr(35), %Packet)
    hadd UIDownloadList $did(AutoDL, 2).lines $+(%Bot, $chr(44), %Packet)   
  }
  elseif ($$1 == Status) {
    if ($$3 == Cancelled || $$3 == Saved) {
      hdel UIDownloadList $$2
    }
    elseif ($$3 == Downloading) {
      %ActiveDL = $$2
    }
    did -oz AutoDL 2 $$2 $+($did(AutoDL, 2, $$2).text, $chr(32), $+(..., $$3))
  }
  elseif ($$1 == Update) {
    var %text = $did(AutoDL, 2, %ActiveDL).text
    var %leftpos = $pos(%text, ., 1)
    var %replacetext = $right(%text, $calc((%leftpos - 1) - (2 * (%leftpos - 1))))
    did -oz AutoDL 2 %ActiveDL $replace(%text, %replacetext, $+(..., $$2))
  }
  elseif ($$1 == Clear) {
    hdel -w UIDownloadList *
    %ActiveDL = 0
    did -r AutoDL 2
  }
}

;;;
;;;/AutoDL_Notification <message>
;;;
alias -l /AutoDL_Notification {
  did -az AutoDL 18 $$1-
  if ($did(AutoDL, 101).state == 1) {
    echo 2 -s $$1-
  } 
}

;;;
;;;Used by wrapper to update GUI on next download
;;;
alias /AutoDL_Downloading {
  var %Counter = 1
  var %MaxCounter = $hget(UIDownloadList, 0).item
  while (%Counter <= %MaxCounter) {
    var %Download = $hget(UIDownloadList, %Counter).data
    var %Bot = $gettok(%Download, 1, 44)
    var %Packet = $gettok(%Download, 2, 44)
    if ((%Bot == $$1) && (%Packet == $$2)) {
      /AutoDL_UI Status $hget(UIDownloadList, %Counter).item Downloading
      break
    }
    inc %Counter 1
  }
}

;;;
;;;Used by wrapper to update GUI on current download
;;;
alias /AutoDL_StatusUpdate {
  var %status
  if (($$1 == Complete) || ($$1 == Failed)) {
    %status = $$1
    hdel UIDownloadList %ActiveDL
  }
  elseif ($$1 == Retrying) {
    %status = $$1
  }
  /AutoDL_UI Update %status
  if ($hget(UIDownloadList, 0).item == 0) {
    %ActiveDL = 0
    /AutoDL_Notification Queue Complete.
  }
}

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;;;;;;;;;;;;;;;;;;;;GUI Events;;;;;;;;;;;;;;;;;;;;;;;;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

;;;
;;;Initialize AutoDL Event
;;;
on *:dialog:AutoDL:init:0:{
  set -e %UIDLLName " $+ $nofile($mircexe) $+ AutoDL\Client\mIRCClient.dll $+ "
  set -e %DownloadDelay 5
  set -e %ActiveDL 0
  hmake UIDownloadList 10
  hmake AliasList 10
  hmake LocalSettings 2

  var %SettingsFile = " $+ $nofile($mircexe) $+ AutoDL\LocalSettings.htb"
  if ($exists(%SettingsFile)) {
    hload LocalSettings %SettingsFile
    if ($hget(LocalSettings, 1).item == Notifications && $hget(LocalSettings, Notifications) == True) {
      did -c AutoDL 101
    }
    else if ($hget(LocalSettings, 1).data == True) {
      did -c AutoDL 104
    }
    if ($hget(LocalSettings, 2).item == StartupLoad && $hget(LocalSettings, StartupLoad) == True) {
      did -c AutoDL 104
    }
    else if ($hget(LocalSettings, 2).data == True) {
      did -c AutoDL 101
    }
  }

  var %loadedDlls = $dll(0)
  if (%loadedDlls == 0) {
    /AutoDLOpen
  }
  else {
    var %found = $false
    while (%loadedDlls > 0) {
      if ($nopath($dll(%loadedDlls)) == mIRCWrapper.dll) {
        %found = $true
        break
      }
      dec %loadedDlls 1
    }
    if (%found == $false) {
      /AutoDLOpen
    }
  }

  if ($did(AutoDL, 104).state == 1) {
    /LoadAll
  }
}

;;;
;;;Close AutoDL Event
;;;
on *:dialog:AutoDL:close:0:{
  dll -u %UIDLLName
  /AutoDLClose
  hfree UIDownloadList
  hfree AliasList
  hfree LocalSettings
  unset %ActiveDL
  unset %DownloadDelay
  unset %UIDLLName
  .rlevel -r AutoDLTrustedSources
}

;;;
;;;Download Delay Event
;;;
on *:dialog:DownloadDelay:init:0:{
  did -a DownloadDelay 2 %DownloadDelay
}

;;;
;;;Menu->Aliases->View/Remove Alias Event
;;;
on *:dialog:AutoDL:menu:57:{
  if ($dialog(DeleteAlias, DeleteAliasTable) && (%DeleteSelection != null)) {
    %DeleteSelection = $remove(%DeleteSelection, $chr(32))
    var %return = $dll(%UIDLLName, Alias_Remove, %DeleteSelection)   
    if (%return == #OK) {
      hdel AliasList %DeleteSelection
    }
    else {
      tokenize 44 %result
      /AutoDL_Notification $+($1, $chr(58), $chr(32), $2)
    }
  }
  unset %DeleteSelection
}

;;;
;;;DeleteAlias Initialize Event
;;;
on *:dialog:DeleteAlias:init:0:{
  var %ListSize = $hget(AliasList, 0).item
  if (%ListSize > 0) {
    var %Bot
    var %Alias
    var %Counter = 1
    var %MaxCounter = %ListSize
    while (%Counter <= %MaxCounter) {
      %Alias = $hget(AliasList, %Counter).item
      %Bot = $hget(AliasList, %Alias)
      did -az DeleteAlias 2 $+(%Alias,$chr(32),$chr(58),$chr(32),%Bot)
      inc %Counter 1
    }
  }
  set -e %DeleteSelection null
}

;;;
;;;DeleteAlias Selection Event
;;;
on *:dialog:DeleteAlias:sclick:2:{
  %DeleteSelection = $gettok($did(DeleteAlias, 2).seltext, 1, 58)
}

;;;
;;;Clear Queue Event
;;;
on *:dialog:AutoDL:sclick:3:{
  var %result = $dll(%UIDLLName, Download_Clear, None)
  if (%result == #OK) {  
    /AutoDL_UI Clear
  }
  else {
    tokenize 44 %result
    /AutoDL_Notification $+($1, $chr(58), $chr(32), $2)
  }
}

;;;
;;;Save Queue Event
;;;
on *:dialog:AutoDL:sclick:14:{
  var %result = $dll(%UIDLLName, Download_Save, None)
  if (%result == #OK) {
    var %Counter = 1
    var %MaxCounter = $hget(UIDownloadList, 0).item
    while (%Counter <= %MaxCounter) {
      /AutoDL_UI Status $hget(UIDownloadList, 1).item Saved
      inc %Counter 1
    }
  }
  else {
    tokenize 44 %result
    /AutoDL_Notification $+($1, $chr(58), $chr(32), $2)
  }
}

;;;
;;;Load Queue Event
;;;
on *:dialog:AutoDL:sclick:15:{
  var %result = $dll(%UIDLLName, Download_Load, None)
  if ($gettok(%result, 1, 44) != #Error) {
    var %Counter = 1
    var %MaxCounter = $numtok(%result, 44)   
    tokenize 44 %result
    while (%Counter <= %MaxCounter) {
      var %Bot = $ [ $+ [ %Counter ] ]
      var %Packet = $ [ $+ [ $calc(%Counter + 1) ] ] 
      /AutoDL_UI Add %Bot %Packet
      inc %Counter 2
    }
  }
  else {
    tokenize 44 %result
    /AutoDL_Notification $+($1, $chr(58), $chr(32), $2)
  }
}
;;;
;;;Start Download Event
;;;
on *:dialog:AutoDL:sclick:19:{
  if ($hget(UIDownloadList, 0).item > 0) {
    var %result = $dll(%UIDLLName, Download_StartDownload, None)
    if (%result == #OK) {
      /AutoDL_Notification Starting Download...
    }
    else {
      tokenize 44 %result
      /AutoDL_Notification $+($1, $chr(58), $chr(32), $2)
    }
  }
  else {
    /AutoDL_Notification *Error: Cannot start an empty queue.
  }
}

;;;
;;;Add Download Event
;;;
on *:dialog:AutoDL:sclick:12:{
  var %BotName = $did(9)
  var %FileList = $did(11)
  var %Trimming = $true
  while (%Trimming) {
    if ($right(%FileList, 1) == $chr(32) || $right(%FileList, 1) == $chr(44)) {
      %FileList = $left(%FileList, -1)
    }
    else {
      %Trimming = $false
    }
    if ($left(%FileList, 1) == $chr(32) || $left(%FileList, 1) == $chr(44)) {
      %FileList = $right(%FileList, -1)
      if (%Trimming == $false) {
        %Trimming = $true
      }
    }
  }
  %FileList = $regsubex(%FileList, $+(/[\s,$chr(44),]+/g), $chr(44))

  var %Counter = 1
  var %MaxCounter = $numtok(%FileList, 44)
  tokenize 44 %FileList
  %FileList = $null
  while (%Counter <= %MaxCounter) {
    var %Token = $ [ $+ [ %Counter ] ]
    if ($chr(45) isin %Token) {
      var %From = $gettok(%Token, 1, 45)
      var %To = $gettok(%Token, 2, 45)
      while (%From <= %To) {
        %FileList = $+(%FileList, %From, $chr(44))
        inc %From 1
      }
    }
    else {
      %FileList = $+(%FileList, %Token, $chr(44))
    }
    inc %Counter 1
  }
  %FileList = $left(%FileList, -1)

  %Trimming = $true
  while (%Trimming) {
    if ($right(%BotName, 1) == $chr(32)) {
      %BotName = $left(%BotName, -1)
    }
    else {
      %Trimming = $false
    }
    if ($left(%BotName, 1) == $chr(32)) {
      %BotName = $right(%BotName, -1)
      if (%Trimming == $false) {
        %Trimming = $true
      }
    }
  }

  var %Counter = 1
  var %MaxCounter = $numtok(%FileList, 44)
  var %DownloadList = $+(%BotName, $chr(44), $gettok(%FileList, %Counter, 44), $chr(44))
  inc %Counter 1
  while (%Counter <= %MaxCounter) {
    %DownloadList = $+(%DownloadList, %BotName, $chr(44), $gettok(%FileList, %Counter, 44), $chr(44))
    inc %Counter 1
  }
  %DownloadList = $left(%DownloadList, -1)

  var %result = $dll(%UIDLLName, Download_Add, %DownloadList)
  if (%result == #OK) {
    var %Counter = 1
    var %MaxCounter = $numtok(%FileList, 44)
    tokenize 44 %FileList
    while (%Counter <= %MaxCounter) {
      /AutoDL_UI Add %BotName $ [ $+ [ %Counter ] ]
      inc %Counter 1
    }
    did -r AutoDL 9
    did -r AutoDL 11
  }
  else {
    tokenize 44 %result
    /AutoDL_Notification $+($1, $chr(58), $chr(32), $2)
  }
}

;;;
;;;Remove Download Event
;;;
on *:dialog:AutoDL:sclick:17:{
  var %Counter = 1
  var %MaxCounter = $did(AutoDL, 2, 0).sel
  var %Lines
  var %Downloads
  while (%Counter <= %MaxCounter) {
    var %LineNumber = $did(AutoDL, 2, %Counter).sel   
    inc %Counter 1
    if (%LineNumber != %ActiveDL) {
      %Lines = $+(%Lines, %LineNumber, $chr(44))
      var %Data = $did(AutoDL, 2, %LineNumber).text
      var %Bot = $gettok(%Data, 1, 32)
      var %Packet = $gettok(%Data, 2, 32)
      %Packet = $right(%Packet, $calc($len(%Packet) - 1))
      %Downloads = $+(%Downloads, %Bot, $chr(44), %Packet, $chr(44))     
    }
    else {
      /AutoDL_Notification *Cannot remove download in progress.
    }
  }
  %Lines = $left(%Lines, -1)
  %Downloads = $left(%Downloads, -1)
  var %result = $dll(%UIDLLName, Download_Remove, %Downloads)
  if (%result == #OK) {
    var %Counter = 1
    var %MaxCounter = $numtok(%Lines, 44)
    tokenize 44 %Lines
    while (%Counter <= %MaxCounter) {
      var %Line = $ [ $+ [ %Counter ] ]
      /AutoDL_UI Status %Line Cancelled
      inc %Counter 1
    } 
  }
  else {
    tokenize 44 %result
    /AutoDL_Notification $+($1, $chr(58), $chr(32), $2)
  }
}

;;;
;;;Menu->Settings->Notifications Event
;;;
on *:dialog:AutoDL:menu:101:{
  if ($did(101).state == 1) {
    did -u AutoDL 101
    hadd LocalSettings Notifications False
  }
  else {
    did -c AutoDL 101
    hadd LocalSettings Notifications True
  }
}

;;;
;;;Menu->Settings->Retry Failed Download Event
;;;
on *:dialog:AutoDL:menu:102:{
  var %tempValue
  if ($did(102).state == 1) {
    did -u AutoDL 102
    %tempValue = false
  }
  else {
    did -c AutoDL 102
    %tempValue = true
  }
  var %result = $dll(%UIDLLName, Setting_Update, $+(RetryFailedDownload, $chr(44), %tempValue))
  if (%result != #OK) {
    if ($did(102).state == 1) {
      did -u AutoDL 102
    }
    else {
      did -c AutoDL 102
    }
    tokenize 44 %result
    /AutoDL_Notification $+($1, $chr(58), $chr(32), $2)
  }
}

;;;
;;;Menu->Settings->Download Delay Event
;;;
on *:dialog:AutoDL:menu:103:{
  var %newDelay = $dialog(DownloadDelay, DownloadDelayTable)
  if (%newDelay isnum) {
    var %result = $dll(%UIDLLName, Setting_Update, $+(DownloadDelay, $chr(44), %newDelay))
    if (%result == #OK) {
      %DownloadDelay = %newDelay
    }
    else {
      tokenize 44 %result
      /AutoDL_Notification $+($1, $chr(58), $chr(32), $2)
    }
  }
}

;;;
;;;Menu->Exit Event
;;;
on *:dialog:AutoDL:menu:53:{
  dialog -k AutoDL
}

;;;
;;;Menu->Help Event
;;;
on *:dialog:AutoDL:menu:54:{
  dialog -m HelpDL HelpDLTable
}

;;;
;;;Menu->Aliases->Add Alias Event
;;;
on *:dialog:AutoDL:menu:55:{
  if ($dialog(AddAlias, AddAliasTable)) {
    var %result = $dll(%UIDLLName, Alias_Add, $+(%Alias, $chr(44), %Bot))
    if (%result == #OK) {
      hadd AliasList %Alias %Bot
    }
    else {
      tokenize 44 %result
      /AutoDL_Notification $+($1, $chr(58), $chr(32), $2)
    }
    unset %Bot
    unset %Alias
  }
}

;;;
;;;Menu->Aliases->Clear Event
;;;
on *:dialog:AutoDL:menu:58:{
  var %result = $dll(%UIDLLName, Alias_Clear, None)
  if (%result == #OK) {
    hdel -w AliasList *
    /AutoDL_Notification Aliases Cleared.
  }
  else {
    tokenize 44 %result
    /AutoDL_Notification $+($1, $chr(58), $chr(32), $2)
  }
}

;;;
;;;Menu->Save Event
;;;
on *:dialog:AutoDL:menu:59:{
  var %result = $dll(%UIDLLName, Setting_Save, None)
  if (%result == #OK) {
    /AutoDL_Notification Settings saved.
  }
  else {
    tokenize 44 %result
    /AutoDL_Notification $+($1, $chr(58), $chr(32), $2)
  }

  var %result = $dll(%UIDLLName, Alias_Save, None)
  if (%result == #OK) {
    /AutoDL_Notification Aliases saved.
  }
  else {
    tokenize 44 %result
    /AutoDL_Notification $+($1, $chr(58), $chr(32), $2)
  }

  hsave LocalSettings " $+ $nofile($mircexe) $+ AutoDL\LocalSettings.htb"
}

;;;
;;;Load Function
;;;
alias -l /LoadAll {
  var %result = $dll(%UIDLLName, Setting_Load, None)
  if ($gettok(%result, 1, 44) != #Error) {
    var %Counter = 1
    var %MaxCounter = $numtok(%result, 44)
    tokenize 44 %result
    while (%Counter <= %MaxCounter) {
      var %setting = $ [ $+ [ %Counter ] ]
      var %value = $ [ $+ [ $calc(%Counter + 1) ] ]
      if (%setting == RetryFailedDownload) {
        if (%value == True) {
          did -c AutoDL 102
        }
        else {
          did -u AutoDL 102
        }
      }
      if (%setting == DownloadDelay) {
        %DownloadDelay = %value
      }
      inc %Counter 2
    }
  }
  else {
    tokenize 44 %result
    /AutoDL_Notification $+($1, $chr(58), $chr(32), $2)
  }

  var %result = $dll(%UIDLLName, Alias_Load, None)
  if ($gettok(%result, 1, 44) != #Error) {
    var %Counter = 1
    var %MaxCounter = $numtok(%result, 44)
    tokenize 44 %result
    while (%Counter <= %MaxCounter) {
      var %Alias = $ [ $+ [ %Counter ] ]
      var %Name = $ [ $+ [ $calc(%Counter + 1) ] ]
      hadd AliasList %Alias %Name
      inc %Counter 2
    }
  }
  else {
    tokenize 44 %result
    /AutoDL_Notification $+($1, $chr(58), $chr(32), $2)
  }
}

;;;
;;;Clear Status Event
;;;
on *:dialog:AutoDL:sclick:20:{
  /did -r AutoDL 18
}

;;;
;;;Clear Saved Queue Event
;;;
on *:dialog:AutoDL:sclick:21:{
  var %result = $dll(%UIDLLName, Download_ClearSaved, None)
  if (%result == #OK) {
    /AutoDL_Notification Saved Queue cleared.
  }
  else {
    tokenize 44 %result
    /AutoDL_Notification $+($1, $chr(58), $chr(32), $2)
  }
}

;;;
;;;Load on Startup Event
;;;
on *:dialog:AutoDL:menu:104:{
  if ($did(104).state == 1) {
    did -u AutoDL 104
    hadd LocalSettings StartupLoad False
  }
  else {
    did -c AutoDL 104
    hadd LocalSettings StartupLoad True
  }
}

;;;
;;;Menu->Load Event
;;;
on *:dialog:AutoDL:menu:62:{
  /LoadAll
}

;;;
;;;Join Channel Event
;;;
on *:dialog:AutoDL:sclick:6:{
  var %ChannelName = $did(5)
  if ($left(%ChannelName,1) != $chr(35)) {
    %ChannelName = $chr(35) $+ %ChannelName
  }
  join %ChannelName
  did -r AutoDL 5
}

;------------------------------------------------------
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;;;;;;;;;;;;;;;;;End of Auto-DL BETA;;;;;;;;;;;;;;;;;;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
