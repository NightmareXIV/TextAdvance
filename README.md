# TextAdvance
Quest speedrunning assistance tool. Automatically confirm quest acceptation and completion, skip subtitles, cutscenes and most of prompts. 
## Become a Supporter!
If you like TextAdvance, please consider becoming a supporter on Patreon or via other means! This will help me to continue updating TextAdvance and work on new plugins and features and you will receive benefits such as early progress updates, priority support, prioritized feature requests, early testing builds and private tools. 
- [Subscribe on Patreon](https://subscribe.nightmarexiv.com/) - eligible for Discord role
- [Donate Litecoin, Bitcoin, Tether or other crypto](https://crypto.nightmarexiv.com/) - eligible for Discord role

### Also:
- [Explore other plugins I maintain or contributed to](https://explore.nightmarexiv.com/)
- [Join NightmareXIV Discord server to receive fast support and pings about plugin updates](https://discord.gg/m8NRt4X8Gf)
## Description

<p align="center"><img src="https://raw.githubusercontent.com/NightmareXIV/TextAdvance/master/meta/image.png"></p>

Primary functions:
- Automatically skip quest subtitles
- Automatically skip cutscenes (only ones that can be normally skipped)
- Automatically confirm quests acception and completion
- Automatically interact with nearby quest entities
- Automatically pick most valuable rewards
- Automatically fill and confirm request popups
- Configurable buttons to temporarily pause or enable plugin.

Additional functions:
- Integration with Splatoon, which highlights nearby quest-related entities. You can get Splatoon from Puni.sh repo `https://love.puni.sh/ment.json`. [Read how to install Splatoon here.](https://github.com/PunishXIV/Splatoon?tab=readme-ov-file#installation)
- Integration with vnavmesh, which upon using `/at mtq` will attempt to build a path to nearest quest entity and `/at mtf` which will attempt to build a path to flag on map. You can get vnavmesh from third party repo `https://puni.sh/api/repository/veynrepo`

**TextAdvance automatically disables itself every time you log out and requires you to type `/at` command whenever you want to reenable it.** You can override this behavior by enabling "Don't auto-disable plugin on logout" option.

Alternatively, you can configure characters on which you want it to automatically enable upon log in.
## This plugin is in development
This means that there are still features that I would like to implement in future or features that I would like to enhance, as well as that I'm accepting suggestions and feature requests.
## Installation
1. Install [FFXIVQuickLauncher](https://github.com/goatcorp/FFXIVQuickLauncher?tab=readme-ov-file#xivlauncher-----) and enable Dalamud in it's settings. You have to run the game through FFXIVQuickLauncher in order for any of these plugins to work.
2. Open Dalamud settings by typing `/xlsettings` in game chat.
3. Go to "Experimental" tab.
4. Find "Custom Plugin Repositories" section, agree with listed terms if needed and paste the following link into text input field: `https://github.com/NightmareXIV/MyDalamudPlugins/raw/main/pluginmaster.json`
5. Click "Save" button.

You should now have NightmareXIV plugins available in your plugin installer. <br>
Open plugin installer by typing `/xlplugins` in game chat, go to "Available plugins" section and search for a plugin you would like to install.

![image](https://github.com/NightmareXIV/MyDalamudPlugins/blob/main/meta/install/installer.png?raw=true)

## Support
Join NightmareXIV Discord server to receive support for this plugin: https://discord.gg/m8NRt4X8Gf
[![](https://dcbadge.vercel.app/api/server/m8NRt4X8Gf)](https://discord.gg/m8NRt4X8Gf)

The server operates on a ticket-based system. Please create a ticket and describe your issue.
Additionally, you may create an issue in the repository. Reply time for tickets may be significantly longer than on Discord, however, the issue does not have any risks to be lost.
(Basically, if you want to report a critical bug or receive help, prefer Discord, if you want to suggest feature or report non-critical bug, prefer Github)
