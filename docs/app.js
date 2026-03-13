const canvas = document.getElementById('gameCanvas');
const ctx = canvas.getContext('2d');

const hud = {
  level: document.getElementById('level'),
  colonized: document.getElementById('colonized'),
  humans: document.getElementById('humans'),
  status: document.getElementById('status')
};

const resetButton = document.getElementById('resetButton');

const GAME = {
  level: 1,
  planetsPerLevel: 8,
  baseHumans: 42,
  passengersPerLaunch: 8,
  babyGrowthMultiplier: 0.35,
  planetMinRadius: 20,
  planetMaxRadius: 34,
  asteroids: [],
  debris: [],
  planets: [],
  rockets: [],
  lastTime: 0,
  statusTimeout: null,
  lastClickedPlanetIndex: null,
  bgScrollX: 0
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

function rand(min, max) {
  return Math.random() * (max - min) + min;
}

function dist(a, b) {
  return Math.hypot(a.x - b.x, a.y - b.y);
}

function setStatus(msg) {
  hud.status.textContent = msg;
}

function updateHud() {
  const colonizedCount = GAME.planets.filter((p) => p.population > 0).length;
  const totalHumans = GAME.planets.reduce((sum, p) => sum + p.population, 0);
  hud.level.textContent = String(GAME.level);
  hud.colonized.textContent = `${colonizedCount} / ${GAME.planetsPerLevel}`;
  hud.humans.textContent = String(totalHumans);
}

function makePlanet(index, x, y, radius, level) {
  const baseSpeed = rand(0.25, 0.7) + level * 0.07;
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
  const padding = 75;
  const minGap = Math.max(32, 70 - GAME.level * 2.2);
  const targetCount = GAME.planetsPerLevel;

  GAME.planets = [];
  GAME.asteroids = [];
  GAME.debris = [];
  GAME.rockets = [];
  GAME.lastClickedPlanetIndex = 0;

  for (let i = 0; i < targetCount; i += 1) {
    let tries = 0;
    while (tries < 500) {
      const radius = rand(GAME.planetMinRadius, GAME.planetMaxRadius);
      const candidate = {
        x: rand(padding, canvas.width - padding),
        y: rand(padding, canvas.height - padding),
        radius
      };
      if (canPlacePlanet(candidate, GAME.planets, minGap)) {
        GAME.planets.push(makePlanet(i, candidate.x, candidate.y, radius, GAME.level));
        break;
      }
      tries += 1;
    }
  }

  const asteroidCount = Math.min(2 + GAME.level, 10);
  for (let i = 0; i < asteroidCount; i += 1) {
    let tries = 0;
    while (tries < 300) {
      const asteroid = {
        x: rand(50, canvas.width - 50),
        y: rand(50, canvas.height - 50),
        radius: rand(12, 19),
        texturePath: DEBRIS_TEXTURE_CONFIG.rocky
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

  const debrisCountMin = Math.min(1 + GAME.level, 7);
  const debrisCountMax = Math.min(3 + GAME.level, 10);
  for (const planet of GAME.planets) {
    const debrisCount = Math.floor(rand(debrisCountMin, debrisCountMax + 1));
    for (let i = 0; i < debrisCount; i += 1) {
      const isAsteroid = i % 2 === 0;
      const pool = isAsteroid ? DEBRIS_ASTEROID_POOL : DEBRIS_SATELLITE_POOL;
      const texturePath = pool[Math.floor(Math.random() * pool.length)];
      GAME.debris.push({
        planetIndex: planet.index,
        angle: rand(0, Math.PI * 2),
        orbitRadius: planet.radius + rand(14, 30),
        orbitSpeed: rand(0.5, 1.8) * (Math.random() > 0.5 ? 1 : -1),
        size: isAsteroid ? rand(10, 18) : rand(12, 20),
        texturePath
      });
    }
  }

  setStatus(`Level ${GAME.level} generated`);
  updateHud();
}

function updateDebris(dt) {
  for (const piece of GAME.debris) {
    piece.angle += piece.orbitSpeed * dt;
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

function updateRockets(dt) {
  for (let i = GAME.rockets.length - 1; i >= 0; i -= 1) {
    const rocket = GAME.rockets[i];
    rocket.x += rocket.vx * dt;
    rocket.y += rocket.vy * dt;
    rocket.life -= dt;

    if (rocket.life <= 0 || rocket.x < -20 || rocket.x > canvas.width + 20 || rocket.y < -20 || rocket.y > canvas.height + 20) {
      GAME.rockets.splice(i, 1);
      continue;
    }

    const hitAsteroid = GAME.asteroids.find((a) => dist(rocket, a) <= a.radius + 3);
    if (hitAsteroid) {
      setStatus('Rocket destroyed by asteroid');
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
  if (colonizedCount === GAME.planetsPerLevel) {
    GAME.level += 1;
    GAME.baseHumans = Math.max(28, GAME.baseHumans - 1);
    GAME.passengersPerLaunch = Math.max(4, GAME.passengersPerLaunch - 0.15);
    generateLevel();
    setStatus(`All planets colonized! Welcome to level ${GAME.level}`);
  }
}

function drawBackground(dt) {
  const bgAsset = loadTexture(BG_TEXTURE);
  GAME.bgScrollX = (GAME.bgScrollX + 12 * dt) % canvas.width;

  ctx.save();
  if (bgAsset.loaded) {
    const offsetX = -GAME.bgScrollX;
    // Draw twice side-by-side for seamless horizontal tile
    ctx.drawImage(bgAsset.image, offsetX, 0, canvas.width, canvas.height);
    ctx.drawImage(bgAsset.image, offsetX + canvas.width, 0, canvas.width, canvas.height);
  } else {
    ctx.fillStyle = '#070a13';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    // Fallback stars while image loads
    for (let i = 0; i < 120; i += 1) {
      const x = (i * 173) % canvas.width;
      const y = (i * 97) % canvas.height;
      const alpha = ((i * 13) % 100) / 170;
      ctx.fillStyle = `rgba(255,255,255,${alpha})`;
      ctx.fillRect(x, y, 1.6, 1.6);
    }
  }
  ctx.restore();
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

      ctx.strokeStyle = 'rgba(255, 40, 40, 0.21)';
      ctx.lineWidth = 2;
      ctx.beginPath();
      ctx.moveTo(launchStartWorld.x - planet.x, launchStartWorld.y - planet.y);
      ctx.lineTo(launchEndWorld.x - planet.x, launchEndWorld.y - planet.y);
      ctx.stroke();

      drawLaunchMarker(planet);
    }

    ctx.fillStyle = '#e9f1ff';
    ctx.font = 'bold 12px sans-serif';
    ctx.textAlign = 'center';
    ctx.fillText(`🧑 ${planet.population}`, 0, 4);

    ctx.restore();
  }
}

function drawAsteroids() {
  for (const asteroid of GAME.asteroids) {
    drawTexturedCircle(asteroid.x, asteroid.y, asteroid.radius, asteroid.texturePath, '#8b6a53');
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
    const angle = Math.atan2(rocket.vy, rocket.vx);
    ctx.rotate(angle);
    if (rocketAsset.loaded) {
      const rw = 28;
      const rh = 16;
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

  updateRockets(dt);
  checkProgression();

  ctx.clearRect(0, 0, canvas.width, canvas.height);
  drawBackground(dt);
  drawDebris();
  drawAsteroids();
  drawPlanets();
  drawRockets();

  requestAnimationFrame(tick);
}

canvas.addEventListener('click', (event) => {
  const rect = canvas.getBoundingClientRect();
  const x = ((event.clientX - rect.left) / rect.width) * canvas.width;
  const y = ((event.clientY - rect.top) / rect.height) * canvas.height;

  for (const planet of GAME.planets) {
    if (Math.hypot(planet.x - x, planet.y - y) <= planet.radius) {
      GAME.lastClickedPlanetIndex = planet.index;
      launchRocket(planet);
      return;
    }
  }

  setStatus('Tap a colonized planet to launch');
});

resetButton.addEventListener('click', () => {
  GAME.level = 1;
  GAME.baseHumans = 42;
  GAME.passengersPerLaunch = 8;
  GAME.lastTime = 0;
  generateLevel();
});

generateLevel();
requestAnimationFrame(tick);
