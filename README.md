**Update:** In a stable state (been using it for 4 years) so I'm leaving this as-is indefinitely.

Welcome to the mIRC AutoDL program wiki!  This program is confirmed working with mIRC v7.32 and I've seen no documented scripting engine changes to indicate it will not work with up to mIRC v7.43.  Enjoy!

**Requirements:** mIRC v7.32 or later, .NET Framework 4.5

[I. Introduction](https://github.com/Rinsho/mIRCAutoDL/wiki#introduction)  
[II. Setup](https://github.com/Rinsho/mIRCAutoDL/wiki#setup)  
[III. Usage](https://github.com/Rinsho/mIRCAutoDL/wiki#usage)  
[IV. Known Issues](https://github.com/Rinsho/mIRCAutoDL/wiki#known-issues)  
[V. Future Development](https://github.com/Rinsho/mIRCAutoDL/wiki#future-development)  
[VI. Updates](https://github.com/Rinsho/mIRCAutoDL/wiki#updates)  

---

###**Introduction**
AutoDL is a project I conceived to deal with some limitations of the XDCC protocol for the downloader (not the bot/server):  
- You can only request one file at a time.  Many XDCC bots have introduced small queues to help with this issue, but this leads to the following problems.  
- If you wish to download multiple files, you must either A) manually accept each download, or B) open up your trusted list for however long you are AFK even if the downloads have completed.  
- If utilizing a queue, changing what is queued (removing for instance) requires dumping what you have queued and re-queueing.  It is not a fluid process if multiple files are queued.  
- If you need to interrupt your downloads or quit IRC there is no way to save what you have previously queued.  You have to write down or remember names and packet numbers for the next time you download.  
- Many casual users of IRC don't remember the syntax for the XDCC protocol.  

My program handles all these issues and adds some quality-of-life options for the user.

###**Setup**  
Setup is even simpler in the new version!  Simply double-click `Installer.bat` and follow the instructions.  If you do not wish to use the `mIRCOptionsEditor` during installation, you can manually setup your `mIRC` options like this:

![DCC Options](http://i.imgur.com/RJzpDF6.jpg)

1. Go to `Options`  
2. Choose `DCC`  
3. Select `Auto-get file` in the `On Send request` section on the right  
4. Set `If file exists:` to either `Overwrite` or `Resume`  
4. Click the `Trusted` button  
5. Check both `Limit auto-get to trusted users` and `Show get dialog for non-trusted users`  

This will allow you to AFK and let my program handle the trusted users list as it goes through the downloads only allowing auto-get for a specific download while that download is running.


###**Usage**

![Usage Pic](http://i.imgur.com/yJg8i0N.jpg)

1. **Join:** Join an IRC channel.  Supports both `\#ChannelName` and `ChannelName` formats.
2. **Add:** Adds a bot and packet(s) to the queue.
3. **Start Download:** Starts the download queue.  Does nothing when a queue is currently running.
4. **Clear:** Removes all downloads from the queue and clears the `Download Queue` window.
5. **Save:** Saves all downloads in the queue.  This also removes the downloads from the queue.
6. **Load:** Loads previously saved downloads in the current queue.
7. **Clear Saved:** Removes all downloads from the save file.
8. **Cancel Downloads:** Cancels all downloads selected in the `Download Queue` window.  A download in progress cannot be cancelled.
9. **Clear Status:** Clears the `Notifications` window.
10. **Notifications:** Toggle on to also send notifications to the mIRC status window.
11. **Load On Startup:** Toggle on to load settings and aliases automatically when the program starts.
12. **Retry Failed Download:** Toggle on to re-download failed downloads.  Toggle off to skip failed downloads.
13. **Download Delay:** Set the delay between downloads in seconds.  Default: 5
14. **Add Alias:** Add an alias for a bot's name.  Example: Instead of having to enter `ThisIsAReallyLongBotName`, you could create and use an alias such as `Bot1`.
15. **View/Remove Alias:** View currently active aliases.  Select an alias and click `Ok` to delete.  Note: This will not delete it from the save file.
16. **Clear All:** Deletes all active aliases.  Note: This will not delete it from the save file.
17. **Save:** Saves current settings and aliases to the save file.
18. **Load:** Loads settings and aliases from the save file.


###**Known Issues**
- If the program closes while a download is running, the event to remove the current user from your trusted list
will not work.  In the event this happens, you can manually remove the user by going to `Options`>`DCC`>`Trusted` button.


###**Future Development**
- External GUI (WPF) with a LOT more functionality and customization
- GUI installer (Wix#) for easier installation
- Handling re-direct bots (bots that re-direct download requests to other bots)
- Ability to run multiple queues in parallel
- Allowing queue optimization via a shallow-learning algorithm (max 3 layers)
- Whatever else I think up that would be useful

Note: Only the WPF GUI and Wix# installer are planned for Beta.  More may be added depending on schedule.


###**Updates**
8/2: Fixed an issue where if a fresh install of mIRC did not generate a default `remote.ini` file the auto-load event for `AutoDL.mrc` would not be installed.  If mIRC does not automatically load the new `remote.ini`, simply go to `Tools > Script Editor > File > Load` and choose `remote.ini`.



