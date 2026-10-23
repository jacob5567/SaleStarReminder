# Changelog

## 1.2.0
- Works with [RiskUI](https://thunderstore.io/package/Bubbet/RiskUI/). RiskUI moves the items to the bottom of the screen, which put the reminder off screen. With RiskUI installed and enabled, the reminder now sits below RiskUI's boss bar at the top of the screen and uses RiskUI's font.
- The mocking message is now half the size of the "UNUSED SALE STAR" alert.

## 1.1.0
- While the game circles a chest that's worse than one still on the stage, the alert now grows much faster and moves to the middle of the screen, where it's harder to miss. It moves back when you look away.

## 1.0.1
- Fixed being mocked for a chest you only looked at. A star spent some other way, such as by the Captain's Hacking Beacon opening a different chest, no longer counts against the chest you last aimed at.
- Fixed an error when a chest added by another mod has no name.
- The settings now say that `ShowReminder` can be turned off to get only the mocking message.

## 1.0.0
- New optional message that makes fun of you for spending the star on a small chest, an Equipment Barrel or a Concept Reliquary while a Large Chest, Rusty Lockbox or Legendary Chest was still on the stage. With the most-valuable-chest line on, it also names the chest you could have used. Off by default.
- The mocking lines come from `MockLines.json` next to the mod's DLL, so you can write your own. Vulgar lines are a separate option, also off by default.
- The reminder no longer shows while you remote-operate a drone, since drones can't open the chests a Sale Star works on.

## 0.9.0
- First release.
- Large red "UNUSED SALE STAR" text just below the item box at the top of the screen while you hold an unused Sale Star and the stage still has a chest it works on.
- The text grows and slowly pulses while the game circles a chest that's worse than a Large Chest or better still on the stage. Both can be turned off.
- Optional line naming the most valuable unopened Sale Star chest on the stage, for example "There is a Legendary Chest on this stage".
