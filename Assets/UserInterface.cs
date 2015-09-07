using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class UserInterface : MonoBehaviour 
{
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

    public Text TimeRemainingPanel;

    private static UserInterface mInstance;
    public static UserInterface SharedInstance { get {return UserInterface.mInstance;}}

    void Awake()
    {
        mInstance = this;
    }

    public void DisplaySessionEndedPanel(bool visible, bool didWin)
    {
        SessionEndedPanel.SetActive(visible);
        
        if(visible)
        {
            if(didWin)
            {
                DisplayRandomWinText();
                DisplaySessionEndedAnalytics();
            }
            else
            {
                DisplayrandomLostText();
                DisplaySessionEndedAnalytics();
            }
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
        ScoreValueText.text = _computeScore();
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

    string _computeScore()
    {
        float remainingTime = GameManager.SharedInstance.TimeLeft/GameManager.SharedInstance.CurrentLevel.LevelDuration();
        if (remainingTime < 0)
            remainingTime = 1;
        float score = remainingTime * ((GameManager.SharedInstance.CurrentLevel.HumanPopulation * GameManager.SharedInstance.CurrentLevel.ColonizedPlanetCount));
        GameManager.SharedInstance.TotalScore += score;
        int result = (int)score;
        return result.ToString();
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
        GameManager.SharedInstance.NextLevelButtonTapped();
    }

    public void QuitButtonTapped()
    {
        Application.LoadLevel("Menu");
    }

    public void UI_MissileButtonDown()
    {
        GameManager.SharedInstance.CurrentSelectedPlanet.GetComponent<Planet>().GetComponentInChildren<SpaceStation>().ConfigureSightLine(true);
    }

    public void UI_MissileButtonUp()
    {
        GameManager.SharedInstance.CurrentSelectedPlanet.GetComponent<Planet>().GetComponentInChildren<SpaceStation>().ConfigureSightLine(false);
        GameManager.SharedInstance.CurrentSelectedPlanet.GetComponent<Planet>().LaunchMissile();
    }

    public void UI_ShipButtonDown()
    {
        GameManager.SharedInstance.CurrentSelectedPlanet.GetComponent<Planet>().GetComponentInChildren<SpaceStation>().ConfigureSightLine(true);
    }

    public void UI_ShipButtonUp()
    {
        GameManager.SharedInstance.CurrentSelectedPlanet.GetComponent<Planet>().GetComponentInChildren<SpaceStation>().ConfigureSightLine(false);
        GameManager.SharedInstance.CurrentSelectedPlanet.GetComponent<Planet>().LaunchCrew();
    }

    #endregion
}
