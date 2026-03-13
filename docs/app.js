const canvas = document.getElementById('gameCanvas');
const ctx = canvas.getContext('2d');

const hud = {
  level: document.getElementById('level'),
  colonized: document.getElementById('colonized'),
  humans: document.getElementById('humans'),
  status: document.getElementById('status')
};

const resetButton = document.getElementById('resetButton');
const zoomInButton = document.getElementById('zoomInButton');
const zoomOutButton = document.getElementById('zoomOutButton');

const GAME = {
  level: 1,
  planetsPerLevel: 8,
  baseHumans: 42,
  passengersPerLaunch: 8,
  babyGrowthMultiplier: 0.35,
  planetMinRadius: 35,
  planetMaxRadius: 59.5,
  asteroids: [],
  debris: [],
  planets: [],
  rockets: [],
  lastTime: 0,
  statusTimeout: null,
  lastClickedPlanetIndex: null,
  bgScrollX: 0,
  worldBounds: { left: 0, right: 0, top: 0, bottom: 0 },
  camera: {
    x: 0,
    y: 0,
    zoom: 1,
    minZoom: 0.45,
    maxZoom: 2.4,
    isPanning: false,
    panStartX: 0,
    panStartY: 0,
    cameraStartX: 0,
    cameraStartY: 0,
    movedDuringPan: false
  }
};

const PLANET_TEXTURE_CONFIG = {
  starter: 'assets/PNG_Only/Planets/Planet_Earth.png',
  randomPool: [
    'assets/PNG_Only/Planets/Planet_Ciri.png',
    'assets/PNG_Only/Planets/Planet_Desert.png',
    'assets/PNG_Only/Planets/Planet_Ignot.png',
    'assets/PNG_Only/Planets/Planet_Kronum.png',
    'assets/PNG_Only/Planets/Planet_Quen.png'
  ]
};

const DEBRIS_ASTEROID_POOL = [
  'assets/PNG_Only/Debri/Asteroid Debris.png',
  'assets/PNG_Only/Asteroids/Asteroid_1.png',
  'assets/PNG_Only/Asteroids/Asteroids_2.png'
];

const DEBRIS_SATELLITE_POOL = [
  'assets/PNG_Only/Debri/Satellite Debris.png',
  'assets/PNG_Only/Satellites/Satellite_1.png',
  'assets/PNG_Only/Satellites/Satellite_2.png',
  'assets/PNG_Only/Satellites/ISS.png'
];

const PLANET_TEXTURE_CACHE = new Map();

const LAUNCH_MARKER_TEXTURE = 'assets/PNG_Only/Misc/Florida Space Station.png';
const ROCKET_TEXTURE = 'assets/PNG_Only/Misc/NASA Spaceship.png';
const BG_TEXTURE = 'assets/PNG_Only/Misc/Space Background.png';
const LAUNCH_OFFSET = 8;
const ROCKET_COLLISION_RADIUS = 8;
const ROCKET_SPRITE_ANGLE_OFFSET = Math.PI / 2;
const SHOW_LASER = false;

function rand(min, max) {
  return Math.random() * (max - min) + min;
}

function clamp(value, min, max) {
  return Math.max(min, Math.min(max, value));
}

function randIntInclusive(min, max) {
  return Math.floor(rand(min, max + 1));
}

function dist(a, b) {
  return Math.hypot(a.x - b.x, a.y - b.y);
}

function setStatus(msg) {
  hud.status.textContent = msg;
}

function screenToWorld(screenX, screenY) {
  return {
    x: GAME.camera.x + (screenX - canvas.width * 0.5) / GAME.camera.zoom,
    y: GAME.camera.y + (screenY - canvas.height * 0.5) / GAME.camera.zoom
  };
}

function zoomCamera(multiplier) {
  GAME.camera.zoom = clamp(GAME.camera.zoom * multiplier, GAME.camera.minZoom, GAME.camera.maxZoom);
}

