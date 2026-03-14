const canvas = document.getElementById('gameCanvas');
const ctx = canvas.getContext('2d');

const hud = {
  level: document.getElementById('level'),
  timeRemaining: document.getElementById('timeRemaining'),
  colonized: document.getElementById('colonized'),
  humans: document.getElementById('humans'),
  status: document.getElementById('status')
};

const resetButton = document.getElementById('resetButton');
const zoomInButton = document.getElementById('zoomInButton');
const zoomOutButton = document.getElementById('zoomOutButton');
const tuneButton = document.getElementById('tuneButton');
const tweakPanel = document.getElementById('tweakPanel');
const closeTweakPanelButton = document.getElementById('closeTweakPanelButton');
const applyTweaksButton = document.getElementById('applyTweaksButton');
const resetTweaksButton = document.getElementById('resetTweaksButton');
const jumpLevelButton = document.getElementById('jumpLevelButton');
const jumpLevelInput = document.getElementById('jumpLevelInput');
const tweakInputs = Array.from(document.querySelectorAll('[data-tweak-key]'));

const DEFAULT_TWEAKS = {
  colonizedPitchStep: 0.05,
  colonizedPitchMax: 1.45,
  procreationAtLanding: 4,
  minGapBase: 36,
  minGapPerLevel: 8,
  minGapMaxLevel: 12,
  systemRadiusBaseMultiplier: 0.55,
  systemRadiusGrowthPerLevel: 0.13,
  systemRadiusMaxMultiplier: 2.1,
  earthEdgeRampLevels: 9,
  minRotationBase: 0.75,
  maxRotationBase: 1,
  rotationGrowthQuadratic: 0.01,
  radiusShrinkPerLevel: 0.04,
  radiusShrinkMin: 0.48,
  radiusVarianceGrowth: 0.03,
  radiusVarianceMax: 1.45,
  radiusBiasGrowth: 0.12,
  radiusBiasMax: 2.4,
  debrisStartLevel: 2,
  debrisSparseEndLevel: 5,
  debrisSparsePlanetChance: 0.5,
  debrisSparseMin: 0,
  debrisSparseMax: 1,
  debrisFullStartLevel: 6,
  debrisLateStartLevel: 10,
  debrisFullMin: 1,
  debrisFullMax: 2,
  debrisLateMin: 2,
  debrisLateMaxBase: 3,
  debrisLateGrowthPerLevels: 4
};

const TWEAKS = { ...DEFAULT_TWEAKS };

