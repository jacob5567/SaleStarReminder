# SaleStarReminder

A client-side Risk of Rain 2 mod for the Seekers of the Storm DLC. It reminds you, in large red text across the top of the screen, when you're holding a **Sale Star** you haven't used yet.

While you hold an unused Sale Star and the stage still has an unopened chest it works on:

- **UNUSED SALE STAR** appears in red at the top of the screen, just below the box your items are shown in. It goes away once you spend the star, or once no chest the star works on is left on the stage, for example in the Bazaar.
- **About to waste it:** when the game draws its Sale Star circle around a chest you're looking at, and a better chest that's Large or higher is still unopened on the stage, the alert grows and fades in and out (twice per second, never a hard flash). Both can be turned off.
- **Most valuable chest** (optional, off by default): a line under the reminder names the best unopened chest on the stage to spend the star on, for example "There is a Legendary Chest on this stage". From best to worst:
  1. Legendary Chest
  2. Large Chest, Large Damage/Healing/Utility Chest, Rusty Lockbox (only while you hold a Rusted Key)
  3. Adaptive Chest
  4. Small Chest, Cloaked Chest, Damage/Healing/Utility Chest
  5. Equipment Barrel, Concept Reliquary
- **Making fun of you** (optional, off by default): spend the star on a small chest, an Equipment Barrel or a Concept Reliquary while a Large Chest, Rusty Lockbox or Legendary Chest is still unopened on the stage, and the reminder's spot makes fun of you for a few seconds. With the most-valuable-chest option on, it also names that better chest. The lines come from `MockLines.json` (see below). It works with the reminder turned off, so you can have only this.

The mod never changes gameplay and never sends anything over the network. It works when you join a host who doesn't have it installed.

## Configuration

Edit `BepInEx/config/sevenstack.SaleStarReminder.cfg`, or use the in-game settings menu if [Risk Of Options](https://thunderstore.io/package/Rune580/Risk_Of_Options/) is installed.

| Setting | Default | Description |
|---|---|---|
| `ShowReminder` | true | Show the red reminder. Turn it off and `MockSmallChest` on to only get the mocking message. |
| `ReminderFontSize` | 80 | Size of the main alert text. |
| `SubheaderFontSize` | 20 | Size of the smaller line under the main alert. |
| `ReminderOffsetY` | 8 | Gap below the item box at the top of the screen, in UI pixels. |
| `NearChestScale` | 1.3 | How much bigger the alert gets while the game circles a Sale Star chest that's worse than a Large-or-better chest still on the stage. 1 turns this off. |
| `PulseNearChest` | true | Fade the alert in and out, twice per second, at the same times it grows. |
| `ShowMostValuableChest` | false | Name the most valuable unopened Sale Star chest on the stage under the reminder. Also lets the mocking message name the chest you could have used. |
| `MockSmallChest` | false | Make fun of you after you spend the star on a small chest, Equipment Barrel or Concept Reliquary while a Large Chest or better was still available. |
| `MockIncludeVulgar` | false | Also pick from the vulgar lines when making fun of you. |
| `MockDurationSeconds` | 6 | How long the mocking message stays on screen. |
| `DebugLogging` / `DumpChestsKey` | false / F8 | Log each Sale Star use the mod sees, and dump every purchasable object on the stage, for troubleshooting. |
| `DebugAlwaysShow` | false | Show the reminder even without a Sale Star, for testing. |

### Mocking lines

The lines are in `MockLines.json`, next to `SaleStarReminder.dll` in the mod's plugin folder. Edit it with any text editor and restart the game to use your own. `smallChest` is the big line; `betterChest` is the smaller line under it, where `{0}` becomes the better chest's name. Each has a `clean` and a `vulgar` list. One line is picked at random from `clean`, or from both lists with `MockIncludeVulgar` on.

```json
{
  "smallChest": { "clean": ["..."], "vulgar": ["..."] },
  "betterChest": { "clean": ["You could have used it on the {0}"], "vulgar": ["..."] }
}
```

If the file is missing or isn't valid JSON, the mod logs a warning and doesn't make fun of you. Updating the mod replaces the file, so keep a copy of your own lines.

## Notes

- The Sale Star comes back at the start of every stage, so the reminder comes back too until you spend it.
- The most-valuable-chest line counts chests you haven't found yet, which is why it's off by default.
- The Rusty Lockbox only counts while you hold a Rusted Key yourself, even if a teammate's key made it spawn.
- The reminder hides while you remote-operate a drone, because drones can't open the chests a Sale Star works on.
- Chests from other mods still count as places to spend the star, but they rank below every chest listed above.

## Building

Requires the .NET 8 SDK and a local Risk of Rain 2 install.

```sh
dotnet build -c Release   # also copies the DLL into the r2modman Default profile
./build.sh                # builds and writes dist/SaleStarReminder-<version>.zip for Thunderstore
```

Paths default to the Steam and r2modman locations on Linux. Override `RoR2Dir` and `ProfileDir` in a `Directory.Build.props.user` file (see `Directory.Build.props`).

`tools/make_icon.py` draws `thunderstore/icon.png`.

The mocking lines live in `src/MockLines.json`. The build copies it next to the DLL, both in the r2modman profile and in the package.

The version lives in `thunderstore/manifest.json`. To release, bump it, run `./build.sh`, and publish a GitHub release tagged `v<version>` with the zip attached. The publish workflow then uploads that zip to Thunderstore.