function clampCameraToBounds() {
  const halfVisibleWidth = (canvas.width * 0.5) / GAME.camera.zoom;
  const halfVisibleHeight = (canvas.height * 0.5) / GAME.camera.zoom;
  const bounds = GAME.worldBounds;

  const minX = bounds.left + halfVisibleWidth;
  const maxX = bounds.right - halfVisibleWidth;
  const minY = bounds.top + halfVisibleHeight;
  const maxY = bounds.bottom - halfVisibleHeight;

  GAME.camera.x = minX > maxX ? (bounds.left + bounds.right) * 0.5 : clamp(GAME.camera.x, minX, maxX);
  GAME.camera.y = minY > maxY ? (bounds.top + bounds.bottom) * 0.5 : clamp(GAME.camera.y, minY, maxY);
}

function resetCameraToEarth() {
  if (!GAME.planets.length) return;
  GAME.camera.zoom = 1;
  GAME.camera.x = GAME.planets[0].x;
  GAME.camera.y = GAME.planets[0].y;
  clampCameraToBounds();
}

function updateHud() {
  const colonizedCount = GAME.planets.filter((p) => p.population > 0).length;
  const totalHumans = GAME.planets.reduce((sum, p) => sum + p.population, 0);
  hud.level.textContent = String(GAME.level);
  hud.colonized.textContent = `${colonizedCount} / ${GAME.planetsPerLevel}`;
  hud.humans.textContent = String(totalHumans);
}

function makePlanet(index, x, y, radius, levelParams) {
  const baseSpeed = rand(levelParams.minPlanetRotationSpeed, levelParams.maxPlanetRotationSpeed);
  const texturePath = index === 0
    ? PLANET_TEXTURE_CONFIG.starter
    : PLANET_TEXTURE_CONFIG.randomPool[Math.floor(rand(0, PLANET_TEXTURE_CONFIG.randomPool.length))];

  return {
    index,
    x,
    y,
    radius,
    angle: rand(0, Math.PI * 2),
    rotationSpeed: (Math.random() > 0.5 ? 1 : -1) * baseSpeed,
    population: index === 0 ? GAME.baseHumans : 0,
    texturePath
  };
}

function loadTexture(path) {
  if (PLANET_TEXTURE_CACHE.has(path)) return PLANET_TEXTURE_CACHE.get(path);

  const image = new Image();
  const asset = { image, loaded: false };
  image.onload = () => {
    asset.loaded = true;
  };
  image.src = path;
  PLANET_TEXTURE_CACHE.set(path, asset);
  return asset;
}

function canPlacePlanet(candidate, placed, minGap) {
  return placed.every((p) => dist(candidate, p) > candidate.radius + p.radius + minGap);
}

function buildLevelParams(level) {
  const systemIndex = Math.max(0, level - 1);

  const requiredPlanets = Math.min(50, Math.floor(3 + 0.75 * Math.pow(systemIndex, 1.15)));
  let targetPlanetCount = requiredPlanets;
  if (systemIndex > 0) {
    const inverseOpportunity = Math.max(2, -0.1 * (systemIndex / 0.5) + 5);
    const maxPlanets = requiredPlanets + Math.floor(inverseOpportunity);
    targetPlanetCount = randIntInclusive(requiredPlanets, Math.max(requiredPlanets, maxPlanets));
  }

  const potentialSpeedIncreaseRate = 0.01 * Math.pow(systemIndex, 2);
  const minPlanetRotationSpeed = clamp(0.75 + potentialSpeedIncreaseRate, 0.75, 4);
  const maxPlanetRotationSpeed = clamp(1 + potentialSpeedIncreaseRate, 1, 5);

  let minDebrisCount = 0;
  let maxDebrisCount = 0;
  if (systemIndex > 0) {
    const debrisCurve = Math.pow(systemIndex, 1.15);
    minDebrisCount = clamp(Math.floor(debrisCurve / 4), 1, 4);
    maxDebrisCount = clamp(minDebrisCount + 2 + Math.floor(debrisCurve / 3), 2, 7);
  }

  const asteroidCount = systemIndex === 0 ? 0 : clamp(Math.floor(1 + systemIndex * 0.85), 1, 10);
  const obstacleSpinMultiplier = 1 + systemIndex * 0.09;
  const earthEdgeBias = clamp((systemIndex - 1) / 9, 0, 1);

  return {
    systemIndex,
    requiredPlanets,
    targetPlanetCount,
    minPlanetRotationSpeed,
    maxPlanetRotationSpeed,
    minDebrisCount,
    maxDebrisCount,
    asteroidCount,
    obstacleSpinMultiplier,
    earthEdgeBias
  };
}

