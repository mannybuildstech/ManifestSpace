(function initManifestPowerups() {
  const api = window.ManifestSpaceApi;
  if (!api) {
    return;
  }

  const laserButton = document.getElementById('laserButton');
  const laserCount = document.getElementById('laserCount');
  const debrisDestroyerButton = document.getElementById('debrisDestroyerButton');

  const state = {
    laserCharges: 0,
    laserArmed: false,
    debrisDestroyerUnlocked: false,
    debrisDestroyerArmed: false,
    awardedLaserLevels: new Set(),
    lastLevel: 1
  };

  function updateUi() {
    if (laserCount) {
      laserCount.textContent = String(state.laserCharges);
    }

    if (laserButton) {
      laserButton.classList.toggle('active', state.laserArmed);
      laserButton.title = state.laserCharges > 0
        ? 'Arms laser mode. Click a colonized planet to fire its beam along launch direction.'
        : 'No laser charges left. Gain more by leveling up (starting at level 5).';
    }

    if (debrisDestroyerButton) {
      debrisDestroyerButton.disabled = !state.debrisDestroyerUnlocked;
      debrisDestroyerButton.classList.toggle('active', state.debrisDestroyerArmed);
      debrisDestroyerButton.title = state.debrisDestroyerUnlocked
        ? 'Arms debris-destroyer mode. Click a debris piece to remove it.'
        : 'Unlocks at level 10.';
    }
  }

  function resetProgressionState() {
    state.laserCharges = 0;
    state.laserArmed = false;
    state.debrisDestroyerUnlocked = false;
    state.debrisDestroyerArmed = false;
    state.awardedLaserLevels.clear();
  }

  function awardLasersForLevel(level) {
    if (level < 5) {
      return;
    }

    if (!state.awardedLaserLevels.has(5)) {
      state.awardedLaserLevels.add(5);
      state.laserCharges += 3;
      api.setStatus('Power-up unlocked: +3 lasers');
    }

    for (let l = 7; l <= level; l += 2) {
      if (!state.awardedLaserLevels.has(l)) {
        state.awardedLaserLevels.add(l);
        state.laserCharges += 1;
        api.setStatus(`Level ${l} reward: +1 laser`);
      }
    }
  }

  function onLevelChanged(level) {
    if (level < state.lastLevel) {
      resetProgressionState();
    }

    awardLasersForLevel(level);

    if (level >= 10 && !state.debrisDestroyerUnlocked) {
      state.debrisDestroyerUnlocked = true;
      api.setStatus('Power-up unlocked: Debris Destroyer');
    } else if (level < 10) {
      state.debrisDestroyerUnlocked = false;
      state.debrisDestroyerArmed = false;
    }

    if (state.laserCharges <= 0) {
      state.laserArmed = false;
    }

    state.lastLevel = level;
    updateUi();
  }

  function rayHitDistance(start, direction, center, radius) {
    const vx = center.x - start.x;
    const vy = center.y - start.y;
    const t = vx * direction.x + vy * direction.y;
    if (t < 0) return null;

    const perpendicularSq = vx * vx + vy * vy - t * t;
    if (perpendicularSq > radius * radius) return null;
    return t;
  }

  function fireLaserFromPlanet(planet) {
    const stationH = Math.max(14, planet.radius * 0.55) * 0.55;
    const start = {
      x: planet.x + Math.cos(planet.angle) * (planet.radius + stationH),
      y: planet.y + Math.sin(planet.angle) * (planet.radius + stationH)
    };
    const direction = {
      x: Math.cos(planet.angle),
      y: Math.sin(planet.angle)
    };

    const game = api.getGame();
    let closestHit = null;

    for (const asteroid of game.asteroids) {
      const t = rayHitDistance(start, direction, asteroid, asteroid.radius + 2);
      if (t === null) continue;
      if (!closestHit || t < closestHit.t) {
        closestHit = { t, type: 'asteroid', target: asteroid };
      }
    }

    for (const piece of game.debris) {
      const debrisWorld = api.getDebrisWorldPosition(piece);
      if (!debrisWorld) continue;
      const t = rayHitDistance(start, direction, debrisWorld, piece.size * 0.5 + 1);
      if (t === null) continue;
      if (!closestHit || t < closestHit.t) {
        closestHit = { t, type: 'debris', target: piece };
      }
    }

    if (!closestHit) {
      api.setStatus('Laser fired but hit nothing');
      return;
    }

    if (closestHit.type === 'asteroid') {
      api.removeAsteroid(closestHit.target);
      api.playSound('asteroidCrash');
      api.setStatus('Laser destroyed an asteroid');
      return;
    }

    api.removeDebrisPiece(closestHit.target);
    api.playSound('reinforced');
    api.setStatus('Laser vaporized debris');
  }

  function handleLaserClick(world) {
    if (!state.laserArmed) return false;

    const colonizedPlanet = api.findPlanetAtWorld(world, true);
    if (!colonizedPlanet) {
      return false;
    }

    if (state.laserCharges <= 0) {
      state.laserArmed = false;
      updateUi();
      return false;
    }

    fireLaserFromPlanet(colonizedPlanet);
    state.laserCharges -= 1;
    // One click consumes one armed laser use; re-arm manually for next shot.
    state.laserArmed = false;
    updateUi();
    // Do not block base click handling so rocket launch still works.
    return false;
  }

  function handleDebrisDestroyerClick(world) {
    if (!state.debrisDestroyerArmed) return false;

    const piece = api.findDebrisAtWorld(world);
    if (!piece) {
      api.setStatus('Debris Destroyer: click directly on a debris object');
      return true;
    }

    api.removeDebrisPiece(piece);
    api.playSound('reinforced');
    api.setStatus('Debris Destroyer removed a debris object');
    return true;
  }

  function handlePointerUp(world) {
    if (handleDebrisDestroyerClick(world)) return true;
    if (handleLaserClick(world)) return true;
    return false;
  }

  function shouldShowLaserGuides() {
    return state.laserArmed;
  }

  if (laserButton) {
    laserButton.addEventListener('click', () => {
      if (state.laserCharges <= 0) {
        api.setStatus('No laser charges available');
        return;
      }
      state.laserArmed = !state.laserArmed;
      if (state.laserArmed) {
        state.debrisDestroyerArmed = false;
      }
      updateUi();
    });
  }

  if (debrisDestroyerButton) {
    debrisDestroyerButton.addEventListener('click', () => {
      if (!state.debrisDestroyerUnlocked) {
        api.setStatus('Debris Destroyer unlocks at level 10');
        return;
      }
      state.debrisDestroyerArmed = !state.debrisDestroyerArmed;
      if (state.debrisDestroyerArmed) {
        state.laserArmed = false;
      }
      updateUi();
    });
  }

  window.ManifestPowerups = {
    onLevelChanged,
    handlePointerUp,
    shouldShowLaserGuides
  };

  onLevelChanged(api.getGame().level);
})();
