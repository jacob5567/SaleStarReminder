# SaleStarReminder

A client-side Risk of Rain 2 mod for the Seekers of the Storm DLC. It reminds you, in large red text across the top of the screen, when you're holding a **Sale Star** you haven't used yet.

While you hold an unused Sale Star and the stage still has an unopened chest it works on:

- **UNUSED SALE STAR** appears in red at the top of the screen, just below the box your items are shown in. It goes away once you spend the star, or once no chest the star works on is left on the stage, for example in the Bazaar.
- **Most valuable chest** (optional, off by default): a line under the reminder names the best unopened chest on the stage to spend the star on. From best to worst:
  1. Legendary Chest
  2. Large Chest, Large Damage/Healing/Utility Chest, Rusty Lockbox (only while you hold a Rusted Key)
  3. Adaptive Chest
  4. Chest, Cloaked Chest, Damage/Healing/Utility Chest
  5. Equipment Barrel, Concept Reliquary
- **Making fun of you** (optional, off by default): spend the star on a small chest, an Equipment Barrel or a Concept Reliquary, and the reminder's spot makes fun of you for a few seconds. With the most-valuable-chest option on, it also names the better chest you could have used it on.

The mod never changes gameplay and never sends anything over the network. It works when you join a host who doesn't have it installed.

## Configuration

Edit `BepInEx/config/sevenstack.SaleStarReminder.cfg`, or use the in-game settings menu if [Risk Of Options](https://thunderstore.io/package/Rune580/Risk_Of_Options/) is installed.

| Setting | Default | Description |
|---|---|---|
| `ShowReminder` | true | Show the red reminder. |
| `ReminderFontSize` | 40 | Size of the reminder text. The line under it is half this size. |
| `ReminderOffsetY` | 8 | Gap below the item box at the top of the screen, in UI pixels. |
| `ShowMostValuableChest` | false | Name the most valuable unopened Sale Star chest on the stage under the reminder. |
| `MockSmallChest` | false | Make fun of you after you spend the star on a small chest, Equipment Barrel or Concept Reliquary. |
| `MockIncludeVulgar` | true | Also pick from the vulgar lines when making fun of you. |
| `MockDurationSeconds` | 6 | How long the mocking message stays on screen. |
| `DebugLogging` / `DumpChestsKey` | false / F8 | Log each Sale Star use the mod sees, and dump every purchasable object on the stage, for troubleshooting. |
| `DebugAlwaysShow` | false | Show the reminder even without a Sale Star, for testing. |

## Notes

- The Sale Star comes back at the start of every stage, so the reminder comes back too until you spend it.
- The most-valuable-chest line counts chests you haven't found yet, which is why it's off by default.
- The Rusty Lockbox only counts while you hold a Rusted Key yourself, even if a teammate's key made it spawn.
- Chests from other mods still count as places to spend the star, but they're never named as the most valuable.

## Building

Requires the .NET 8 SDK and a local Risk of Rain 2 install.

```sh
dotnet build -c Release   # also copies the DLL into the r2modman Default profile
./build.sh                # builds and writes dist/SaleStarReminder-<version>.zip for Thunderstore
```

Paths default to the Steam and r2modman locations on Linux. Override `RoR2Dir` and `ProfileDir` in a `Directory.Build.props.user` file (see `Directory.Build.props`).

The mocking lines are in `src/MockLines.cs`. `tools/make_icon.py` draws `thunderstore/icon.png`.

The version lives in `thunderstore/manifest.json`. To release, bump it, run `./build.sh`, and publish a GitHub release tagged `v<version>` with the zip attached. The publish workflow then uploads that zip to Thunderstore.