const GAME = {
  level: 1,
  planetsPerLevel: 8,
  requiredPlanetsForPortal: 1,
  levelColonizations: 0,
  levelState: 'colonizing',
  baseHumans: 42,
  landedPlanetTimeAward: 3,
  levelDuration: 0,
  timeRemaining: 0,
  passengersPerLaunch: 2,
  babyGrowthMultiplier: 0.35,
  planetMinRadius: 35,
  planetMaxRadius: 59.5,
  asteroids: [],
  debris: [],
  planets: [],
  rockets: [],
  portal: null,
  portalTransition: null,
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

function shouldShowLaserGuides() {
  if (SHOW_LASER) return true;
  return Boolean(window.ManifestPowerups?.shouldShowLaserGuides?.());
}

const SOUND_CONFIG = {
  planetSelect: { path: 'assets/sound/Swoosh.mp3', volume: 0.45 },
  launch: { path: 'assets/sound/firelaunch.wav', volume: 0.52 },
  humanDeath: { path: 'assets/sound/People Crash Sound.wav', volume: 0.12 },
  asteroidCrash: { path: 'assets/sound/crash.wav', volume: 0.1 },
  colonized: { path: 'assets/sound/WinFX.wav', volume: 0.5 },
  reinforced: { path: 'assets/sound/pop.mp3', volume: 0.4 },
  levelWin: { path: 'assets/sound/hurray.wav', volume: 0.275 },
  portalEnter: { path: 'assets/sound/portal.wav', volume: 0.5 },
  bgmOminous: { path: 'assets/sound/ominous_sounds.mp3', volume: 0.09, loop: true, allowOverlap: false },
  bgmConquer: { path: 'assets/sound/conquerTheFurther.wav', volume: 0.09, loop: false, allowOverlap: false },
  bgmSystematic: { path: 'assets/sound/systematic_great_for_technological_modern_applications_with_synth_elements_and_funky_guitar.mp3', volume: 0.09, loop: false, allowOverlap: false }
};

const PORTAL_DISTANCE_FROM_FURTHEST_PLANET = 42;
const PORTAL_PLANET_CLEARANCE = 12;
const PORTAL_PLACEMENT_STEP = 20;
const PORTAL_PLACEMENT_ATTEMPTS = 30;
const MAX_PLANETS_PER_LEVEL = 9;
const MAX_VISIBLE_SPREAD_RATIO = 0.86;

const BGM_POST_LEVEL_THREE_SEQUENCE = ['bgmConquer', 'bgmSystematic'];

const SOUND_BANK = new Map();
const SOUND_STATE = {
  enabled: false,
  masterVolume: 0.86,
  currentBgmName: null,
  postLevelThreeIndex: 0
};

function isBgmName(name) {
  return name === 'bgmOminous' || name === 'bgmConquer' || name === 'bgmSystematic';
}

function stopAllBgm(resetPosition = false) {
  for (const name of ['bgmOminous', 'bgmConquer', 'bgmSystematic']) {
    const entry = SOUND_BANK.get(name);
    if (!entry) continue;
    entry.base.pause();
    if (resetPosition) {
      entry.base.currentTime = 0;
    }
  }
  if (resetPosition) {
    SOUND_STATE.currentBgmName = null;
  }
}

function startBgm(name, restart = false) {
  if (!SOUND_STATE.enabled) return;
  const entry = SOUND_BANK.get(name);
  if (!entry) return;
  if (SOUND_STATE.currentBgmName === name && !restart && !entry.base.paused) return;

  stopAllBgm(false);
  if (restart || SOUND_STATE.currentBgmName !== name) {
    entry.base.currentTime = 0;
  }
  entry.base.loop = Boolean(entry.config.loop);
  entry.base.volume = clamp((entry.config.volume ?? 1) * SOUND_STATE.masterVolume, 0, 1);
  SOUND_STATE.currentBgmName = name;

  const playPromise = entry.base.play();
  if (playPromise && typeof playPromise.catch === 'function') {
    playPromise.catch(() => {});
  }
}

function playNextPostLevelThreeBgm(restart = false) {
  const nextName = BGM_POST_LEVEL_THREE_SEQUENCE[SOUND_STATE.postLevelThreeIndex % BGM_POST_LEVEL_THREE_SEQUENCE.length];
  SOUND_STATE.postLevelThreeIndex = (SOUND_STATE.postLevelThreeIndex + 1) % BGM_POST_LEVEL_THREE_SEQUENCE.length;
  startBgm(nextName, restart);
}

function initializeSoundBank() {
  for (const [name, config] of Object.entries(SOUND_CONFIG)) {
    const base = new Audio(config.path);
    base.preload = 'auto';
    base.loop = Boolean(config.loop);
    base.volume = config.volume ?? 1;
    if (name === 'bgmConquer' || name === 'bgmSystematic') {
      base.addEventListener('ended', () => {
        if (!SOUND_STATE.enabled || GAME.level <= 3) return;
        playNextPostLevelThreeBgm();
      });
    }
    SOUND_BANK.set(name, { base, config });
  }
}

function unlockAudio() {
  SOUND_STATE.enabled = true;
  for (const { base } of SOUND_BANK.values()) {
    base.load();
  }
  syncThreatAmbient();
}

function playSound(name, overrides = {}) {
  if (!SOUND_STATE.enabled) return;
  if (isBgmName(name)) {
    startBgm(name, Boolean(overrides.restart));
    return;
  }
  const entry = SOUND_BANK.get(name);
  if (!entry) return;

  const { base, config } = entry;
  const instance = config.allowOverlap === false ? base : base.cloneNode();
  instance.loop = overrides.loop ?? Boolean(config.loop);
  instance.playbackRate = overrides.rate ?? 1;
  instance.volume = clamp((overrides.volume ?? (config.volume ?? 1)) * SOUND_STATE.masterVolume, 0, 1);
  instance.currentTime = 0;

  const playPromise = instance.play();
  if (playPromise && typeof playPromise.catch === 'function') {
    playPromise.catch(() => {});
  }
}

function syncThreatAmbient() {
  if (!SOUND_STATE.enabled) {
    stopAllBgm(false);
    return;
  }

  if (GAME.level <= 3) {
    SOUND_STATE.postLevelThreeIndex = 0;
    startBgm('bgmOminous');
    return;
  }

  if (SOUND_STATE.currentBgmName === 'bgmConquer' || SOUND_STATE.currentBgmName === 'bgmSystematic') {
    const currentEntry = SOUND_BANK.get(SOUND_STATE.currentBgmName);
    if (currentEntry && !currentEntry.base.paused) return;
  }

  playNextPostLevelThreeBgm(true);
}

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
  updateTimerHud();
}

function getLevelDurationForLevel(level) {
  const systemIndex = Math.max(0, level - 1);
  return 25 + systemIndex * 2;
}

function formatTime(seconds) {
  const clampedSeconds = Math.max(0, Math.floor(seconds));
  const min = Math.floor(clampedSeconds / 60);
  const sec = clampedSeconds % 60;
  return `${min}:${String(sec).padStart(2, '0')}`;
}

function updateTimerHud() {
  if (!hud.timeRemaining) return;
  hud.timeRemaining.textContent = formatTime(GAME.timeRemaining);
  hud.timeRemaining.classList.toggle('timer-urgent', GAME.timeRemaining < 20);
}