function pickEarthPosition(radius, padding, levelParams) {
  const center = { x: canvas.width * 0.5, y: canvas.height * 0.5 };
  if (levelParams.earthEdgeBias <= 0) {
    return center;
  }

  const dirAngle = rand(0, Math.PI * 2);
  const direction = { x: Math.cos(dirAngle), y: Math.sin(dirAngle) };
  const maxOffsetX = Math.max(0, canvas.width * 0.5 - padding - radius);
  const maxOffsetY = Math.max(0, canvas.height * 0.5 - padding - radius);
  const maxOffset = Math.min(maxOffsetX, maxOffsetY) * 0.92;
  const offset = maxOffset * levelParams.earthEdgeBias;

  return {
    x: center.x + direction.x * offset,
    y: center.y + direction.y * offset
  };
}


function rayToCanvasEdge(origin, direction) {
  const tCandidates = [];

  if (direction.x > 0) tCandidates.push((canvas.width - origin.x) / direction.x);
  if (direction.x < 0) tCandidates.push((0 - origin.x) / direction.x);
  if (direction.y > 0) tCandidates.push((canvas.height - origin.y) / direction.y);
  if (direction.y < 0) tCandidates.push((0 - origin.y) / direction.y);

  const t = Math.min(...tCandidates.filter((candidate) => candidate > 0));
  return {
    x: origin.x + direction.x * t,
    y: origin.y + direction.y * t
  };
}

function drawTexturedCircle(x, y, radius, texturePath, fallbackColor) {
  const textureAsset = loadTexture(texturePath);

  ctx.save();
  ctx.translate(x, y);
  ctx.beginPath();
  ctx.arc(0, 0, radius, 0, Math.PI * 2);
  ctx.clip();

  if (textureAsset.loaded) {
    ctx.drawImage(textureAsset.image, -radius, -radius, radius * 2, radius * 2);
  } else {
    ctx.fillStyle = fallbackColor;
    ctx.fillRect(-radius, -radius, radius * 2, radius * 2);
  }
  ctx.restore();
}

