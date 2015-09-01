using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Vector2 HomePosition;

    public GameObject CurrentSelectedPlanet;

    public int HumanCount;
    public int PlanetCount = 1;

    public ArrayList asteroidThreatList;

	public GameObject SessionEndedPanel;
    public GameObject SessionpausedPanel;
    public Text titleText;
	public Text scoreText;
    public Text gameTimer;
    public Text humanCountDisplay;
    public Text planetCountDisplay;

    public GameObject AsteroidWarningButton;

	private int maxPlanets = 0;
	private int maxHumans = 0;

    public int winCount;
    public float SolarSystemRadius;
    public int startingHumans = 50;

    public string[] winTitles  = new string[10];
    public string[] loseTitles = new string[10];

    private string randomWinTitle;
    private string randomLostTitle;

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
        EventManager.StartListening(EventManager.eAsteroidSpawnedEvent, asteroidThreatBegan);
        EventManager.StartListening(EventManager.eAsteroidDestroyedEvent, asteroidThreadOver);
    }

    public void OnDisable()
    {
        EventManager.StopListening(EventManager.eAsteroidSpawnedEvent, asteroidThreatBegan);
        EventManager.StopListening(EventManager.eAsteroidDestroyedEvent, asteroidThreadOver);
    }

    public void Start()
    {
        asteroidThreatList = new ArrayList();
        HomePosition = new Vector2(0, 0);
        randomLostTitle = loseTitles[Random.Range(0, loseTitles.Length - 1)];
        randomWinTitle  =  winTitles[Random.Range(0, winTitles.Length - 1)];
    }

    void Awake()
    {
        mInstance = this;
    }

    public void Update()
    {
        //update User Interface
        humanCountDisplay.text = HumanCount.ToString();
        planetCountDisplay.text = PlanetCount.ToString()+"\\"+winCount;

        gameTimer.text = formattedGameDuration();
        
        if(PlanetCount>=winCount)
        {
			SessionEndedPanel.SetActive(true);
            titleText.text = randomWinTitle;
			scoreText.text = (maxPlanets * maxHumans).ToString();
        }

		if(HumanCount <= 0)
		{
			SessionEndedPanel.SetActive(true);
            titleText.text = randomLostTitle;
			scoreText.text = (maxPlanets * maxHumans).ToString();
		}

		if(HumanCount > maxHumans)
		{
			maxHumans = HumanCount;
		}

		if(PlanetCount > maxPlanets)
		{
			maxPlanets = PlanetCount;
		}
    }

    public void asteroidThreatBegan()
    {
        Debug.Log("New Asteroid in play area let's warn the user..");
        AsteroidWarningButton.SetActive(true);
    }

    public void asteroidThreadOver()
    {        
        if (asteroidThreatList.Count == 0)
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

    public void PauseButtonTapped()
    {
        SessionpausedPanel.SetActive(true);
        Time.timeScale = 0.0f;
    }

    public void ResumeButtonTapped()
    {
        SessionpausedPanel.SetActive(false);
        Time.timeScale = 1.0f;
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
        //load gameparameterdictionary

        //initialize it//
    }

    void initializeSolarSystemSession(IDictionary gameParameterDictionary)
    {
        //configure game manager:
            //win count
            //solar system radius
            //starting humans
            //missile recharge duration
   
        //configure planet spawner        
            //min & max planets
            //min & max planet scales
            //minimum planet distance

            //configure planet
                //min & max rotation speed
                
                //configure debriSpawner
                    //min & max debri count
                    //min & max orbit radius
                
                //configure spacestation
                    // missile reload seconds

                //configure spaceship
                    //spaceship lifetime
                    //max passengers

        //configure asteroid spawner
            //min & max spawn interval
    }

    void enableGameComponents()
    {
        // enable planet & asteroid spawners...
    }
}