function applyTweaksFromUi() {
  for (const input of tweakInputs) {
    const key = input.dataset.tweakKey;
    if (!key || !(key in TWEAKS)) continue;
    const parsed = Number(input.value);
    if (!Number.isFinite(parsed)) continue;
    TWEAKS[key] = parsed;
  }

  // Coerce related values into valid ranges.
  TWEAKS.debrisFullStartLevel = Math.max(TWEAKS.debrisStartLevel + 1, TWEAKS.debrisFullStartLevel);
  TWEAKS.debrisSparseEndLevel = clamp(
    TWEAKS.debrisSparseEndLevel,
    TWEAKS.debrisStartLevel,
    TWEAKS.debrisFullStartLevel - 1
  );
  TWEAKS.debrisLateStartLevel = Math.max(TWEAKS.debrisFullStartLevel + 1, TWEAKS.debrisLateStartLevel);
  TWEAKS.debrisFullMin = Math.max(0, TWEAKS.debrisFullMin);
  TWEAKS.debrisFullMax = Math.max(TWEAKS.debrisFullMin, TWEAKS.debrisFullMax);
  TWEAKS.debrisSparsePlanetChance = clamp(TWEAKS.debrisSparsePlanetChance, 0, 1);
  TWEAKS.colonizedPitchStep = Math.max(0, TWEAKS.colonizedPitchStep);
  TWEAKS.colonizedPitchMax = Math.max(1, TWEAKS.colonizedPitchMax);
  TWEAKS.procreationAtLanding = Math.max(0, Math.floor(TWEAKS.procreationAtLanding));
  TWEAKS.earthEdgeRampLevels = Math.max(1, TWEAKS.earthEdgeRampLevels);
  TWEAKS.systemRadiusGrowthPerLevel = Math.max(0, TWEAKS.systemRadiusGrowthPerLevel);
  TWEAKS.rotationGrowthQuadratic = Math.max(0, TWEAKS.rotationGrowthQuadratic);
}

function syncTweakInputsToState() {
  for (const input of tweakInputs) {
    const key = input.dataset.tweakKey;
    if (!key || !(key in TWEAKS)) continue;
    input.value = String(TWEAKS[key]);
  }
  jumpLevelInput.value = String(GAME.level);
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

  const requiredPlanets = Math.min(MAX_PLANETS_PER_LEVEL, Math.floor(3 + 0.75 * Math.pow(systemIndex, 1.15)));
  let targetPlanetCount = requiredPlanets;
  if (systemIndex > 0) {
    const inverseOpportunity = Math.max(2, -0.1 * (systemIndex / 0.5) + 5);
    const maxPlanets = Math.min(MAX_PLANETS_PER_LEVEL, requiredPlanets + Math.floor(inverseOpportunity));
    targetPlanetCount = randIntInclusive(requiredPlanets, Math.max(requiredPlanets, maxPlanets));
  }

  const potentialSpeedIncreaseRate = TWEAKS.rotationGrowthQuadratic * Math.pow(systemIndex, 2);
  const minPlanetRotationSpeed = clamp(TWEAKS.minRotationBase + potentialSpeedIncreaseRate, TWEAKS.minRotationBase, 4);
  const maxPlanetRotationSpeed = clamp(TWEAKS.maxRotationBase + potentialSpeedIncreaseRate, TWEAKS.maxRotationBase, 5);

  let minDebrisCount = 0;
  let maxDebrisCount = 0;
  let debrisPlanetChance = 0;
  if (level < TWEAKS.debrisStartLevel) {
    minDebrisCount = 0;
    maxDebrisCount = 0;
    debrisPlanetChance = 0;
  } else if (level <= TWEAKS.debrisSparseEndLevel || level < TWEAKS.debrisFullStartLevel) {
    minDebrisCount = Math.max(0, Math.floor(TWEAKS.debrisSparseMin));
    maxDebrisCount = Math.max(minDebrisCount, Math.floor(TWEAKS.debrisSparseMax));
    debrisPlanetChance = clamp(TWEAKS.debrisSparsePlanetChance, 0, 1);
  } else if (level < TWEAKS.debrisLateStartLevel) {
    minDebrisCount = Math.max(0, Math.floor(TWEAKS.debrisFullMin));
    maxDebrisCount = Math.max(minDebrisCount, Math.floor(TWEAKS.debrisFullMax));
    debrisPlanetChance = 1;
  } else {
    minDebrisCount = Math.max(0, Math.floor(TWEAKS.debrisLateMin));
    const lateGrowth = Math.floor((level - TWEAKS.debrisLateStartLevel) / Math.max(1, TWEAKS.debrisLateGrowthPerLevels));
    maxDebrisCount = clamp(Math.floor(TWEAKS.debrisLateMaxBase) + lateGrowth, minDebrisCount + 1, 6);
    debrisPlanetChance = 1;
  }

  const asteroidCount = systemIndex === 0 ? 0 : clamp(Math.floor(1 + systemIndex * 0.85), 1, 10);
  const obstacleSpinMultiplier = 0.68 + systemIndex * 0.045;
  const debrisSpeedRamp = clamp((level - 1) / 12, 0, 1);
  const minDebrisOrbitSpeed = 0.18 + debrisSpeedRamp * 0.32;
  const maxDebrisOrbitSpeed = 0.42 + debrisSpeedRamp * 0.68;
  const earthEdgeBias = clamp(systemIndex / Math.max(1, TWEAKS.earthEdgeRampLevels), 0, 1);
  const minPlanetGap = TWEAKS.minGapBase + Math.min(level, TWEAKS.minGapMaxLevel) * TWEAKS.minGapPerLevel;

  // Start with broad size variance and bias more toward smaller planets at higher levels.
  const radiusShrink = clamp(1 - systemIndex * TWEAKS.radiusShrinkPerLevel, TWEAKS.radiusShrinkMin, 1.08);
  const radiusVariance = clamp(1 + systemIndex * TWEAKS.radiusVarianceGrowth, 1, TWEAKS.radiusVarianceMax);
  const minRadiusMultiplier = clamp(0.78 * radiusShrink, 0.35, 1.1);
  const maxRadiusMultiplier = clamp(1.2 * radiusShrink * radiusVariance, minRadiusMultiplier + 0.25, 1.9);
  const radiusBiasPower = clamp(1 + systemIndex * TWEAKS.radiusBiasGrowth, 1, TWEAKS.radiusBiasMax);

  // Allow systems to grow larger than the viewport so zooming out becomes important.
  const systemRadiusMultiplier = clamp(
    TWEAKS.systemRadiusBaseMultiplier + systemIndex * TWEAKS.systemRadiusGrowthPerLevel,
    TWEAKS.systemRadiusBaseMultiplier,
    Math.min(TWEAKS.systemRadiusMaxMultiplier, 1.1)
  );

  return {
    systemIndex,
    requiredPlanets,
    targetPlanetCount,
    minPlanetRotationSpeed,
    maxPlanetRotationSpeed,
    minDebrisCount,
    maxDebrisCount,
    debrisPlanetChance,
    asteroidCount,
    obstacleSpinMultiplier,
    earthEdgeBias,
    minDebrisOrbitSpeed,
    maxDebrisOrbitSpeed,
    minPlanetGap,
    minRadiusMultiplier,
    maxRadiusMultiplier,
    radiusBiasPower,
    systemRadiusMultiplier
  };
}