function generateLevel() {
  const baselineArea = 960 * 600;
  const currentArea = canvas.width * canvas.height;
  const areaScale = Math.sqrt(currentArea / baselineArea);
  const planetScale = Math.max(1, Math.min(2.2, areaScale));
  const levelParams = buildLevelParams(GAME.level);

  const scaledMinRadius = GAME.planetMinRadius * planetScale;
  const scaledMaxRadius = GAME.planetMaxRadius * planetScale;
  const padding = Math.max(110, 110 * planetScale);
  const baseGap = Math.max(32 * planetScale, (70 - GAME.level * 2.2) * planetScale);
  let minGap = baseGap * 1.15;
  if (levelParams.systemIndex === 1) minGap = baseGap * 1.55;
  if (levelParams.systemIndex >= 2) minGap = baseGap * 1.9;
  const targetCount = levelParams.targetPlanetCount;
  GAME.planetsPerLevel = targetCount;

  GAME.planets = [];
  GAME.asteroids = [];
  GAME.debris = [];
  GAME.rockets = [];
  GAME.lastClickedPlanetIndex = 0;

  const earthRadius = rand(scaledMinRadius, scaledMaxRadius);
  const earthPosition = pickEarthPosition(earthRadius, padding, levelParams);
  GAME.planets.push(makePlanet(0, earthPosition.x, earthPosition.y, earthRadius, levelParams));

  const edgeLimit = Math.max(60, Math.min(
    earthPosition.x - padding - scaledMaxRadius,
    canvas.width - earthPosition.x - padding - scaledMaxRadius,
    earthPosition.y - padding - scaledMaxRadius,
    canvas.height - earthPosition.y - padding - scaledMaxRadius
  ));

  let maxSpreadDistance = edgeLimit * 0.58;
  if (levelParams.systemIndex === 1) maxSpreadDistance = edgeLimit * 0.88;
  if (levelParams.systemIndex >= 2) maxSpreadDistance = edgeLimit * 0.98;

  let minSpreadDistance = maxSpreadDistance * 0.42;
  if (levelParams.systemIndex === 1) minSpreadDistance = maxSpreadDistance * 0.62;
  if (levelParams.systemIndex >= 2) minSpreadDistance = maxSpreadDistance * 0.8;

  const minAllowedDistance = earthRadius + scaledMaxRadius + minGap;
  minSpreadDistance = Math.max(minSpreadDistance, minAllowedDistance);
  maxSpreadDistance = Math.max(maxSpreadDistance, minSpreadDistance + 24);

  for (let i = 1; i < targetCount; i += 1) {
    let tries = 0;
    while (tries < 500) {
      const radius = rand(scaledMinRadius, scaledMaxRadius);
      const spawnDistance = rand(minSpreadDistance, maxSpreadDistance);
      const spawnAngle = rand(0, Math.PI * 2);
      const candidate = {
        x: earthPosition.x + Math.cos(spawnAngle) * spawnDistance,
        y: earthPosition.y + Math.sin(spawnAngle) * spawnDistance,
        radius
      };
      if (canPlacePlanet(candidate, GAME.planets, minGap)) {
        GAME.planets.push(makePlanet(i, candidate.x, candidate.y, radius, levelParams));
        break;
      }
      tries += 1;
    }
  }

  const farthestPlanetDistance = GAME.planets.reduce((maxDistance, planet) => {
    const d = Math.hypot(planet.x - earthPosition.x, planet.y - earthPosition.y) + planet.radius;
    return Math.max(maxDistance, d);
  }, 0);
  const worldPadding = Math.max(220, 220 * planetScale);
  GAME.worldBounds = {
    left: earthPosition.x - farthestPlanetDistance - worldPadding,
    right: earthPosition.x + farthestPlanetDistance + worldPadding,
    top: earthPosition.y - farthestPlanetDistance - worldPadding,
    bottom: earthPosition.y + farthestPlanetDistance + worldPadding
  };

  const asteroidCount = levelParams.asteroidCount;
  for (let i = 0; i < asteroidCount; i += 1) {
    let tries = 0;
    while (tries < 300) {
      const spawnAngle = rand(0, Math.PI * 2);
      const spawnDistance = rand(minSpreadDistance * 0.6, farthestPlanetDistance + worldPadding * 0.5);
      const asteroid = {
        x: earthPosition.x + Math.cos(spawnAngle) * spawnDistance,
        y: earthPosition.y + Math.sin(spawnAngle) * spawnDistance,
        radius: rand(12, 19) * 1.25,
        angle: rand(0, Math.PI * 2),
        spinSpeed: rand(0.4, 1.2) * levelParams.obstacleSpinMultiplier,
        texturePath: DEBRIS_ASTEROID_POOL[Math.floor(Math.random() * DEBRIS_ASTEROID_POOL.length)]
      };
      const blocksPlanet = GAME.planets.some((p) => dist(p, asteroid) < p.radius + asteroid.radius + 16);
      const overlapsAsteroid = GAME.asteroids.some((a) => dist(a, asteroid) < a.radius + asteroid.radius + 12);
      if (!blocksPlanet && !overlapsAsteroid) {
        GAME.asteroids.push(asteroid);
        break;
      }
      tries += 1;
    }
  }

  const debrisCountMin = levelParams.minDebrisCount;
  const debrisCountMax = levelParams.maxDebrisCount;
  for (const planet of GAME.planets) {
    const debrisCount = debrisCountMax > 0 ? randIntInclusive(debrisCountMin, debrisCountMax) : 0;
    for (let i = 0; i < debrisCount; i += 1) {
      const isAsteroid = i % 2 === 0;
      const pool = isAsteroid ? DEBRIS_ASTEROID_POOL : DEBRIS_SATELLITE_POOL;
      const texturePath = pool[Math.floor(Math.random() * pool.length)];
      GAME.debris.push({
        planetIndex: planet.index,
        angle: rand(0, Math.PI * 2),
        orbitRadius: planet.radius + rand(14, 30),
        orbitSpeed: rand(0.45, 1.5) * levelParams.obstacleSpinMultiplier * (Math.random() > 0.5 ? 1 : -1),
        size: (isAsteroid ? rand(10, 18) : rand(12, 20)) * 1.25,
        texturePath
      });
    }
  }

  // Use actual spawned count so level completion cannot become unreachable.
  GAME.planetsPerLevel = GAME.planets.length;

  resetCameraToEarth();
  setStatus(`Level ${GAME.level} generated`);
  updateHud();
}

