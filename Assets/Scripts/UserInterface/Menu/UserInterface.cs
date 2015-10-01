using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using UnityEngine.Advertisements;

public class UserInterface : MonoBehaviour 
{
    static int addDisplayInterval = 10;

    public Text TotalSystemsText;
    public Text TotalHumansText;
    public Text TotalColoniesText;
    public Text ScoreValueText;
    
    public Text SessionEndedTitle;
    public string[] winTitles = new string[10];
    public string[] loseTitles = new string[10];

    public Text HumanCountText;
    public Text PlanetCountText;

    public GameObject[] planetGoalUnlockedImages;

    public GameObject MainCanvas;
    public GameObject SessionEndedPanel;
    public GameObject SessionPausedPanel;

    public GameObject LevelUI;
    public GameObject AsteroidWarningButton;
    public GameObject PortalFinderButton;

    public Text TimeRemainingPanel;

    public Color    RatingUnlockedColor;
    public Color    RatingLockedColor;    
    public Image[]  RatingImages;

    public GameObject   retryOrNextLevelButton;
    public Text         retryOrContinueText;

    public RadialFill  missileButtonFill;
    public RadialFill  shipButtonFill;
    
    private static UserInterface mInstance;
    public static UserInterface SharedInstance { get {return UserInterface.mInstance;}}

    bool rewardAddMode = false;
    
    int numSessions;

    public void OnEnable()
    {
        EventManager.StartListening(EventManager.ePlanetsAquiredEvent, _enablePortalFinder);
        EventManager.StartListening(EventManager.ePortalEnteredEvent, _disablePortalFinder);
    }

    public void OnDisable()
    {
        EventManager.StopListening(EventManager.ePlanetsAquiredEvent, _enablePortalFinder);
        EventManager.StopListening(EventManager.ePortalEnteredEvent, _disablePortalFinder);
    }

    void _enablePortalFinder()
    {
        //TODO play sound? 'portal found'
        PortalFinderButton.SetActive(true);
    }

    void _disablePortalFinder()
    {
        PortalFinderButton.SetActive(false);
    }

    public void Start()
    {
        if(Application.platform==RuntimePlatform.Android)
        {
            Advertisement.Initialize("71219", false);
        }
        else if(Application.platform==RuntimePlatform.IPhonePlayer)
        {
            Advertisement.Initialize("71221", false);
        }
        else
        {
            Advertisement.Initialize("71219", true);
        }
    }

    void Awake()
    {
        mInstance = this;
    }

    public void DisplaySessionEndedPanel(bool visible, bool didWin)
    {
        RatingImages[0].color = RatingLockedColor;
        RatingImages[1].color = RatingLockedColor;
        RatingImages[2].color = RatingLockedColor;
        SessionEndedPanel.SetActive(visible);
       
        if(visible)
        {
            LevelUI.SetActive(false);
            numSessions++;

            if (didWin)
            {
                rewardAddMode = false;
                retryOrContinueText.text = "Next Level";
                retryOrNextLevelButton.SetActive(true);
                if(numSessions%addDisplayInterval==0)
                {
                    StartCoroutine(displayAdd());
                }
                else
                {
                    MusicPlayer.SharedInstance.playLevelWinSFX();
                }
                    
                DisplayRandomWinText();
                DisplaySessionEndedAnalytics();
                DisplayRating();
            }
            else
            {
                Debug.Log("Player lost, showing session ended panel");

                retryOrContinueText.text = "Watch Ad to Retry";
                rewardAddMode = true;
                DisplayrandomLostText();
                DisplaySessionEndedAnalytics();
            }
        }
    }

    IEnumerator displayAdd()
    {
        while(!Advertisement.IsReady())
            yield return null;
        Advertisement.Show("defaultZone");   
    }

    IEnumerator displayRewardAdd()
    {
        ShowOptions addOptions = new ShowOptions();
        addOptions.resultCallback = RewardAdCallback;
        while (!Advertisement.IsReady())
            yield return null;
        Advertisement.Show("rewardedVideoZone", addOptions);   
    }

    void RewardAdCallback(ShowResult result)
    {
        if(result==ShowResult.Finished)
        {
            rewardAddMode = false;
            Debug.Log("Watched add, user can continue...");
            NextLevelButtonTapped();
        }
        else
        {
            //TODO add leaderboard stuff here....
            QuitButtonTapped();
        }
    }

    public void DisplayRandomWinText()
    {
        SessionEndedTitle.text = winTitles[Random.Range(0, winTitles.Length - 1)]; ;
    }

    public void DisplayrandomLostText()
    {
        SessionEndedTitle.text = loseTitles[Random.Range(0, loseTitles.Length - 1)]; ;
    }

    public void DisplaySessionEndedAnalytics()
    {
        TotalColoniesText.text = GameManager.SharedInstance.TotalPlanets.ToString();
        TotalHumansText.text = GameManager.SharedInstance.TotalHumans.ToString();
        TotalSystemsText.text = GameManager.SharedInstance.TotalSystems.ToString();
        StartCoroutine(fillScore(_computeScore()));
        //display rating
    }

    public void DisplayRating()
    {
        StartCoroutine(fillRating());
    }

