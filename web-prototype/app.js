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
  planets: [],
  rockets: [],
  lastTime: 0,
  statusTimeout: null
};

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
  return {
    index,
    x,
    y,
    radius,
    angle: rand(0, Math.PI * 2),
    rotationSpeed: (Math.random() > 0.5 ? 1 : -1) * baseSpeed,
    population: index === 0 ? GAME.baseHumans : 0
  };
}

function canPlacePlanet(candidate, placed, minGap) {
  return placed.every((p) => dist(candidate, p) > candidate.radius + p.radius + minGap);
}

function generateLevel() {
  const padding = 75;
  const minGap = Math.max(32, 70 - GAME.level * 2.2);
  const targetCount = GAME.planetsPerLevel;

  GAME.planets = [];
  GAME.asteroids = [];
  GAME.rockets = [];

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
      const asteroid = { x: rand(50, canvas.width - 50), y: rand(50, canvas.height - 50), radius: rand(12, 19) };
      const blocksPlanet = GAME.planets.some((p) => dist(p, asteroid) < p.radius + asteroid.radius + 16);
      const overlapsAsteroid = GAME.asteroids.some((a) => dist(a, asteroid) < a.radius + asteroid.radius + 12);
      if (!blocksPlanet && !overlapsAsteroid) {
        GAME.asteroids.push(asteroid);
        break;
      }
      tries += 1;
    }
  }

  setStatus(`Level ${GAME.level} generated`);
  updateHud();
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
    x: planet.x + launchDir.x * (planet.radius + 8),
    y: planet.y + launchDir.y * (planet.radius + 8),
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

function drawBackgroundStars() {
  ctx.save();
  for (let i = 0; i < 120; i += 1) {
    const x = (i * 173) % canvas.width;
    const y = (i * 97) % canvas.height;
    const alpha = ((i * 13) % 100) / 170;
    ctx.fillStyle = `rgba(255,255,255,${alpha})`;
    ctx.fillRect(x, y, 1.6, 1.6);
  }
  ctx.restore();
}

function drawPlanets() {
  for (const planet of GAME.planets) {
    ctx.save();
    ctx.translate(planet.x, planet.y);

    const colonized = planet.population > 0;
    ctx.fillStyle = colonized ? '#34d399' : '#7080b5';
    ctx.beginPath();
    ctx.arc(0, 0, planet.radius, 0, Math.PI * 2);
    ctx.fill();

    ctx.strokeStyle = colonized ? 'rgba(143, 255, 213, 0.9)' : 'rgba(164, 182, 230, 0.55)';
    ctx.lineWidth = 2;
    ctx.stroke();

    const lx = Math.cos(planet.angle) * (planet.radius + 10);
    const ly = Math.sin(planet.angle) * (planet.radius + 10);
    ctx.strokeStyle = '#ffd966';
    ctx.lineWidth = 3;
    ctx.beginPath();
    ctx.moveTo(0, 0);
    ctx.lineTo(lx, ly);
    ctx.stroke();

    ctx.fillStyle = '#e9f1ff';
    ctx.font = 'bold 12px sans-serif';
    ctx.textAlign = 'center';
    ctx.fillText(`${planet.index + 1} (${planet.population})`, 0, 4);

    ctx.restore();
  }
}

function drawAsteroids() {
  ctx.fillStyle = '#b08863';
  for (const asteroid of GAME.asteroids) {
    ctx.beginPath();
    ctx.arc(asteroid.x, asteroid.y, asteroid.radius, 0, Math.PI * 2);
    ctx.fill();
  }
}

function drawRockets() {
  for (const rocket of GAME.rockets) {
    ctx.save();
    ctx.translate(rocket.x, rocket.y);
    const angle = Math.atan2(rocket.vy, rocket.vx);
    ctx.rotate(angle);
    ctx.fillStyle = '#ffd966';
    ctx.beginPath();
    ctx.moveTo(8, 0);
    ctx.lineTo(-7, 4);
    ctx.lineTo(-4, 0);
    ctx.lineTo(-7, -4);
    ctx.closePath();
    ctx.fill();
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

  updateRockets(dt);
  checkProgression();

  ctx.clearRect(0, 0, canvas.width, canvas.height);
  drawBackgroundStars();
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