function updateDebris(dt) {
  for (const piece of GAME.debris) {
    piece.angle += piece.orbitSpeed * dt;
  }
}

function updateAsteroids(dt) {
  for (const asteroid of GAME.asteroids) {
    asteroid.angle += asteroid.spinSpeed * dt;
  }
}

function launchRocket(planet) {
  if (planet.population <= 0) {
    setStatus('No population to launch from this planet');
    return;
  }

  const livingPlanets = GAME.planets.filter((p) => p.population > 0);
  if (livingPlanets.length === GAME.planetsPerLevel) return;

  const launchPassengers = Math.min(GAME.passengersPerLaunch, planet.population);
  planet.population -= launchPassengers;

  const launchDir = { x: Math.cos(planet.angle), y: Math.sin(planet.angle) };
  GAME.rockets.push({
    x: planet.x + launchDir.x * (planet.radius + LAUNCH_OFFSET),
    y: planet.y + launchDir.y * (planet.radius + LAUNCH_OFFSET),
    vx: launchDir.x * (190 + GAME.level * 14),
    vy: launchDir.y * (190 + GAME.level * 14),
    passengers: launchPassengers,
    life: 4.7,
    sourceIndex: planet.index
  });

  setStatus(`Launched ${launchPassengers} humans from planet ${planet.index + 1}`);
  updateHud();
}

function getDebrisWorldPosition(piece) {
  const planet = GAME.planets[piece.planetIndex];
  if (!planet) return null;
  return {
    x: planet.x + Math.cos(piece.angle) * piece.orbitRadius,
    y: planet.y + Math.sin(piece.angle) * piece.orbitRadius
  };
}

function markRocketDestroyed(rocket, reason) {
  rocket.status = reason;
  if (reason === 'Stray') {
    setStatus(`Rocket became Stray; ${rocket.passengers} humans died`);
  } else if (reason === 'Debris') {
    setStatus(`Rocket destroyed by debris; ${rocket.passengers} humans died`);
  } else {
    setStatus(`Rocket destroyed by asteroid; ${rocket.passengers} humans died`);
  }
}

function updateRockets(dt) {
  for (let i = GAME.rockets.length - 1; i >= 0; i -= 1) {
    const rocket = GAME.rockets[i];
    rocket.x += rocket.vx * dt;
    rocket.y += rocket.vy * dt;
    rocket.life -= dt;

    const outOfBounds = rocket.x < GAME.worldBounds.left - 20
      || rocket.x > GAME.worldBounds.right + 20
      || rocket.y < GAME.worldBounds.top - 20
      || rocket.y > GAME.worldBounds.bottom + 20;
    if (rocket.life <= 0 || outOfBounds) {
      markRocketDestroyed(rocket, outOfBounds ? 'Stray' : 'Expired');
      GAME.rockets.splice(i, 1);
      continue;
    }

    const hitAsteroid = GAME.asteroids.find((a) => dist(rocket, a) <= a.radius + ROCKET_COLLISION_RADIUS);
    if (hitAsteroid) {
      markRocketDestroyed(rocket, 'Asteroid');
      GAME.rockets.splice(i, 1);
      continue;
    }

    const hitDebris = GAME.debris.find((piece) => {
      const debrisWorld = getDebrisWorldPosition(piece);
      if (!debrisWorld) return false;
      const debrisRadius = piece.size * 0.45;
      return dist(rocket, debrisWorld) <= debrisRadius + ROCKET_COLLISION_RADIUS;
    });
    if (hitDebris) {
      markRocketDestroyed(rocket, 'Debris');
      GAME.rockets.splice(i, 1);
      continue;
    }

    const target = GAME.planets.find((p) => p.index !== rocket.sourceIndex && dist(rocket, p) <= p.radius);
    if (target) {
      const wasVirgin = target.population === 0;
      target.population += rocket.passengers;
      if (wasVirgin) {
        const newborns = Math.ceil(rocket.passengers * GAME.babyGrowthMultiplier + GAME.level * 0.75);
        target.population += newborns;
        setStatus(`Planet ${target.index + 1} colonized (+${rocket.passengers}, babies +${newborns})`);
      } else {
        setStatus(`Reinforced planet ${target.index + 1}`);
      }
      GAME.rockets.splice(i, 1);
      updateHud();
    }
  }
}

