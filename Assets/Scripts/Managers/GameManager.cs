using UnityEngine;
using System.Collections;

using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum LaunchMode { MissileMode, HumanMode};

    public LaunchMode currentLaunchMode;
    public int HumanCount;
    public int PlanetCount = 1;

    public GameObject[] CurrentAsteroids;

	public GameObject panel;
	public Text titleText;
	public Text scoreText;

	public Button HumanButton;
	public Button MissleButton;

	public Sprite HumanNonSelected;
	public Sprite MissleNonSelected;
	public Sprite HumanSelected;
	public Sprite MissleSelected;

    public Text humanCountDisplay;
    public Text planetCountDisplay;

    public GameObject AsteroidWarningButton;

	private int maxPlanets = 0;
	private int maxHumans = 0;

    public int winCount;
    public float SolarSystemRadius;

    //currently assuming the singleton object is instantiated before it gets accessed!
    public static GameManager SharedInstance
    {
        get
        {
            return GameManager.mInstance;
        }
    }

    private static GameManager mInstance;
    
	void Start()
	{
		CurrentAsteroids = new GameObject[10];
	}

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
		currentLaunchMode = LaunchMode.HumanMode;
    }

    public void Update()
    {
        //update global counters
        humanCountDisplay.text = HumanCount.ToString();
        planetCountDisplay.text = PlanetCount.ToString();

        if(PlanetCount>=winCount)
        {
			panel.SetActive(true);
			titleText.text = "YOU ARE THE MASTER OF SPACE!";
			scoreText.text = (maxPlanets * maxHumans).ToString();
        }

		if(HumanCount <= 0)
		{
			panel.SetActive(true);
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

    #region Asteroid Warning
    public void newAsteroid()
    {
        Debug.Log("New Asteroid in play area let's warn the user..");
        AsteroidWarningButton.SetActive(true);
    }

    public void lessAsteroids()
    {
        if (GameManager.SharedInstance.CurrentAsteroids.Length == 0)
        {
            Debug.Log("Asteroid was destroyed.. let's supress the warning");
            AsteroidWarningButton.SetActive(false);
        }
            
    }
    #endregion

    #region Launch Type
    public void MissileModeSelected()
    {
        currentLaunchMode = LaunchMode.MissileMode;
		HumanButton.image.sprite = HumanNonSelected;
		MissleButton.image.sprite = MissleSelected;

	}

    public void HumanModeSelected()
    {
        currentLaunchMode = LaunchMode.HumanMode;

		MissleButton.image.sprite = MissleNonSelected;
		HumanButton.image.sprite = HumanSelected;
    }

    #endregion
}
