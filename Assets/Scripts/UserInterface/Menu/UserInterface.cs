using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using UnityEngine.Advertisements;
using UnityEngine.SocialPlatforms;
using System;

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

    public GameObject RetryDialog;
    public Text       LivesLeftText;

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

    public GameObject LeaderBoardButton;

    private static UserInterface mInstance;
    public static UserInterface SharedInstance { get {return UserInterface.mInstance;}}

    public GameObject PowerUpPanel;

    bool gameOverMode = false;
    
    int numSessions;

    string addbuddizz = "7c9f1ef3-3652-44b4-89c4-fdfbbae80e90";

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
        print("MFS Authenticating");
        StartCoroutine(authenticateGoogle());
        missileButtonFill.StartRadialFill();
        shipButtonFill.StartRadialFill();
        if(Application.platform==RuntimePlatform.Android)
        {
            Advertisement.Initialize("79414", true);
        }
        else if(Application.platform==RuntimePlatform.IPhonePlayer)
        {
            Advertisement.Initialize("", true);
        }
        else
        {
            Advertisement.Initialize("79414", true);
        }

        AdBuddizBinding.SetAndroidPublisherKey(addbuddizz);
        AdBuddizBinding.CacheAds();
    }

    IEnumerator authenticateGoogle()
    {
        Social.localUser.Authenticate((bool success) =>
        {
            print("MFS Login Finished...Success: " + success);
        });
        yield return null;
    }

    void Awake()
    {
        mInstance = this;
    }
    
    #region Unity Ad

    public void DisplayRetryDialog()
    {
        RetryDialog.GetComponent<SessionEndedPanelAnim>().ShowPanel();
        LivesLeftText.text = GameManager.SharedInstance.LivesLeft + " tries left!";
    }

    public void RetryButtonTapped()
    {
        RetryDialog.GetComponent<SessionEndedPanelAnim>().HidePanel();
        GameManager.SharedInstance.LivesLeft--;
        StartCoroutine(displayRewardAdd());
    }

    public void NoRetryButtonTapped()
    {
        RetryDialog.GetComponent<SessionEndedPanelAnim>().HidePanel();
        UserInterface.SharedInstance.DisplaySessionEndedPanel(true, false);
    }

    IEnumerator displayRewardAdd()
    {
        ShowOptions addOptions = new ShowOptions();
        addOptions.resultCallback = RewardAdCallback;
        while (!Advertisement.IsReady())
        {
            yield return null;
        }
        Advertisement.Show("rewardedVideoZone", addOptions);
    }

    void RewardAdCallback(ShowResult result)
    {
        LivesLeftText.text = "Ad Callback";
        gameOverMode = false;
        Debug.Log("Watched add, user can continue...");
        InitiateLevelButtonTapped();
    }