function checkProgression() {
  const colonizedCount = GAME.planets.filter((p) => p.population > 0).length;
  if (GAME.planetsPerLevel > 0 && colonizedCount >= GAME.planetsPerLevel) {
    GAME.level += 1;
    GAME.baseHumans = Math.max(28, GAME.baseHumans - 1);
    GAME.passengersPerLaunch = Math.max(4, GAME.passengersPerLaunch - 0.15);
    generateLevel();
    setStatus(`All planets colonized! Welcome to level ${GAME.level}`);
  }
}

function drawBackground(dt) {
  ctx.save();
  ctx.fillStyle = '#000';
  ctx.fillRect(0, 0, canvas.width, canvas.height);
  ctx.restore();
}

function drawRoundedRect(x, y, width, height, radius) {
  const r = Math.min(radius, width * 0.5, height * 0.5);
  ctx.beginPath();
  ctx.moveTo(x + r, y);
  ctx.lineTo(x + width - r, y);
  ctx.quadraticCurveTo(x + width, y, x + width, y + r);
  ctx.lineTo(x + width, y + height - r);
  ctx.quadraticCurveTo(x + width, y + height, x + width - r, y + height);
  ctx.lineTo(x + r, y + height);
  ctx.quadraticCurveTo(x, y + height, x, y + height - r);
  ctx.lineTo(x, y + r);
  ctx.quadraticCurveTo(x, y, x + r, y);
  ctx.closePath();
}

function drawLaunchMarker(planet) {
  const markerAsset = loadTexture(LAUNCH_MARKER_TEXTURE);
  // Station width and height — keep it small relative to the planet
  const stationW = Math.max(14, planet.radius * 0.55);
  const stationH = stationW * 0.55;

  ctx.save();
  // Move to planet center, then rotate so the station stands perpendicular to the surface
  // (angle 0 = right; station base sits on the surface, nose points outward)
  ctx.rotate(planet.angle);
  // Place station so its base is at the planet edge
  ctx.translate(planet.radius + stationH * 0.5, 0);
  // Rotate 90° so station is upright relative to the surface normal
  ctx.rotate(Math.PI / 2);

  if (markerAsset.loaded) {
    ctx.drawImage(markerAsset.image, -stationW * 0.5, -stationH * 0.5, stationW, stationH);
  } else {
    ctx.fillStyle = '#e8eef7';
    ctx.fillRect(-stationW * 0.5, -stationH * 0.5, stationW, stationH);
  }

  ctx.restore();
}

function drawPlanets() {
  for (const planet of GAME.planets) {
    ctx.save();
    ctx.translate(planet.x, planet.y);

    const colonized = planet.population > 0;
    drawTexturedCircle(0, 0, planet.radius, planet.texturePath, colonized ? '#34d399' : '#7080b5');

    ctx.strokeStyle = colonized ? 'rgba(143, 255, 213, 0.9)' : 'rgba(164, 182, 230, 0.55)';
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.arc(0, 0, planet.radius, 0, Math.PI * 2);
    ctx.stroke();

    if (colonized) {
      const stationH = Math.max(14, planet.radius * 0.55) * 0.55;
      const laserOriginDist = planet.radius + stationH;
      const launchStartWorld = {
        x: planet.x + Math.cos(planet.angle) * laserOriginDist,
        y: planet.y + Math.sin(planet.angle) * laserOriginDist
      };
      const launchDirection = {
        x: Math.cos(planet.angle),
        y: Math.sin(planet.angle)
      };
      const launchEndWorld = rayToCanvasEdge(launchStartWorld, launchDirection);

      if (SHOW_LASER) {
        ctx.strokeStyle = 'rgba(255, 40, 40, 0.21)';
        ctx.lineWidth = 2;
        ctx.beginPath();
        ctx.moveTo(launchStartWorld.x - planet.x, launchStartWorld.y - planet.y);
        ctx.lineTo(launchEndWorld.x - planet.x, launchEndWorld.y - planet.y);
        ctx.stroke();
      }

      drawLaunchMarker(planet);
    }

    const populationLabel = ` ${planet.population}`;
    ctx.font = 'bold 12px sans-serif';
    const labelWidth = Math.ceil(ctx.measureText(populationLabel).width + 20);
    const labelHeight = 18;
    const labelX = -labelWidth / 2;
    const labelY = -labelHeight / 2;

    ctx.fillStyle = 'rgba(0, 0, 0, 0.82)';
    drawRoundedRect(labelX, labelY, labelWidth, labelHeight, 7);
    ctx.fill();

    ctx.fillStyle = '#e9f1ff';
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText(populationLabel, 0, 0);

    ctx.restore();
  }
}