    public void DisplayCurrentData()
    {
        HumanCountText.text  = GameManager.SharedInstance.CurrentLevel.HumanPopulation.ToString();
        PlanetCountText.text = GameManager.SharedInstance.CurrentLevel.ColonizedPlanetCount.ToString() + "\\" + GameManager.SharedInstance.CurrentLevel.RequiredPlanets;
     
        float timeLeft = GameManager.SharedInstance.TimeLeft;
        TimeRemainingPanel.color = (timeLeft<20.0f)?Color.red:Color.white;
        
        int sec = (int)GameManager.SharedInstance.TimeLeft % 60;
	    int min = (int)GameManager.SharedInstance.TimeLeft / 60;
        TimeRemainingPanel.text = string.Format("{0:00}:{1:00}", min, sec);
    }

    public void DisplayPlanetGoalAchievedImages(bool visible)
    {
        foreach (GameObject flag in planetGoalUnlockedImages)
            flag.SetActive(visible);
    }

    int _computeScore()
    {
        float remainingTime = GameManager.SharedInstance.TimeLeft/GameManager.SharedInstance.CurrentLevel.LevelDuration();
        if (remainingTime < 0)
            remainingTime = 1;
        float score = remainingTime * ((GameManager.SharedInstance.CurrentLevel.HumanPopulation * GameManager.SharedInstance.CurrentLevel.ColonizedPlanetCount));
        
        int humanDeaths = GameManager.SharedInstance.CurrentLevel.HumanDeaths;
        int planetsLost = GameManager.SharedInstance.CurrentLevel.LostColonies;
        score -=  humanDeaths + planetsLost;
        score *= _computeRating();
        GameManager.SharedInstance.TotalScore += score;
        return (int)score;
    }

    public int _computeRating()
    {
        int humanDeaths = GameManager.SharedInstance.CurrentLevel.HumanDeaths;
        int planetsLost = GameManager.SharedInstance.CurrentLevel.LostColonies;

        if(humanDeaths==0 && planetsLost==0)
        {
            return 3;
        }
        else if ((humanDeaths/GameManager.SharedInstance.CurrentLevel.MaxPassengerCount)<5)
        {
            return 2;
        }
        else
        {
            return 1;
        }
    }

    IEnumerator fillScore(int score)
    {
        int curScore = 0;
        Debug.Log("Filling score " + score);
        while (curScore < score)
        {
            ScoreValueText.text = curScore.ToString();
            curScore+=(int)(score/10);
            yield return null;
        }
    }

    IEnumerator fillRating()
    {
        int count = _computeRating();
        Debug.Log("Filling Rating " + count);
        
        for (int i = 0; i < RatingImages.Length; i++)
        {
            if ((i + 1) <= count)
            {
                RatingImages[i].color = RatingUnlockedColor;
                Debug.Log("yellow");
                MusicPlayer.SharedInstance.PlayPlanetRatingSound(i);
            }
            else
            { 
            }
            yield return new WaitForSeconds(.25f);
        }
    }

    #region button actions

    public void HumanModeSelected()
    {
        GameManager.SharedInstance.CurrentSelectedPlanet.GetComponent<Planet>().LaunchCrew();
    }

    public void PauseButtonTapped()
    {
        GameManager.SharedInstance.PauseButtonTapped();
        SessionPausedPanel.SetActive(true);
    }

    public void ResumeButtonTapped()
    {
        GameManager.SharedInstance.ResumeButtonTapped();
        SessionPausedPanel.SetActive(false);
    }

    public void NextLevelButtonTapped()
    {
        if(rewardAddMode)
        {
            StartCoroutine(displayRewardAdd());
        }
        else
        {
            Debug.Log("user moving to next level");
            LevelUI.SetActive(true);
            SessionEndedPanel.SetActive(false);
            GameManager.SharedInstance.NextLevelButtonTapped();
        }
    }

    public void QuitButtonTapped()
    {
        Application.LoadLevel("Menu");
    }

    public void UI_MissileButtonDown()
    {
        missileButtonFill.StartRadialFill();
        GameManager.SharedInstance.CurrentSelectedPlanet.GetComponent<Planet>().GetComponentInChildren<SpaceStation>().ConfigureSightLine(true);
    }

    public void UI_MissileButtonUp()
    {
        bool trigger = missileButtonFill.ProgressBarFull();
        if(trigger)
        {
            GameManager.SharedInstance.CurrentSelectedPlanet.GetComponent<Planet>().LaunchMissile();
        }
        GameManager.SharedInstance.CurrentSelectedPlanet.GetComponent<Planet>().GetComponentInChildren<SpaceStation>().ConfigureSightLine(false);
        missileButtonFill.StopRadialFill(trigger);
    }

    public void UI_ShipButtonDown()
    {
        shipButtonFill.StartRadialFill();
        GameManager.SharedInstance.CurrentSelectedPlanet.GetComponent<Planet>().GetComponentInChildren<SpaceStation>().ConfigureSightLine(true);
    }

    public void UI_ShipButtonUp()
    {
        bool trigger = shipButtonFill.ProgressBarFull();
        if (trigger)
        {
            GameManager.SharedInstance.CurrentSelectedPlanet.GetComponent<Planet>().LaunchCrew();
        }
        GameManager.SharedInstance.CurrentSelectedPlanet.GetComponent<Planet>().GetComponentInChildren<SpaceStation>().ConfigureSightLine(false);
        shipButtonFill.StopRadialFill(trigger);
    }

    #endregion
}
