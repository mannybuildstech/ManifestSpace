using UnityEngine;
using System.Collections;

using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject CurrentSelectedPlanet;

    public int HumanCount;
    public int PlanetCount = 1;

    public Queue asteroidQueue = new Queue();

	public GameObject SessionEndedPanel;
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

    //currently assuming the singleton object is instantiated before it gets accessed!
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
        EventManager.StartListening(EventManager.eAsteroidSpawnedEvent, newAsteroid);
        EventManager.StartListening(EventManager.eAsteroidDestroyedEvent, lessAsteroids);
    }

    public void OnDisable()
    {
        EventManager.StopListening(EventManager.eAsteroidSpawnedEvent, newAsteroid);
        EventManager.StopListening(EventManager.eAsteroidDestroyedEvent, lessAsteroids);
    }

	public void restartGame()
	{
		Application.LoadLevel(1);
	}

    void Awake()
    {
        mInstance = this;
    }

    public void Update()
    {
        //update global counters
        humanCountDisplay.text = HumanCount.ToString();
        planetCountDisplay.text = PlanetCount.ToString();


        float timer = Time.timeSinceLevelLoad;
        string minSec = string.Format("{0}:{1:00}", (int)timer / 60, (int)timer % 60);
        gameTimer.text = minSec.ToString();

        if(PlanetCount>=winCount)
        {
			SessionEndedPanel.SetActive(true);
			titleText.text = "YOU ARE THE MASTER OF SPACE!";
			scoreText.text = (maxPlanets * maxHumans).ToString();
        }

		if(HumanCount <= 0)
		{
			SessionEndedPanel.SetActive(true);
			titleText.text = "SPACE HAS DESTROYED THE HUMAN RACE!";
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

    #region Asteroid 
    public void newAsteroid()
    {
        Debug.Log("New Asteroid in play area let's warn the user..");
        AsteroidWarningButton.SetActive(true);
    }

    public void lessAsteroids()
    {
        if (asteroidQueue.Count == 0)
        {
            Debug.Log("Asteroid was destroyed.. let's supress the warning");
            AsteroidWarningButton.SetActive(false);
        }
            
    }
    #endregion

    #region Projectile 
    public void MissileModeSelected()
    {
        CurrentSelectedPlanet.GetComponent<Planet>().LaunchMissile();
	}

    public void HumanModeSelected()
    {
        CurrentSelectedPlanet.GetComponent<Planet>().LaunchCrew();
    }

    #endregion

    public void RestartLevel()
    {
        //TODO, change to 1 after we add a new menu
        Application.LoadLevel(0);
    }
}
