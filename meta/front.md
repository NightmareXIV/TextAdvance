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
