using UnityEngine;
using System.Collections;

public class TutorialController : MonoBehaviour 
{
    public GameObject LaunchTutorial;
    public GameObject PortalTutorial;   
    public GameObject MissileTutorial;
    public GameObject PowerUpTutorial;

    public void OnEnable()
    {
        EventManager.StartListening(EventManager.ePlanetsAquiredEvent, PlayPortalTutorial);
        EventManager.StartListening(EventManager.eCameraPannedToNewHomeEvent, PlayMissileTutorial);
        EventManager.StartListening(EventManager.ePowerUpReceived, PlayPowerUpTutorial);
    }

    public void OnDisable()
    {
        EventManager.StopListening(EventManager.ePlanetsAquiredEvent, PlayPortalTutorial);
        EventManager.StopListening(EventManager.eCameraPannedToNewHomeEvent, PlayMissileTutorial);
        EventManager.StopListening(EventManager.ePowerUpReceived, PlayPowerUpTutorial);
    }

	// Use this for initialization
	void Start () 
    {
        if(shouldPlayTutorial(0))
        {
            Invoke("launchTutorial", 1.0f);
        }
	}

    void launchTutorial()
    {
        LaunchTutorial.GetComponent<SessionEndedPanelAnim>().ShowPanel();
        GameManager.SharedInstance.PauseGame();
    }

    public void LaunchTutorialDismissed()
    {
        incrementTutorialData();
        LaunchTutorial.GetComponent<SessionEndedPanelAnim>().HidePanel();
        GameManager.SharedInstance.ResumeGame();
    }

    public void PlayPortalTutorial()
    {
        if(shouldPlayTutorial(1))
        {
            Invoke("portallauncher", 1.0f);
        }
    }
    
    void portallauncher()
    {
        PortalTutorial.GetComponent<SessionEndedPanelAnim>().ShowPanel();
        GameManager.SharedInstance.PauseGame();
    }

    public void PortalTutorialDismissed()
    {
        incrementTutorialData();
        PortalTutorial.GetComponent<SessionEndedPanelAnim>().HidePanel();
        GameManager.SharedInstance.ResumeGame();
    }

    public void PlayMissileTutorial()
    {
        if(shouldPlayTutorial(2) && GameManager.SharedInstance.levelIndex==1)
        {
            Invoke("missileDelay", 1.0f);
        }
    }

    void missileDelay()
    {
        MissileTutorial.GetComponent<SessionEndedPanelAnim>().ShowPanel();
        GameManager.SharedInstance.PauseGame();
    }

    public void MissileTutorialDismissed()
    {
        incrementTutorialData();
        MissileTutorial.GetComponent<SessionEndedPanelAnim>().HidePanel();
        GameManager.SharedInstance.ResumeGame();
    }

    public void PlayPowerUpTutorial()
    {
        if (shouldPlayTutorial(3))
        {
            Invoke("invokeDelayPowerUpTutorial",2.0f);
        }
    }

    void invokeDelayPowerUpTutorial()
    {
        PowerUpTutorial.GetComponent<SessionEndedPanelAnim>().ShowPanel();
        GameManager.SharedInstance.PauseGame();
    }

    public void PowerUpTutorialDismissed()
    {
        incrementTutorialData();
        PowerUpTutorial.GetComponent<SessionEndedPanelAnim>().HidePanel();
        GameManager.SharedInstance.ResumeGame();
    }

    string key = "tutorial_manifest_space";
    bool shouldPlayTutorial(int tutorialIndex)
    {
        
        int value = -1;
        if(PlayerPrefs.HasKey(key))
        {
            value = PlayerPrefs.GetInt(key);
        }
        else
        {
            PlayerPrefs.SetInt(key, -1);
        }
        return value<tutorialIndex;
    }

    void incrementTutorialData()
    {
        int oldValue = -1;
        if (PlayerPrefs.HasKey(key))
        {
            oldValue = PlayerPrefs.GetInt(key);
        }
        PlayerPrefs.SetInt(key,oldValue+1);
    }
}
