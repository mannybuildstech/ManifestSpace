using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject SpawnerObject;
    
    public int levelIndex;
    public SolarSystemSeed  CurrentLevel;

    public Vector2          CurrentHomePosition;
    public GameObject       CurrentSelectedPlanet;
    
    public ArrayList        AsteroidThreatList;

    enum LevelState {Colonizing,LocatingPortal,Paused,Lost,Won};
    LevelState CurrentLevelState;

    LevelState _stateBeforePause;

    /// <summary>
    /// TODO: User Inteface stuff could be separated into its own class
    /// </summary>
    public GameObject[] PlanetReqFlags;
    public GameObject GameCanvas;
    public GameObject LevelUI;
    public GameObject SessionEndedPanel;
    public GameObject SessionpausedPanel;
    public GameObject AsteroidWarningButton;
    public Text titleText;
	public Text scoreText;
    public Text gameTimer;
    public Text humanCountDisplay;
    public Text planetCountDisplay;

    public string[] winTitles = new string[10];
    public string[] loseTitles = new string[10];

    public float _levelStartTime;
 
    /// <summary>
    /// TODO: move to its own analytics utility
    /// </summary>
	private int maxPlanets = 0;
	private int maxHumans = 0;

    //TODO: make sure this exists throught out multiple scenes
    public static GameManager SharedInstance
    {
        get
        {
            return GameManager.mInstance;
        }
    }

    private static GameManager mInstance;

    #region Unity Events

    public void OnEnable()
    {
        EventManager.StartListening(EventManager.eSolarSystemDidFinishSpawning, _solarSystemSpawned);
        EventManager.StartListening(EventManager.ePortalEnteredEvent, _enteredNextLevelPortal);
        EventManager.StartListening(EventManager.eNextHomeIsReadyEvent, _LandedNextPlanetEvent);
        EventManager.StartListening(EventManager.eAsteroidSpawnedEvent, asteroidThreatBegan);
        EventManager.StartListening(EventManager.eAsteroidDestroyedEvent, asteroidThreadOver);
    }

    public void OnDisable()
    {
        EventManager.StopListening(EventManager.eSolarSystemDidFinishSpawning, _solarSystemSpawned);
        EventManager.StartListening(EventManager.ePortalEnteredEvent, _enteredNextLevelPortal);
        EventManager.StartListening(EventManager.eNextHomeIsReadyEvent, _LandedNextPlanetEvent);
        EventManager.StopListening(EventManager.eAsteroidSpawnedEvent, asteroidThreatBegan);
        EventManager.StopListening(EventManager.eAsteroidDestroyedEvent, asteroidThreadOver);
    }

    void Awake()
    {
        mInstance = this;
    }

    public void Start()
    {
        CurrentLevelState = LevelState.Paused;
        AsteroidThreatList = new ArrayList();
        CurrentHomePosition = new Vector2(0, 0);
        levelIndex = 0;
        initializeGame();
    }
  
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            _enteredNextLevelPortal();

        }

        if(CurrentLevelState == LevelState.Colonizing || CurrentLevelState == LevelState.LocatingPortal)
        {
            _update_UI();
            _update_score();
            _update_levelcheck();
        }
    }

    #endregion

    #region updateMethods

    void _update_UI()
    {
        //update User Interface
        humanCountDisplay.text = CurrentLevel.HumanPopulation.ToString();
        planetCountDisplay.text = CurrentLevel.ColonizedPlanetCount.ToString() + "\\" + CurrentLevel.RequiredPlanets;

        float timer = Time.time - _levelStartTime;
        string minSec = string.Format("{0}:{1:00}", (int)timer / 60, (int)timer % 60);
        gameTimer.text = minSec.ToString();
    }

    public void _update_score()
    {
        if (CurrentLevel.HumanPopulation > maxHumans)
            maxHumans = CurrentLevel.HumanPopulation;

        if (CurrentLevel.ColonizedPlanetCount > maxPlanets)
            maxPlanets = CurrentLevel.ColonizedPlanetCount;
    }

    public void _update_levelcheck()
    {
        scoreText.text = (maxPlanets * maxHumans).ToString();
        if (CurrentLevel.ColonizedPlanetCount >= CurrentLevel.RequiredPlanets && CurrentLevelState==LevelState.Colonizing)
        {
            CurrentLevelState = LevelState.LocatingPortal;
            EventManager.PostEvent(EventManager.ePlanetsAquiredEvent);

            MusicPlayer.SharedInstance.planetAchievementSound();
            foreach (GameObject flag in PlanetReqFlags)
                flag.SetActive(true);
        }

        if ((CurrentLevelState==LevelState.Colonizing || CurrentLevelState==LevelState.LocatingPortal))
        {
            if(CurrentLevel.HumanPopulation <= 0)
            {
                CurrentLevelState = LevelState.Lost;
                titleText.text = loseTitles[Random.Range(0, loseTitles.Length - 1)];
                SessionEndedPanel.SetActive(true);

                Debug.Log("Lost game... ");
                Debug.Log("Removing asteroid threat");
                Debug.Log("Closing portals");
                Debug.Log("Regenerating a solar system of level: "+levelIndex);

                StartCoroutine(_removeAsteroidThreats());
                GameObject portal = SpawnerObject.GetComponent<PortalSpawner>().CurrentPortal;
                if (portal != null)
                    Destroy(portal);                
                _transitionToNextLevel(levelIndex);
            }
        }
    }
    
    #endregion

    #region Game Events

    void asteroidThreatBegan()
    {
        Debug.Log("New Asteroid in play area let's warn the user..");
        AsteroidWarningButton.SetActive(true);
    }

    void asteroidThreadOver()
    {        
        if (AsteroidThreatList.Count == 0)
        {
            Debug.Log("Asteroid was destroyed.. let's supress the warning");
            AsteroidWarningButton.SetActive(false);
        }       
    }
    
    void _solarSystemSpawned()
    {
        if (levelIndex == 0 && CurrentLevelState!=LevelState.Lost)
        {
            _levelStartTime = Time.time;
            CurrentLevelState = LevelState.Colonizing;

            Debug.Log(">> First Solar System Spawned");
            GameCanvas.SetActive(true);
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

    IEnumerator _removeAsteroidThreats()
    {
        while(AsteroidThreatList.Count!=0)
        {
            GameObject asteroid = (GameObject)AsteroidThreatList[0];
            Destroy(asteroid);
            AsteroidThreatList.RemoveAt(0);
            yield return null;
        }
    }

    void _LandedNextPlanetEvent()
    {
        LevelUI.SetActive(true);
        CurrentLevelState = LevelState.Colonizing;
        _levelStartTime = Time.time;
    }

    void _enteredNextLevelPortal()
    {
        LevelUI.SetActive(false);
        titleText.text = winTitles[Random.Range(0, winTitles.Length - 1)];
        SessionEndedPanel.SetActive(true);
        CurrentLevelState = LevelState.Won;
        int newLevel = levelIndex + 1;
        _transitionToNextLevel(newLevel);
    }
    #endregion

    #region UserInterface Actions

        public void MissileModeSelected()
        {
            CurrentSelectedPlanet.GetComponent<Planet>().LaunchMissile();
	    }

        public void HumanModeSelected()
        {
            CurrentSelectedPlanet.GetComponent<Planet>().LaunchCrew();
        }

        public void PauseButtonTapped()
        {
            _stateBeforePause = CurrentLevelState;
            CurrentLevelState = LevelState.Paused;
            SessionpausedPanel.SetActive(true);
        }

        public void ResumeButtonTapped()
        {
            CurrentLevelState = _stateBeforePause; 
            SessionpausedPanel.SetActive(false);
        }

        public void QuitButtonTapped()
        {
            Application.LoadLevel("Menu");
        }

        public void NextLevelButtonTapped()
        {
            SessionEndedPanel.SetActive(false);

            humanCountDisplay.text = "";
            planetCountDisplay.text = "";
            gameTimer.text = "";

            if(CurrentLevelState==LevelState.Lost)
            {
                _levelStartTime = Time.time;
                CurrentLevelState = LevelState.Colonizing;
                GameCanvas.SetActive(true);
                Camera.main.GetComponent<MobileCameraControl>().StartPanMode(MobileCameraControl.CameraMode.panHome);
            }
            else if(CurrentLevelState==LevelState.Won)
            {
                //also triggers landing portal animation!
                Camera.main.GetComponent<MobileCameraControl>().StartPanMode(MobileCameraControl.CameraMode.panNewHome);
                humanCountDisplay.text = "";
                planetCountDisplay.text = "";
                gameTimer.text = "";
            }
            else
            {
                Debug.Log("this shouldn't happen");
            }
        }

    #endregion

    void initializeGame()
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

   void _transitionToNextLevel(int newLevelIndex)
   {
       Vector2 nextHomePosition = Random.insideUnitCircle * (CurrentLevel.SolarSystemRadius * 4.0f);
       levelIndex = newLevelIndex;
       CurrentHomePosition = nextHomePosition;
       SolarSystemSeed nextLevel = new SolarSystemSeed(newLevelIndex);
       CurrentLevel = nextLevel;
       initializeGame();
   }
}