function drawAsteroids() {
  for (const asteroid of GAME.asteroids) {
    drawDebrisSprite(asteroid.x, asteroid.y, asteroid.radius * 2, asteroid.texturePath, asteroid.angle, '#8b6a53');
  }
}

function drawDebrisSprite(x, y, size, texturePath, angle, fallbackColor) {
  const asset = loadTexture(texturePath);
  ctx.save();
  ctx.translate(x, y);
  ctx.rotate(angle);
  if (asset.loaded) {
    ctx.drawImage(asset.image, -size * 0.5, -size * 0.5, size, size);
  } else {
    ctx.fillStyle = fallbackColor;
    ctx.beginPath();
    ctx.arc(0, 0, size * 0.4, 0, Math.PI * 2);
    ctx.fill();
  }
  ctx.restore();
}

function drawDebris() {
  for (const piece of GAME.debris) {
    const planet = GAME.planets[piece.planetIndex];
    if (!planet) continue;

    const x = planet.x + Math.cos(piece.angle) * piece.orbitRadius;
    const y = planet.y + Math.sin(piece.angle) * piece.orbitRadius;
    // Rotate sprite tangent to orbit so it looks like it's flying along
    const tangentAngle = piece.angle + Math.PI / 2;
    const isSatellite = DEBRIS_SATELLITE_POOL.includes(piece.texturePath);
    const fallback = isSatellite ? '#b8c2d1' : '#8a6f5f';
    drawDebrisSprite(x, y, piece.size, piece.texturePath, tangentAngle, fallback);
  }
}

function drawRockets() {
  const rocketAsset = loadTexture(ROCKET_TEXTURE);
  for (const rocket of GAME.rockets) {
    ctx.save();
    ctx.translate(rocket.x, rocket.y);
    const angle = Math.atan2(rocket.vy, rocket.vx) + ROCKET_SPRITE_ANGLE_OFFSET;
    ctx.rotate(angle);
    if (rocketAsset.loaded) {
      const baseHeight = 30;
      const sourceWidth = rocketAsset.image.naturalWidth || rocketAsset.image.width || 1;
      const sourceHeight = rocketAsset.image.naturalHeight || rocketAsset.image.height || 1;
      const aspectRatio = sourceWidth / sourceHeight;
      const rh = baseHeight;
      const rw = rh * aspectRatio;
      ctx.drawImage(rocketAsset.image, -rw * 0.5, -rh * 0.5, rw, rh);
    } else {
      ctx.fillStyle = '#ffd966';
      ctx.beginPath();
      ctx.moveTo(8, 0);
      ctx.lineTo(-7, 4);
      ctx.lineTo(-4, 0);
      ctx.lineTo(-7, -4);
      ctx.closePath();
      ctx.fill();
    }
    ctx.restore();
  }
}

