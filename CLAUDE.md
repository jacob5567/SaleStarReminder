
<!-- cope:profile -->
## Build and test notes (maintained by COPE; edit freely)
- Check: `dotnet build -c Release -p:CopyToProfile=false`
- Test signal: compile-only: a pass proves the C# compiles against the local RoR2/Unity/BepInEx references; there are no tests, and behavior is only verified in-game.
- Oracle: not-possible: the code runs only inside the Risk of Rain 2 host and there is no test project or harness.
- Hazards: a normal build copies the DLL, pdb and MockLines.json into the r2modman Default profile; `-p:CopyToProfile=false` disables that. Never run `./build.sh` (it builds and writes dist/ zips) or the publish workflow. The build needs the game install at `~/.local/share/Steam/steamapps/common/Risk of Rain 2` and Risk Of Options in the r2modman profile (read-only references, outside the repo).
- Worktree needs: `Directory.Build.props.user` (optional local override that sets CopyToProfile=false; the check passes the flag itself). The game install and r2modman profile are outside the repo at fixed paths.
- Baseline: pass: builds with 0 warnings, 0 errors (2026-10-17, 772f85c)
<!-- /cope:profile -->
