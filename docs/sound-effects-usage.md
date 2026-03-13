# ManifestSpace Sound Effects Usage (Assets Scripts)

This report documents sound effect usage based on script analysis in `Assets/Scripts`.

## Key Notes

- The win cheer (`hurray`) is played when the win/session-ended panel is shown after a successful level.
- Asteroid impact SFX (`astroidCrash`) is played when an asteroid hits Earth or a planet.
- Spaceship collisions with debris/asteroids play `humanDeath` and also spawn an explosion visual prefab.
- Some declared clips are currently unused in code or have commented playback calls.

## Sound Effect Inventory

| Audio field (MusicPlayer or other) | Played by method | Trigger locations (script:line) | Usage in gameplay | Status |
|---|---|---|---|---|
| `hurray` | `playLevelWinSFX()` | `Assets/Scripts/UserInterface/Menu/UserInterface.cs:194` | Win/level-complete cheer when session-ended panel is shown for a win. | Active |
| `portal` | `playPortalEnteredSFX()` | `Assets/Scripts/Spawners/PortalSpawner.cs:59` | Portal transition SFX during landing sequence ship launch from portal. | Active |
| `portalHum` | `portalBackgroundHum(bool)` | `Assets/Scripts/Models/PortalBehavior.cs:10`, `:44`, `:60` | Portal ambient hum around portal lifecycle events (start/destroy/collision). | Active (always called with `true`) |
| `swoosh` | `playPlanetSelectSFX()` | `Assets/Scripts/Models/Planet.cs:127` | Planet selection sound when selecting a colonized planet. | Active |
| `launch` | `humanLaunchSound()`, `missileLaunchSound()` | `Assets/Scripts/Models/SpaceStation.cs:58`, `:70` | Launch sound for both human ships and missiles. | Active |
| `humanDeath` | `humansDiedSound()` | `Assets/Scripts/Models/Projectile.cs:51`, `:119` | Human-loss SFX when ship oxygen runs out or ship collides with debris/asteroid. | Active |
| `colonizedSound` | `playColonizedSound()` | `Assets/Scripts/Models/Projectile.cs:111`, `:147` | Colonization/re-colonization success sound when planets are populated. | Active |
| `astroidCrash` | `asteroidBlowSound()` | `Assets/Scripts/Models/AsteroidThreat.cs:95` | Asteroid crash impact sound when asteroid collides with Earth/planet. | Active |
| `missileBlow` | `missileBlowSound()` | `Assets/Scripts/Models/Projectile.cs:168`, `:180`; `Assets/Scripts/SightLineBehavior.cs:30` | Explosion/impact SFX for missile hits and flamethrower debris hits. | Active |
| `alarmThreat` | `asteroidWarning()` | `Assets/Scripts/Models/AsteroidThreat.cs:64` | Warning alert when asteroid enters danger radius. | Active |
| `popSound` | `PlayPlanetRatingSound(int)` | `Assets/Scripts/UserInterface/Menu/UserInterface.cs:354` | Rating pop during post-level star/rating fill animation. | Active |
| `TimeIsRunningOut` | `playRunningOutOfTimeSFX()` | No active call sites; playback line is commented in `MusicPlayer.cs:69` | Intended low-time warning SFX. | Declared, not active |
| `planetsAquiredSound` | none | No playback found in scripts | Intended planet-goal-achieved SFX (name suggests this). | Declared, unused |
| `conquerTheFurther` | none | No playback found in scripts | Likely music/track slot, not currently used in script playback. | Declared, unused |
| `mainMenuJam` | none | No playback found in scripts | Likely menu music slot, not currently used in script playback. | Declared, unused |
| `clipAudioSource` (in `startGame`) | `clipAudioSource.Play()` | `Assets/Scripts/UserInterface/Menu/startGame.cs:33` | Plays a start-button/menu clip before loading main scene. Clip asset is assigned in inspector. | Active (asset not identifiable from code alone) |

## Related Gameplay Clarification

- "All planets conquered" transition in `GameManager` calls `planetAchievementSound()` (`Assets/Scripts/Managers/GameManager.cs:113`), but that method currently has no active `PlayOneShot` (commented out in `MusicPlayer`).
- The audible win cheer is still present because `playLevelWinSFX()` is triggered later from UI win flow (`UserInterface.cs:194`).
- For "rocket crashed against asteroid":
  - Spaceship/crew rocket collision path uses `humanDeath` (`Projectile.cs:119`) and spawns an explosion prefab.
  - Asteroid-into-planet collision path uses `astroidCrash` (`AsteroidThreat.cs:95`).
  - Missile collision paths use `missileBlow` (`Projectile.cs:168`, `:180`).