function tick(timestamp) {
  if (!GAME.lastTime) GAME.lastTime = timestamp;
  const dt = Math.min((timestamp - GAME.lastTime) / 1000, 0.032);
  GAME.lastTime = timestamp;

  for (const planet of GAME.planets) {
    planet.angle += planet.rotationSpeed * dt;
  }

  updateDebris(dt);
  updateAsteroids(dt);

  updateRockets(dt);
  checkProgression();

  ctx.clearRect(0, 0, canvas.width, canvas.height);
  drawBackground(dt);
  ctx.save();
  ctx.translate(canvas.width * 0.5, canvas.height * 0.5);
  ctx.scale(GAME.camera.zoom, GAME.camera.zoom);
  ctx.translate(-GAME.camera.x, -GAME.camera.y);
  drawDebris();
  drawAsteroids();
  drawPlanets();
  drawRockets();
  ctx.restore();

  requestAnimationFrame(tick);
}

function resizeCanvas() {
  const desktop = window.matchMedia('(min-width: 1041px)').matches;
  const viewportWidth = window.innerWidth;
  const viewportHeight = window.innerHeight;

  const cssWidth = Math.max(320, desktop ? viewportWidth - 32 : viewportWidth - 16);
  const cssHeight = Math.max(420, desktop ? viewportHeight - 190 : viewportHeight - 240);

  canvas.style.width = `${Math.floor(cssWidth)}px`;
  canvas.style.height = `${Math.floor(cssHeight)}px`;

  const nextWidth = Math.floor(cssWidth);
  const nextHeight = Math.floor(cssHeight);

  if (canvas.width !== nextWidth || canvas.height !== nextHeight) {
    canvas.width = nextWidth;
    canvas.height = nextHeight;
    generateLevel();
    clampCameraToBounds();
  }
}

canvas.addEventListener('pointerdown', (event) => {
  GAME.camera.isPanning = true;
  GAME.camera.panStartX = event.clientX;
  GAME.camera.panStartY = event.clientY;
  GAME.camera.cameraStartX = GAME.camera.x;
  GAME.camera.cameraStartY = GAME.camera.y;
  GAME.camera.movedDuringPan = false;
  canvas.setPointerCapture(event.pointerId);
});

canvas.addEventListener('pointermove', (event) => {
  if (!GAME.camera.isPanning) return;

  const dx = event.clientX - GAME.camera.panStartX;
  const dy = event.clientY - GAME.camera.panStartY;
  if (Math.abs(dx) > 3 || Math.abs(dy) > 3) {
    GAME.camera.movedDuringPan = true;
  }

  GAME.camera.x = GAME.camera.cameraStartX - dx / GAME.camera.zoom;
  GAME.camera.y = GAME.camera.cameraStartY - dy / GAME.camera.zoom;
  clampCameraToBounds();
});

canvas.addEventListener('pointerup', (event) => {
  if (!GAME.camera.isPanning) return;
  GAME.camera.isPanning = false;
  canvas.releasePointerCapture(event.pointerId);

  if (GAME.camera.movedDuringPan) {
    return;
  }

  const rect = canvas.getBoundingClientRect();
  const sx = ((event.clientX - rect.left) / rect.width) * canvas.width;
  const sy = ((event.clientY - rect.top) / rect.height) * canvas.height;
  const world = screenToWorld(sx, sy);

  for (const planet of GAME.planets) {
    if (Math.hypot(planet.x - world.x, planet.y - world.y) <= planet.radius) {
      GAME.lastClickedPlanetIndex = planet.index;
      launchRocket(planet);
      return;
    }
  }

  setStatus('Tap a colonized planet to launch');
});

canvas.addEventListener('pointercancel', () => {
  GAME.camera.isPanning = false;
});

canvas.addEventListener('wheel', (event) => {
  event.preventDefault();
  zoomCamera(event.deltaY < 0 ? 1.1 : 0.9);
  clampCameraToBounds();
}, { passive: false });

zoomInButton.addEventListener('click', () => {
  zoomCamera(1.18);
  clampCameraToBounds();
});

zoomOutButton.addEventListener('click', () => {
  zoomCamera(0.85);
  clampCameraToBounds();
});

resetButton.addEventListener('click', () => {
  GAME.level = 1;
  GAME.baseHumans = 42;
  GAME.passengersPerLaunch = 8;
  GAME.lastTime = 0;
  generateLevel();
  resetCameraToEarth();
});

window.addEventListener('resize', resizeCanvas);
resizeCanvas();
requestAnimationFrame(tick);
