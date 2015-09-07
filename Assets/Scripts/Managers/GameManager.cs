using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject SpawnerObject;
    
    public int levelIndex;
    public float TotalScore;
    
    public SolarSystemSeed  CurrentLevel;
    public Vector2          CurrentHomePosition;
    public GameObject       CurrentSelectedPlanet;
    
    //this should go to the asteroid spawner script...
    public ArrayList        AsteroidThreatList;

    enum LevelState {Colonizing,LocatingPortal,Paused,Lost,Won};
    LevelState CurrentLevelState;
    LevelState _stateBeforePause;

    /// <summary>
    /// TODO: move to its own analytics utility
    /// </summary>
    /// 
    bool _playedWarning = false;
    int  _maxPlanets = 0;
	int  _maxHumans = 0;
    int _totalHumans = 0;
    int _totalPlanets = 0;
    int _totalSystems = 0;
    float _levelStartTime;
    float _timeLeft;

    public float TimeLeft{ get { return _timeLeft; }}
    public int TotalHumans  { get { return _totalHumans; }}
    public int TotalPlanets { get { return _totalPlanets; }}
    public int TotalSystems { get { return _totalSystems; }}

    public static GameManager SharedInstance { get { return GameManager.mInstance; }}
    private static GameManager mInstance;

    #region Unity Events
 
    void Awake()
    {
        mInstance = this;
    }
    
    public void OnEnable()
    {
        EventManager.StartListening(EventManager.eSolarSystemDidFinishSpawning, _solarSystemSpawned);
        EventManager.StartListening(EventManager.ePortalEnteredEvent, _levelWinHandler);
        EventManager.StartListening(EventManager.eNextHomeIsReadyEvent, _nextLevelReadyHandler);
        EventManager.StartListening(EventManager.eAsteroidSpawnedEvent, asteroidThreatBegan);
        EventManager.StartListening(EventManager.eAsteroidDestroyedEvent, asteroidThreadOver);
    }
    
    public void OnDisable()
    {
        EventManager.StopListening(EventManager.eSolarSystemDidFinishSpawning, _solarSystemSpawned);
        EventManager.StartListening(EventManager.ePortalEnteredEvent, _levelWinHandler);
        EventManager.StartListening(EventManager.eNextHomeIsReadyEvent, _nextLevelReadyHandler);
        EventManager.StopListening(EventManager.eAsteroidSpawnedEvent, asteroidThreatBegan);
        EventManager.StopListening(EventManager.eAsteroidDestroyedEvent, asteroidThreadOver);
    }
    
    public void Start()
    {
        CurrentLevelState = LevelState.Paused;
        AsteroidThreatList = new ArrayList();
        CurrentHomePosition = new Vector2(0, 0);
        levelIndex = 0;
        _initializeSolarSystem();
    }
    
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
            _levelWinHandler();

        if(CurrentLevelState == LevelState.Colonizing || CurrentLevelState == LevelState.LocatingPortal)
        {
            _timeLeft -= Time.deltaTime;
            UserInterface.SharedInstance.DisplayCurrentData(); //update User Interface
            _update_analytics();
            _update_levelcheck();
        }
    }

    public void _update_analytics()
    {
        if (CurrentLevel.HumanPopulation > _maxHumans)
            _maxHumans = CurrentLevel.HumanPopulation;

        if (CurrentLevel.ColonizedPlanetCount > _maxPlanets)
            _maxPlanets = CurrentLevel.ColonizedPlanetCount;
    }
    public void _update_levelcheck()
    {
        if (CurrentLevel.ColonizedPlanetCount >= CurrentLevel.RequiredPlanets && CurrentLevelState == LevelState.Colonizing)
        {
            CurrentLevelState = LevelState.LocatingPortal;
            MusicPlayer.SharedInstance.planetAchievementSound();
            UserInterface.SharedInstance.DisplayPlanetGoalAchievedImages(true);
            EventManager.PostEvent(EventManager.ePlanetsAquiredEvent);
        }
        if ((CurrentLevelState == LevelState.Colonizing || CurrentLevelState == LevelState.LocatingPortal) && (CurrentLevel.HumanPopulation <= 0) || _timeLeft<=0)
        {
            _levelLostHandler();
        }
    }
    
    #endregion

    #region Game Events

    void _levelWinHandler()
    {
        //      Analytics stuff     //
        _totalHumans += CurrentLevel.HumanPopulation;
        _totalPlanets += CurrentLevel.ColonizedPlanetCount;
        _totalSystems++;
        UserInterface.SharedInstance.LevelUI.SetActive(false);
        MusicPlayer.SharedInstance.playLevelWinSFX();
        UserInterface.SharedInstance.DisplaySessionEndedPanel(true, true);
        CurrentLevelState = LevelState.Won;
        int newLevel = levelIndex + 1;
        
        _transitionToNextLevel(newLevel);
    }
    
    void _levelLostHandler()
    {
        CurrentLevelState = LevelState.Lost;
        _totalHumans  += CurrentLevel.HumanPopulation;
        _totalPlanets += CurrentLevel.ColonizedPlanetCount;
        UserInterface.SharedInstance.DisplayPlanetGoalAchievedImages(false);
        UserInterface.SharedInstance.DisplaySessionEndedPanel(true, false);

        Debug.Log("Lost game, removing asteroids, closing portals, & other shenanigans");
        StartCoroutine(_removeAsteroidThreats());
        GameObject portal = SpawnerObject.GetComponent<PortalSpawner>().CurrentPortal;
        if (portal != null)
            Destroy(portal);
        _transitionToNextLevel(levelIndex);
    }
    
    void _nextLevelReadyHandler()
    {
        UserInterface.SharedInstance.LevelUI.SetActive(true);
        CurrentLevelState = LevelState.Colonizing;
        _levelStartTime = Time.time;
        _timeLeft = CurrentLevel.LevelDuration();
    }

    void _solarSystemSpawned()
    {
        if (levelIndex == 0 && CurrentLevelState!=LevelState.Lost)
        {
            _levelStartTime = Time.time;
            CurrentLevelState = LevelState.Colonizing;
            _timeLeft = CurrentLevel.LevelDuration();
            Debug.Log(">> First Solar System Spawned");
            UserInterface.SharedInstance.MainCanvas.SetActive(true);
            CurrentLevel.ColonizedPlanetCount = 1;
            
        }
        else if(CurrentLevelState==LevelState.Lost)
        {
            Planet home = SpawnerObject.GetComponent<SolarSystemGenerator>().CurrentHomePlanet().GetComponent<Planet>();
            CurrentLevel.ColonizedPlanetCount = 1;
        }
        else
        {
            Debug.Log(">> Next Solar System Spawned. Index: ["+levelIndex+"]");
        }
    }

    void asteroidThreatBegan()
    {
        Debug.Log("New Asteroid in play area let's warn the user..");
        UserInterface.SharedInstance.AsteroidWarningButton.SetActive(true);
        
    }
    
    void asteroidThreadOver()
    {
        if (AsteroidThreatList.Count == 0)
        {
            Debug.Log("Asteroid was destroyed.. let's supress the warning");
            UserInterface.SharedInstance.AsteroidWarningButton.SetActive(false);
        }
    }

    #endregion

    #region UserInterface Actions

    public void PauseButtonTapped()
    {
        _stateBeforePause = CurrentLevelState;
        CurrentLevelState = LevelState.Paused;
    }

    public void ResumeButtonTapped()
    {
        CurrentLevelState = _stateBeforePause; 
    }

    public void NextLevelButtonTapped()
    {
        UserInterface.SharedInstance.DisplaySessionEndedPanel(false, false);
        if (CurrentLevelState == LevelState.Lost)
        {
            _levelStartTime = Time.time;
            _timeLeft = CurrentLevel.LevelDuration();
            CurrentLevelState = LevelState.Colonizing;
            UserInterface.SharedInstance.MainCanvas.SetActive(true);
            Planet earth = SpawnerObject.GetComponent<SolarSystemGenerator>().CurrentHomePlanet().GetComponent<Planet>();
            earth.HumanCount += CurrentLevel.StartingHumans;
            CurrentLevel.HumanPopulation += CurrentLevel.StartingHumans;

            Camera.main.GetComponent<MobileCameraControl>().StartPanMode(MobileCameraControl.CameraMode.panHome);
        }
        else if (CurrentLevelState == LevelState.Won) //triggers portal animation
        {
            Camera.main.GetComponent<MobileCameraControl>().StartPanMode(MobileCameraControl.CameraMode.panNewHome);
            UserInterface.SharedInstance.DisplayPlanetGoalAchievedImages(false);
        }
        else
        {
            Debug.Log("this shouldn't happen");
        }
    }

    #endregion

    void _transitionToNextLevel(int newLevelIndex)
    {
        Vector2 nextHomePosition = Random.insideUnitCircle * (CurrentLevel.SolarSystemRadius * 8.0f);
        levelIndex = newLevelIndex;
        CurrentHomePosition = nextHomePosition;
        SolarSystemSeed nextLevel = new SolarSystemSeed(newLevelIndex);
        CurrentLevel = nextLevel;
        _initializeSolarSystem();
    }

    void _initializeSolarSystem()
    {
        Debug.Log("Initializing solar system with index: " + levelIndex);

        GameManager.SharedInstance.CurrentLevel = new SolarSystemSeed(levelIndex);
        SpawnerObject.SetActive(true);

        SolarSystemGenerator planetSeed = SpawnerObject.GetComponent<SolarSystemGenerator>();
        planetSeed.minPlanets     = CurrentLevel.MinPlanetCount;
        planetSeed.maxPlanets     = CurrentLevel.MaxPlanetCount;
        planetSeed.MinimumPlanetDistance = CurrentLevel.MinPlanetDistance;
        planetSeed.MinPlanetScale = CurrentLevel.MinPlanetScale;
        planetSeed.MaxPlanetScale = CurrentLevel.MaxPlanetScale;

        AsteroidSpawner asteroidSeed = SpawnerObject.GetComponent<AsteroidSpawner>();
        asteroidSeed.minSpawnInterval = CurrentLevel.AsteroidThreatMinInterval;
        asteroidSeed.maxSpawnInterval = CurrentLevel.AsteroidThreatMaxInterval;
        
        StartCoroutine(SpawnerObject.GetComponent<SolarSystemGenerator>().GenerateSolarSystem());
        asteroidSeed.enabled = true;
    }

    IEnumerator _removeAsteroidThreats()
    {
        while (AsteroidThreatList.Count != 0)
        {
            GameObject asteroid = (GameObject)AsteroidThreatList[0];
            Destroy(asteroid);
            AsteroidThreatList.RemoveAt(0);
            yield return null;
        }
    }

}