function pickEarthPosition(radius, levelParams, systemRadius) {
  const center = { x: canvas.width * 0.5, y: canvas.height * 0.5 };
  if (levelParams.earthEdgeBias <= 0) {
    return center;
  }

  const edgeDirections = [
    { x: 1, y: 0 },
    { x: 0, y: 1 },
    { x: -1, y: 0 },
    { x: 0, y: -1 }
  ];
  const direction = edgeDirections[Math.floor(levelParams.systemIndex / 3) % edgeDirections.length];
  const offset = systemRadius * 0.62 * levelParams.earthEdgeBias;

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

  const scaledMinRadius = GAME.planetMinRadius * planetScale * levelParams.minRadiusMultiplier;
  const scaledMaxRadius = GAME.planetMaxRadius * planetScale * levelParams.maxRadiusMultiplier;
  const padding = Math.max(110, 110 * planetScale);
  const minGap = levelParams.minPlanetGap * planetScale;
  const systemRadius = Math.min(canvas.width, canvas.height) * levelParams.systemRadiusMultiplier;
  const targetCount = levelParams.targetPlanetCount;
  GAME.planetsPerLevel = targetCount;

  GAME.planets = [];
  GAME.asteroids = [];
  GAME.debris = [];
  GAME.rockets = [];
  GAME.portal = null;
  GAME.portalTransition = null;
  GAME.lastClickedPlanetIndex = 0;
  GAME.levelColonizations = 0;
  GAME.levelState = 'colonizing';
  GAME.levelDuration = getLevelDurationForLevel(GAME.level);
  GAME.timeRemaining = GAME.levelDuration;

  const earthRadius = rand(Math.max(22, scaledMinRadius), Math.max(26, scaledMinRadius * 1.18));
  const earthPosition = pickEarthPosition(earthRadius, levelParams, systemRadius);
  GAME.planets.push(makePlanet(0, earthPosition.x, earthPosition.y, earthRadius, levelParams));

  const edgeLimit = Math.max(60, Math.min(
    earthPosition.x - padding - scaledMaxRadius,
    canvas.width - earthPosition.x - padding - scaledMaxRadius,
    earthPosition.y - padding - scaledMaxRadius,
    canvas.height - earthPosition.y - padding - scaledMaxRadius
  ));

  const spreadRamp = clamp((GAME.level - 1) / 10, 0, 1);
  let maxSpreadDistance = systemRadius * (0.66 + spreadRamp * 0.46);
  let minSpreadDistance = maxSpreadDistance * (0.34 + spreadRamp * 0.46);

  // Keep planetary spread visible in the viewport envelope for this level.
  maxSpreadDistance = Math.min(maxSpreadDistance, edgeLimit * MAX_VISIBLE_SPREAD_RATIO);

  const minAllowedDistance = earthRadius + scaledMaxRadius + minGap;
  maxSpreadDistance = Math.max(maxSpreadDistance, minAllowedDistance + 24);
  minSpreadDistance = clamp(minSpreadDistance, minAllowedDistance, Math.max(minAllowedDistance, maxSpreadDistance - 24));

  const center = { x: canvas.width * 0.5, y: canvas.height * 0.5 };
  const awayFromEarthAngle = Math.atan2(center.y - earthPosition.y, center.x - earthPosition.x);
  const spreadArcHalfWidth = Math.PI * (0.85 - spreadRamp * 0.45);

  for (let i = 1; i < targetCount; i += 1) {
    let tries = 0;
    while (tries < 500) {
      const radiusLerp = Math.pow(Math.random(), levelParams.radiusBiasPower);
      const radius = scaledMinRadius + (scaledMaxRadius - scaledMinRadius) * radiusLerp;
      const spawnDistance = rand(minSpreadDistance, maxSpreadDistance);
      const spawnAngle = levelParams.earthEdgeBias > 0.15
        ? awayFromEarthAngle + rand(-spreadArcHalfWidth, spreadArcHalfWidth)
        : rand(0, Math.PI * 2);
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

  const worldBodies = [...GAME.planets, ...GAME.asteroids];
  const leftEdge = Math.min(...worldBodies.map((body) => body.x - body.radius));
  const rightEdge = Math.max(...worldBodies.map((body) => body.x + body.radius));
  const topEdge = Math.min(...worldBodies.map((body) => body.y - body.radius));
  const bottomEdge = Math.max(...worldBodies.map((body) => body.y + body.radius));
  GAME.worldBounds = {
    left: leftEdge - worldPadding,
    right: rightEdge + worldPadding,
    top: topEdge - worldPadding,
    bottom: bottomEdge + worldPadding
  };

  const debrisCountMin = levelParams.minDebrisCount;
  const debrisCountMax = levelParams.maxDebrisCount;
  for (const planet of GAME.planets) {
    const shouldSpawnDebris = debrisCountMax > 0 && Math.random() <= levelParams.debrisPlanetChance;
    const debrisCount = shouldSpawnDebris ? randIntInclusive(debrisCountMin, debrisCountMax) : 0;
    for (let i = 0; i < debrisCount; i += 1) {
      const isAsteroid = i % 2 === 0;
      const pool = isAsteroid ? DEBRIS_ASTEROID_POOL : DEBRIS_SATELLITE_POOL;
      const texturePath = pool[Math.floor(Math.random() * pool.length)];
      GAME.debris.push({
        planetIndex: planet.index,
        angle: rand(0, Math.PI * 2),
        orbitRadius: planet.radius + rand(14, 30),
        orbitSpeed: rand(levelParams.minDebrisOrbitSpeed, levelParams.maxDebrisOrbitSpeed)
          * levelParams.obstacleSpinMultiplier
          * (Math.random() > 0.5 ? 1 : -1),
        size: (isAsteroid ? rand(10, 18) : rand(12, 20)) * 1.25,
        texturePath
      });
    }
  }

  // Use actual spawned count so level completion cannot become unreachable.
  GAME.planetsPerLevel = GAME.planets.length;
  GAME.requiredPlanetsForPortal = GAME.planetsPerLevel;

  resetCameraToEarth();
  setStatus(`Level ${GAME.level} generated`);
  updateHud();
  syncThreatAmbient();
  window.ManifestPowerups?.onLevelChanged?.(GAME.level);
}

function failCurrentLevel(reason) {
  const failedLevel = GAME.level;
  GAME.levelState = 'lost';
  GAME.portal = null;
  GAME.portalTransition = null;
  GAME.rockets = [];

  if (reason === 'time') {
    setStatus(`Time expired. System ${failedLevel} lost, retrying...`);
  } else {
    setStatus(`All humans lost. System ${failedLevel} lost, retrying...`);
  }

  generateLevel();
}

function updateLevelTimer(dt) {
  if (GAME.levelState !== 'colonizing' && GAME.levelState !== 'locatingPortal') {
    return;
  }

  GAME.timeRemaining = Math.max(0, GAME.timeRemaining - dt);
  updateTimerHud();
}

function checkFailConditions() {
  if (GAME.levelState !== 'colonizing' && GAME.levelState !== 'locatingPortal') {
    return;
  }

  const totalHumans = GAME.planets.reduce((sum, p) => sum + p.population, 0)
    + GAME.rockets.reduce((sum, r) => sum + (r.passengers || 0), 0);

  if (GAME.timeRemaining <= 0) {
    failCurrentLevel('time');
    return;
  }

  if (totalHumans <= 0) {
    failCurrentLevel('humans');
  }
}

function getPortalPosition() {
  if (!GAME.planets.length) return null;
  const home = GAME.planets[0];
  let furthest = home;
  let furthestDistance = -1;

  for (const planet of GAME.planets) {
    const distanceFromHome = Math.hypot(planet.x - home.x, planet.y - home.y);
    if (distanceFromHome > furthestDistance) {
      furthest = planet;
      furthestDistance = distanceFromHome;
    }
  }

  let directionX;
  let directionY;
  if (furthestDistance <= 1e-6) {
    directionX = 1;
    directionY = 0;
    furthestDistance = 0;
  } else {
    const dx = furthest.x - home.x;
    const dy = furthest.y - home.y;
    const invDistance = 1 / furthestDistance;
    directionX = dx * invDistance;
    directionY = dy * invDistance;
  }

  const portalCollisionRadius = 15;
  let placementDistance = furthestDistance + PORTAL_DISTANCE_FROM_FURTHEST_PLANET;

  for (let i = 0; i < PORTAL_PLACEMENT_ATTEMPTS; i += 1) {
    const candidate = {
      x: home.x + directionX * placementDistance,
      y: home.y + directionY * placementDistance
    };

    const overlapsPlanet = GAME.planets.some((planet) => {
      const minDistance = planet.radius + portalCollisionRadius + PORTAL_PLANET_CLEARANCE;
      return Math.hypot(candidate.x - planet.x, candidate.y - planet.y) < minDistance;
    });

    if (!overlapsPlanet) {
      return candidate;
    }

    placementDistance += PORTAL_PLACEMENT_STEP;
  }

  return {
    x: home.x + directionX * placementDistance,
    y: home.y + directionY * placementDistance
  };
}

function spawnPortalForCurrentLevel() {
  if (GAME.portal) return;
  const portalPosition = getPortalPosition();
  if (!portalPosition) return;

  GAME.portal = {
    x: portalPosition.x,
    y: portalPosition.y,
    radiusX: 15,
    radiusY: 24,
    collisionRadius: 15,
    angle: 0,
    spinSpeed: 3.6,
    innerSpinSpeed: -5.2,
    shearingAngle: 0,
    shearingSpeed: 1.8,
    pulse: 0,
    pulseSpeed: 3.1,
    collapse: 0
  };

  setStatus('Portal located. Send a rocket through it to reach the next system.');
}

function finalizePortalTransit() {
  GAME.portal = null;
  GAME.portalTransition = null;
  GAME.level += 1;
  GAME.baseHumans = Math.max(28, GAME.baseHumans - 1);
  GAME.passengersPerLaunch = 2;
  generateLevel();
  setStatus(`Portal transit complete. Welcome to level ${GAME.level}`);
}

function beginPortalTransit(rocket) {
  if (!GAME.portal || GAME.levelState === 'transitioning') return;

  GAME.levelState = 'transitioning';
  GAME.portalTransition = {
    timer: 0,
    duration: 0.95,
    rocket: {
      x: rocket.x,
      y: rocket.y,
      vx: rocket.vx,
      vy: rocket.vy
    }
  };
  playSound('portalEnter');
  setStatus('Portal engaged. Stabilizing interstellar transit...');
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

function updatePortal(dt) {
  if (!GAME.portal) return;

  if (GAME.portalTransition) {
    const transition = GAME.portalTransition;
    transition.timer = Math.min(transition.duration, transition.timer + dt);
    const progress = transition.timer / transition.duration;
    GAME.portal.collapse = progress;
    GAME.portal.spinSpeed = 3.6 + progress * 6.8;
    GAME.portal.pulseSpeed = 3.1 + progress * 4.6;

    const travel = transition.rocket;
    const toPortalX = GAME.portal.x - travel.x;
    const toPortalY = GAME.portal.y - travel.y;
    travel.vx = travel.vx * (1 - dt * 5.2) + toPortalX * dt * 14;
    travel.vy = travel.vy * (1 - dt * 5.2) + toPortalY * dt * 14;
    travel.x += travel.vx * dt;
    travel.y += travel.vy * dt;

    if (progress >= 1) {
      playSound('levelWin');
      finalizePortalTransit();
      return;
    }
  }

  GAME.portal.angle += GAME.portal.spinSpeed * dt;
  GAME.portal.shearingAngle += GAME.portal.shearingSpeed * dt;
  GAME.portal.pulse += GAME.portal.pulseSpeed * dt;
}

function launchRocket(planet) {
  if (GAME.levelState === 'transitioning') {
    setStatus('Transit in progress. Launch systems are temporarily locked.');
    return;
  }

  if (planet.population <= 0) {
    setStatus('No population to launch from this planet');
    return;
  }

  const livingPlanets = GAME.planets.filter((p) => p.population > 0);
  if (livingPlanets.length === GAME.planetsPerLevel && !GAME.portal) return;

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

  playSound('launch');
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
  playSound('humanDeath');
  if (reason === 'Asteroid') {
    playSound('asteroidCrash');
  }
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

    if (GAME.portal && dist(rocket, GAME.portal) <= GAME.portal.collisionRadius + ROCKET_COLLISION_RADIUS) {
      beginPortalTransit(rocket);
      GAME.rockets.splice(i, 1);
      continue;
    }

    const target = GAME.planets.find((p) => p.index !== rocket.sourceIndex && dist(rocket, p) <= p.radius);
    if (target) {
      const wasVirgin = target.population === 0;
      target.population += rocket.passengers;
      if (wasVirgin) {
        const newborns = TWEAKS.procreationAtLanding;
        target.population += newborns;
        GAME.timeRemaining += GAME.landedPlanetTimeAward;
        const colonizedPitch = clamp(
          1 + GAME.levelColonizations * TWEAKS.colonizedPitchStep,
          1,
          TWEAKS.colonizedPitchMax
        );
        playSound('colonized', { rate: colonizedPitch });
        GAME.levelColonizations += 1;
        setStatus(`Planet ${target.index + 1} colonized (+${rocket.passengers}, babies +${newborns}, time +${GAME.landedPlanetTimeAward}s)`);
      } else {
        playSound('reinforced');
        setStatus(`Reinforced planet ${target.index + 1}`);
      }
      GAME.rockets.splice(i, 1);
      updateHud();
    }
  }
}

function checkProgression() {
  const colonizedCount = GAME.planets.filter((p) => p.population > 0).length;
  if (
    GAME.levelState === 'colonizing'
    && GAME.planetsPerLevel > 0
    && colonizedCount >= GAME.planetsPerLevel
  ) {
    GAME.levelState = 'locatingPortal';
    spawnPortalForCurrentLevel();
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

      if (shouldShowLaserGuides()) {
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

function drawPortal() {
  const portal = GAME.portal;
  if (!portal) return;

  const collapse = portal.collapse ?? 0;
  const collapseScale = 1 - collapse * 0.78;
  const alphaScale = 1 - collapse * 0.55;
  const pulse = Math.sin(portal.pulse) * 0.5 + 0.5;
  const glowRadiusX = (portal.radiusX + 12 + pulse * 8) * collapseScale;
  const glowRadiusY = (portal.radiusY + 16 + pulse * 10) * collapseScale;
  const coreRadiusX = portal.radiusX * collapseScale;
  const coreRadiusY = portal.radiusY * collapseScale;

  ctx.save();
  ctx.translate(portal.x, portal.y);

  ctx.rotate(portal.angle);
  ctx.globalCompositeOperation = 'lighter';
  ctx.strokeStyle = `rgba(80, 232, 255, ${0.95 * alphaScale})`;
  ctx.lineWidth = 5;
  ctx.shadowColor = `rgba(40, 210, 255, ${0.95 * alphaScale})`;
  ctx.shadowBlur = 28;
  ctx.beginPath();
  ctx.ellipse(0, 0, coreRadiusX, coreRadiusY, 0, 0, Math.PI * 2);
  ctx.stroke();

  ctx.shadowBlur = 18;
  ctx.strokeStyle = `rgba(169, 248, 255, ${0.82 * alphaScale})`;
  ctx.lineWidth = 2.6;
  ctx.beginPath();
  ctx.ellipse(0, 0, glowRadiusX, glowRadiusY, 0, 0, Math.PI * 2);
  ctx.stroke();

  ctx.rotate(portal.angle * 0.32 + portal.shearingAngle);
  ctx.strokeStyle = `rgba(112, 220, 255, ${0.8 * alphaScale})`;
  ctx.lineWidth = 2.8;
  ctx.beginPath();
  ctx.ellipse(0, 0, coreRadiusX * 0.7, coreRadiusY * 0.7, Math.sin(portal.shearingAngle) * 0.35, 0, Math.PI * 2);
  ctx.stroke();

  ctx.rotate(-portal.angle * 1.15);
  ctx.strokeStyle = `rgba(78, 196, 255, ${0.72 * alphaScale})`;
  ctx.lineWidth = 1.8;
  ctx.beginPath();
  ctx.ellipse(0, 0, coreRadiusX * 0.48, coreRadiusY * 0.48, 0, 0, Math.PI * 2);
  ctx.stroke();

  ctx.fillStyle = `rgba(130, 235, 255, ${0.18 * alphaScale})`;
  ctx.beginPath();
  ctx.ellipse(0, 0, coreRadiusX * 0.42, coreRadiusY * 0.42, 0, 0, Math.PI * 2);
  ctx.fill();

  if (GAME.portalTransition) {
    const progress = GAME.portalTransition.timer / GAME.portalTransition.duration;
    const burstAlpha = (1 - progress) * 0.9;
    const burstRadius = 8 + progress * 34;
    ctx.strokeStyle = `rgba(166, 245, 255, ${burstAlpha})`;
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.ellipse(0, 0, burstRadius * 0.7, burstRadius, 0, 0, Math.PI * 2);
    ctx.stroke();
  }

  ctx.restore();
}

function drawPortalTransitionRocket() {
  if (!GAME.portalTransition) return;

  const rocketAsset = loadTexture(ROCKET_TEXTURE);
  const travel = GAME.portalTransition.rocket;
  const progress = GAME.portalTransition.timer / GAME.portalTransition.duration;
  const alpha = clamp(1 - progress * 1.15, 0, 1);
  const scale = clamp(1 - progress * 0.65, 0.25, 1);

  ctx.save();
  ctx.globalAlpha = alpha;
  ctx.translate(travel.x, travel.y);
  const angle = Math.atan2(travel.vy, travel.vx) + ROCKET_SPRITE_ANGLE_OFFSET;
  ctx.rotate(angle);
  ctx.scale(scale, scale);

  if (rocketAsset.loaded) {
    const baseHeight = 30;
    const sourceWidth = rocketAsset.image.naturalWidth || rocketAsset.image.width || 1;
    const sourceHeight = rocketAsset.image.naturalHeight || rocketAsset.image.height || 1;
    const aspectRatio = sourceWidth / sourceHeight;
    const rh = baseHeight;
    const rw = rh * aspectRatio;
    ctx.drawImage(rocketAsset.image, -rw * 0.5, -rh * 0.5, rw, rh);
  } else {
    ctx.fillStyle = '#d5f8ff';
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

function tick(timestamp) {
  if (!GAME.lastTime) GAME.lastTime = timestamp;
  const dt = Math.min((timestamp - GAME.lastTime) / 1000, 0.032);
  GAME.lastTime = timestamp;

  for (const planet of GAME.planets) {
    planet.angle += planet.rotationSpeed * dt;
  }

  updateLevelTimer(dt);

  updateDebris(dt);
  updateAsteroids(dt);
  updatePortal(dt);

  updateRockets(dt);
  checkProgression();
  checkFailConditions();

  ctx.clearRect(0, 0, canvas.width, canvas.height);
  drawBackground(dt);
  ctx.save();
  ctx.translate(canvas.width * 0.5, canvas.height * 0.5);
  ctx.scale(GAME.camera.zoom, GAME.camera.zoom);
  ctx.translate(-GAME.camera.x, -GAME.camera.y);
  drawDebris();
  drawAsteroids();
  drawPlanets();
  drawPortal();
  drawRockets();
  drawPortalTransitionRocket();
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

  if (window.ManifestPowerups?.handlePointerUp?.(world)) {
    return;
  }

  for (const planet of GAME.planets) {
    if (Math.hypot(planet.x - world.x, planet.y - world.y) <= planet.radius) {
      GAME.lastClickedPlanetIndex = planet.index;
      if (planet.population > 0) {
        playSound('planetSelect', { volume: 0.36 });
      }
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
  GAME.levelColonizations = 0;
  GAME.baseHumans = 42;
  GAME.passengersPerLaunch = 2;
  GAME.lastTime = 0;
  generateLevel();
  resetCameraToEarth();
  syncThreatAmbient();
  syncTweakInputsToState();
});

tuneButton.addEventListener('click', () => {
  tweakPanel.hidden = !tweakPanel.hidden;
});

closeTweakPanelButton.addEventListener('click', () => {
  tweakPanel.hidden = true;
});

applyTweaksButton.addEventListener('click', () => {
  applyTweaksFromUi();
  generateLevel();
  resetCameraToEarth();
  syncTweakInputsToState();
});

resetTweaksButton.addEventListener('click', () => {
  Object.assign(TWEAKS, DEFAULT_TWEAKS);
  syncTweakInputsToState();
  generateLevel();
  resetCameraToEarth();
});

jumpLevelButton.addEventListener('click', () => {
  const requested = Number(jumpLevelInput.value);
  GAME.level = Number.isFinite(requested) ? Math.max(1, Math.floor(requested)) : GAME.level;
  GAME.lastTime = 0;
  generateLevel();
  resetCameraToEarth();
  syncTweakInputsToState();
});

window.ManifestSpaceApi = {
  getGame: () => GAME,
  getCanvas: () => canvas,
  dist,
  clamp,
  setStatus,
  playSound,
  screenToWorld,
  getDebrisWorldPosition,
  findPlanetAtWorld(worldPoint, colonizedOnly = false) {
    return GAME.planets.find((planet) => {
      if (colonizedOnly && planet.population <= 0) return false;
      return Math.hypot(planet.x - worldPoint.x, planet.y - worldPoint.y) <= planet.radius;
    }) || null;
  },
  findDebrisAtWorld(worldPoint) {
    return GAME.debris.find((piece) => {
      const debrisWorld = getDebrisWorldPosition(piece);
      if (!debrisWorld) return false;
      return dist(worldPoint, debrisWorld) <= piece.size * 0.5;
    }) || null;
  },
  removeDebrisPiece(piece) {
    const index = GAME.debris.indexOf(piece);
    if (index >= 0) {
      GAME.debris.splice(index, 1);
      return true;
    }
    return false;
  },
  removeAsteroid(asteroid) {
    const index = GAME.asteroids.indexOf(asteroid);
    if (index >= 0) {
      GAME.asteroids.splice(index, 1);
      return true;
    }
    return false;
  }
};

initializeSoundBank();
unlockAudio();
window.addEventListener('pointerdown', unlockAudio, { once: true });
window.addEventListener('keydown', unlockAudio, { once: true });
window.addEventListener('resize', resizeCanvas);
syncTweakInputsToState();
resizeCanvas();
requestAnimationFrame(tick);
