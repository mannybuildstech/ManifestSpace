# Manifest Space web prototype

This folder contains a lightweight browser prototype that mirrors the main gameplay loop from the Unity project and adapts it to your requested mechanics.

## Logic review from Unity project

Key scripts reviewed:
- `Assets/Scripts/Managers/GameManager.cs`
- `Assets/Scripts/Spawners/SolarSystemGenerator.cs`
- `Assets/Scripts/Models/Planet.cs`
- `Assets/Scripts/Models/SpaceStation.cs`
- `Assets/Scripts/Models/Projectile.cs`
- `Assets/Scripts/Models/SolarSystemSeed.cs`

Observed gameplay loop:
1. A solar system is procedurally generated each level (planet count/size/distance shift by level).
2. Clicking/selecting colonized planets and launching ships moves passengers from source to destination.
3. Planet population increases on successful landing and decreases on launch/death.
4. Colonizing enough planets unlocks portal/next phase; entering portal advances level and difficulty.
5. Asteroid threats appear as obstacles and can kill ships.

Adaptations in this prototype:
- Added explicit "babies" growth on first successful colonization of a planet (`newborns` bonus).
- Fixed-size goal of **8 planets** per level for a clear prototype objective.
- Advancing when all 8 are colonized increases difficulty (faster rotation + more asteroids + tighter spacing).
- Debris now orbits every planet, and debris count increases with each level.

## Run locally

From repository root:

```bash
python3 -m http.server 4173
```

Then open:

`http://localhost:4173/web-prototype/`

## Deploy to a static site (GitHub Pages)

This repo now includes an automated Pages workflow at:

- `.github/workflows/deploy-web-prototype.yml`

How to enable it:

1. Push this branch to GitHub.
2. In GitHub repo settings, open **Pages**.
3. Set **Source** to **GitHub Actions**.
4. After every merge/push to `main` or `master`, deployment runs automatically (you can also run it manually from **Actions → Deploy web prototype to GitHub Pages**).
5. Your site will be published at:
   - `https://<your-org-or-user>.github.io/<repo-name>/`

## Controls

- Click/tap a colonized planet to launch humans in the direction of its rotating launcher arm.
- Time launches so rockets avoid asteroids and hit uncolonized planets.
- Colonize all 8 planets to advance to the next level.
- Use **Reset Run** to restart from level 1.

## Troubleshooting GitHub Pages folder picker

If Pages settings only show `/(root)` and `/docs`, that is expected for **branch-based** Pages. GitHub only allows those two folder options.

Use one of these setups:

1. **Recommended (already configured in this repo):** set Pages **Source** to **GitHub Actions** so `.github/workflows/deploy-web-prototype.yml` publishes `web-prototype/` directly.
2. **Branch fallback:** keep Source as **Deploy from a branch** and choose `/docs` (this repo includes a mirrored build in `docs/`).
