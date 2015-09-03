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
    
    /// <summary>
    /// TODO: User Inteface stuff could be separated into its own class
    /// </summary>
    public GameObject OverLayCanvas;
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
    private string randomWinTitle;
    private string randomLostTitle;

    /// <summary>
    /// TODO: move to its own analytics utility
    /// </summary>
	private int maxPlanets = 0;
	private int maxHumans = 0;

    bool gameStarted;

    //TODO: make sure this exists throught out multiple scenes
    public static GameManager SharedInstance
    {
        get
        {
            return GameManager.mInstance;
        }
    }

    private static GameManager mInstance;
    
    public void OnEnable()
    {
        EventManager.StartListening(EventManager.eSolarSystemDidFinishSpawning, solarSystemDidSpawn);
        EventManager.StartListening(EventManager.eAsteroidSpawnedEvent, asteroidThreatBegan);
        EventManager.StartListening(EventManager.eAsteroidDestroyedEvent, asteroidThreadOver);
    }

    public void OnDisable()
    {
        EventManager.StopListening(EventManager.eSolarSystemDidFinishSpawning, solarSystemDidSpawn);
        EventManager.StopListening(EventManager.eAsteroidSpawnedEvent, asteroidThreatBegan);
        EventManager.StopListening(EventManager.eAsteroidDestroyedEvent, asteroidThreadOver);
    }

    void Awake()
    {
        mInstance = this;
    }

    public void Start()
    {
        gameStarted = false;
        AsteroidThreatList = new ArrayList();
        CurrentHomePosition = new Vector2(0, 0);
        //TODO: if tutorial made the tutorial seed would be a hardcoded -1
        levelIndex = 15;
        initializeGame();
        OverLayCanvas.SetActive(true);
    }
  
    public void Update()
    {
        if(gameStarted)
        {
            //update User Interface
            humanCountDisplay.text = CurrentLevel.HumanPopulation.ToString();
            planetCountDisplay.text = CurrentLevel.ColonizedPlanetCount.ToString() + "\\" + CurrentLevel.RequiredPlanets;

            gameTimer.text = formattedGameDuration();

            if (CurrentLevel.ColonizedPlanetCount >= CurrentLevel.RequiredPlanets)
            {
                SessionEndedPanel.SetActive(true);
                titleText.text = randomWinTitle;
                scoreText.text = (maxPlanets * maxHumans).ToString();
            }

            if (CurrentLevel.HumanPopulation <= 0)
            {
                SessionEndedPanel.SetActive(true);
                titleText.text = randomLostTitle;
                scoreText.text = (maxPlanets * maxHumans).ToString();
            }

            if (CurrentLevel.HumanPopulation > maxHumans)
                maxHumans = CurrentLevel.HumanPopulation;

            if (CurrentLevel.ColonizedPlanetCount > maxPlanets)
                maxPlanets = CurrentLevel.ColonizedPlanetCount;
        }
    }

    public void solarSystemDidSpawn()
    {
        gameStarted = true;
    }

    public void asteroidThreatBegan()
    {
        Debug.Log("New Asteroid in play area let's warn the user..");
        AsteroidWarningButton.SetActive(true);
    }

    public void asteroidThreadOver()
    {        
        if (AsteroidThreatList.Count == 0)
        {
            Debug.Log("Asteroid was destroyed.. let's supress the warning");
            AsteroidWarningButton.SetActive(false);
        }       
    }

    #region UserInterface Actions

        public void MissileModeSelected()
        {
            CurrentSelectedPlanet.GetComponent<Planet>().LaunchMissile();
	    }

        public void HumanModeSelected()
        {
            CurrentSelectedPlanet.GetComponent<Planet>().LaunchCrew();
        }

        public void RestartLevel()
        {
            //TODO, change to 1 after we add a new menu
            Application.LoadLevel(0);
        }

        public void NextLevel()
        {

        }

        public void PauseButtonTapped()
        {
            SessionpausedPanel.SetActive(true);
        }

        public void ResumeButtonTapped()
        {
            SessionpausedPanel.SetActive(false);
        }

        public void QuitButtonTapped()
        {
            Application.LoadLevel("Menu");
        }

    #endregion

    public string formattedGameDuration()
    {
        float timer = Time.timeSinceLevelLoad;
        string minSec = string.Format("{0}:{1:00}", (int)timer / 60, (int)timer % 60);
        return minSec.ToString();
    }

    void initializeGame()
    {
        randomLostTitle = loseTitles[Random.Range(0, loseTitles.Length - 1)];
        randomWinTitle = winTitles[Random.Range(0, winTitles.Length - 1)];

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
        asteroidSeed.asteroidTargetPosition = CurrentHomePosition;


        StartCoroutine(SpawnerObject.GetComponent<SolarSystemGenerator>().GenerateSolarSystem());
        asteroidSeed.enabled = true;
    }

   public void TransitionToNextLevel()
   {
       Vector2 nextHomePosition = Random.insideUnitCircle*(CurrentLevel.SolarSystemRadius*Random.Range(CurrentLevel.SolarSystemRadius/2,CurrentLevel.SolarSystemRadius*2));

       //TODO asteroidSpawner & planetSpawner start listening to destroy event

       SpawnerObject = Instantiate(SpawnerObject,nextHomePosition,Quaternion.identity) as GameObject;

       levelIndex++;
       SolarSystemSeed nextLevel = new SolarSystemSeed(levelIndex);
       
       initializeGame();

       //Maybe: add random spacejunk anywhere between the solar systems so there is a point of reference as the camera moves over

       //1  camera zooms out and moves to the right!
            // couroutine sends events: 
                    //previousSystemShouldDeleteEvent when solarsystem is offscreen or when next one is shown
                    //portalShouldOpenEvent when camera animation is over
       
            // asteroidSpawner & planetSpawner previousSystemShouldDeleteEvent - all previous level game objects will be removed
            // portalBehavior script responds instantiates ship, lands it on home planet !
       //2  
       //   a   when previous system goes off screen: asteroidSpawner, planetSpawner destroy all instantiated objects
       //   b   spawners move their transform to the next location & generate the new system at this location

       //onPreviousSolarSystemDestroyed --> 

       CurrentLevel = nextLevel;
       CurrentHomePosition = nextHomePosition;
   }
}