#endregion


    public void DisplaySessionEndedPanel(bool visible, bool didWin)
    {
        RatingImages[0].color = RatingLockedColor;
        RatingImages[1].color = RatingLockedColor;
        RatingImages[2].color = RatingLockedColor;

        SessionEndedPanel.SetActive(true);

        if(visible)
        {
            SessionEndedPanel.GetComponent<SessionEndedPanelAnim>().ShowPanel();
        }
        else
        {
            SessionEndedPanel.GetComponent<SessionEndedPanelAnim>().HidePanel();
        }
        
       
        if(visible)
        {
            Invoke("hideLevelUIAfterSessionEnded", .55f);
            numSessions++;

            if (didWin)
            {
                gameOverMode = false;
                retryOrContinueText.text = "Next Level";
                retryOrNextLevelButton.SetActive(true);
                
                MusicPlayer.SharedInstance.playLevelWinSFX();
                
                    
                DisplayRandomWinText();
                DisplaySessionEndedAnalytics();
                DisplayRating();

                if(GameManager.SharedInstance.levelIndex%3==0 && GameManager.SharedInstance.levelIndex!=0)
                {
                    AdBuddizBinding.ShowAd();
                }
            }
            else
            {
                _disablePortalFinder();
                long scoreToSend = Convert.ToInt64(GameManager.SharedInstance.TotalScore);
                Social.Active.ReportScore(scoreToSend, GooglePlayFeatures.leaderboard_space_pioneers,(bool result) => 
                {
                    if(result)
                    {
                        ScoreValueText.color = Color.yellow;
                        LeaderBoardButton.SetActive(true);
                    }
                    else
                    {
                        LeaderBoardButton.SetActive(false);
                    }
                });

                Debug.Log("Player lost, showing session ended panel");
                retryOrContinueText.text = "Start Over";
                gameOverMode = true;
                DisplayrandomLostText();
                DisplaySessionEndedAnalytics();
            }
        }
    }

    public void hideLevelUIAfterSessionEnded()
    {
        LevelUI.SetActive(false);
    }


    public void DisplayLeaderboard()
    {
        Social.Active.ShowLeaderboardUI();
    }

    public void DisplayRandomWinText()
    {
        SessionEndedTitle.text = winTitles[UnityEngine.Random.Range(0, winTitles.Length - 1)]; ;
    }

    public void DisplayrandomLostText()
    {
        SessionEndedTitle.text = loseTitles[UnityEngine.Random.Range(0, loseTitles.Length - 1)]; ;
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

        float timeLeft = GameManager.SharedInstance.TimeRemaining;
        TimeRemainingPanel.color = (timeLeft<20.0f)?Color.red:Color.white;

        int sec = (int)GameManager.SharedInstance.TimeRemaining % 60;
        int min = (int)GameManager.SharedInstance.TimeRemaining / 60;
        TimeRemainingPanel.text = string.Format("{0}:{1:00}", min, sec);
    }

    public void DisplayPlanetGoalAchievedImages(bool visible)
    {
        foreach (GameObject flag in planetGoalUnlockedImages)
            flag.SetActive(visible);
    }

    int _computeScore()
    {
        float remainingTime = GameManager.SharedInstance.TimeRemaining / GameManager.SharedInstance.CurrentLevel.LevelDuration();
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
        int result;
        int humanDeaths = GameManager.SharedInstance.CurrentLevel.HumanDeaths;
        int planetsLost = GameManager.SharedInstance.CurrentLevel.LostColonies;

        if(humanDeaths==0 && planetsLost==0)
        {
            result=3;
        }
        else if ((humanDeaths/GameManager.SharedInstance.CurrentLevel.MaxPassengerCount)<5)
        {
            result=2;
        }
        else
        {
            result=1;
        }

        if(GameManager.SharedInstance.levelIndex>=3 && result==3 && !PowerUpPanel.active)
        {
            PowerUpPanel.SetActive(true);
            EventManager.PostEvent(EventManager.ePowerUpReceived);
        }

        return result;
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

    public void InitiateLevelButtonTapped()
    {
        if(gameOverMode)
        {
            Application.LoadLevel("ManifestSpaceMain");
        }
        else
        {
            Debug.Log("user moving to next level");
            LevelUI.SetActive(true);
            SessionEndedPanel.GetComponent<SessionEndedPanelAnim>().HidePanel();
            Invoke("_nextLevelTapped", .5f);
        }
    }

    void _nextLevelTapped()
    {
        GameManager.SharedInstance.NextLevelButtonTapped();
    }

    public void QuitButtonTapped()
    {
        Application.LoadLevel("Menu");
    }

    public void UI_PowerUpActivated()
    {
        PowerUpPanel.SetActive(false);
        GameManager.SharedInstance.CurrentSelectedPlanet.GetComponent<Planet>().LaunchMissile(true);
    }

    public void UI_MissileButtonDown()
    {
        
    }

    public void UI_MissileButtonUp()
    {
        bool trigger = missileButtonFill.ProgressBarFull();
        if (!trigger)
            return;

        missileButtonFill.StopRadialFill(false);
        GameManager.SharedInstance.CurrentSelectedPlanet.GetComponent<Planet>().LaunchMissile(false);
        Invoke("RechargeMissile", .35f);
    }

    public void RechargeMissile()
    {
        missileButtonFill.StartRadialFill();
    }

    public void UI_ShipButtonDown()
    {
    }

    public void UI_ShipButtonUp()
    {
        bool trigger = shipButtonFill.ProgressBarFull();
        if (!trigger)
            return;
            
        GameManager.SharedInstance.CurrentSelectedPlanet.GetComponent<Planet>().LaunchCrew();

        shipButtonFill.StopRadialFill(false);
        Invoke("RechargeShips", .35f);
    }

    public void RechargeShips()
    {
        shipButtonFill.StartRadialFill();
    }

    #endregion
}
