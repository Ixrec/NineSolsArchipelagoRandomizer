# Nine Sols Archipelago Randomizer

A Nine Sols mod for [the Archipelago multi-game randomizer system](https://archipelago.gg/).

## Status

In Development (as of March 2024).

Nothing playable yet. Expect it to be a few months before we get a playable release.

## Contact

For questions, feedback, or discussion related to the randomizer, please visit the "Nine Sols" thread in [the Archipelago Discord server](https://discord.gg/8Z65BR2), or the "Archipelago Randomizer" thread in [the Nine Sols Modding Discord server](https://discord.gg/sjA27B4e), or message me (`ixrec`) directly on Discord.

## What is an "Archipelago Randomizer", and why would I want one?

Let's say I'm playing Nine Sols, and my friend is playing Ocarina of Time. When I pick up the item dropped by Baichang, I find my friend's Hookshot, allowing them to reach several OoT chests they couldn't before. In one of those chests they find my Cloud Leap, allowing me to reach the Grotto of Scriptures. I retrieve the Shanhai 1000's map chip, and find my friend's Ocarina. This continues until we both find enough of our items to finish our games.

In essence, a multi-game randomizer system like Archipelago allows a group of friends to each bring whatever games they want (if they have an Archipelago mod) and smush them all together into one big cooperative multiplayer experience.

### What This Mod Changes

- The Archipelago items and locations are Yi's core movement abilities, almost every chest in the game (including the jin-only chests), every enemy drop, most of the objects you can examine for "database" entries, and several of the item-granting NPC interactions in FSP.
	- For now, none of the "post-PonR" content is randomized (except that the goal is defeating Eigong).
	- For now, none of the "post-all poisons" / Shennong questline / True Ending content is randomized.
	- For now, no shop items or skill tree upgrades are randomized.
- Starting a New Game will prompt you for Archipelago connection info, then put you immediately into the Four Seasons Pavilion (with power already on) instead of the usual intro sequence.
	- Teleport is immediately unlocked, along with one other root node for you to teleport too.
		- For now, this "first node" is always Apeman Facility (Monitoring).
	- The FSP's front door is "jammed" (i.e. the exit load zone is disabled), so you won't have immediate access to Central Hall.
	- Shuanshuan and Shennong are there immediately.
	- For now, Chiyou will move to FSP after checking the "Factory (GH): Raise the Bridge for Chiyou" location.
		- In vanilla it's triggered by the post-Prison Chiyou rescue cutscene, which in randomizer would be too easy to skip over, and IMO would push too many things into a fixed/non-random order.
	- Like in vanilla, Kuafu will move to FSP after checking the "Kuafu's Vital Sanctum" location.
- Many scripted events are now either triggered by sol seal counts, unlocked immediately and forever, or skipped entirely.
	- The Jiequan 1 fight and Prison sequence become available after collecting (for now) 3 sol seals. Unlike the vanilla game, that's *any* 3 sol seals.
	- The Lady Ethereal Soulscape entrance appears after collecting (for now) any 4 sol seals.
	- The Root Pinnacle opens, granting access to the final Eigong fight, after collecting (for now) 8 sol seals, instead of the Point of no Return cutscenes.
	- The Peach Blossom Village rescue quest will no longer wait for you to escape Prison before activating. It can be done as soon as you find the Abandoned Mines Access Token and can reach the gate it unlocks.
	- All "Limitless Realm" segments are disabled/skipped for now.
	- Ji remains at Daybreak Tower to give you the Ancient Sheet Music even if he's supposed to be somewhere else.

## Installation

TODO

## Other Suggested Mods and Tools

[BepinExConfigurationManager](https://thunderstore.io/c/nine-sols/p/ninesolsmodding/BepinExConfigurationManager/) is not _strictly_ required, but it lets you change Nine Sols mod settings (not just this mod) with an in-game menu simply by pressing F1. Without it, you'd have to edit config files instead.

[My CutsceneSkip mod](https://thunderstore.io/c/nine-sols/p/Ixrec/CutsceneSkip/) does exactly what it sounds like.

Universal Tracker is fully supported. For now, it's also the only supported tracker, so it's highly recommended. See the pinned messages [in its Discord thread](https://discord.com/channels/731205301247803413/1170094879142051912) for details.

[N00byKing's NineSolsTracker mod](https://thunderstore.io/c/nine-sols/p/N00byKing/NineSolsTracker/) may help with finding items and chests in-game.

[Gogas1 BossChallengeMod](https://thunderstore.io/c/nine-sols/p/Gogas1/BossChallengeMod/) is obviously relevant if you're good at this game, but I'd particularly like to highlight its "random modifier" settings.

## Roadmap

First playable release: Manage save files, connect to AP, start in FSP, actually shuffle the items and locations, various game state edits to keep all this playable, have defeating Eigong as the goal.

Features I probably do want to prioritize:
- combat logic/boss scaling/some way of mitigating lategame boss sponginess when encountered early in rando
- random "spawn" / first root node
- randomizing jade computing power costs, just like HK rando
- randomizing the timing/conditions of 4 major scripted events: the Jiequan 1 & prison sequence, the Peach Blossom Village invasion, Lady E's soulscape unlocking, and Eigong's fight unlocking
- trap items (Sniper Trap? Prison State Trap? Internal Damage Trap? etc)
- randomize Wall Climb and Grapple starting abilities

Features I probably want, but not that soon:
- decide on additional goals, how to handle post-PonR content, and whether to do anything with the Chien/Chiyou/Shennong quests and True Ending
- hard/glitch/trick logic
- entrance randomization

Features I'm less sure about but am considering:
- randomizing shop items
- randomizing the skill tree
- turning root node unlocks into items, like HK's shuffle stag stations option
- in-game hints from the Shanhai 9000s
- randomize BGM

## Credits

- GameWyrm, Gregório, Hopop, Juanba, mynameis, XDrotkon and others in various Nine Sols and Archipelago-related Discord servers for feedback, discussion and encouragement
- dubi steinkek, yuki.kako, N00byKing and others from the "Nine Sols Modding" Discord server for help modding Nine Sols and for creating the other Nine Sols mods that this randomizer relies on or is often played with
- Flitter for talking me into trying out Archipelago randomizers in the first place
- All the Archipelago contributors who made that great multi-randomizer system
- Everyone at Red Candle Games who made this great game
