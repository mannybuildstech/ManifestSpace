using UnityEngine;
using System.Collections;

using UnityEngine.UI;

using GooglePlayGames;
using UnityEngine.SocialPlatforms;

public class startGame : MonoBehaviour 
{
    public Text Title;
    public Button startButton;

    public AudioSource clipAudioSource;

	// Use this for initialization
	void Start () 
    {
        
	}
	
	public void Game()
	{
        startButton.enabled = false;
        Social.localUser.Authenticate((bool success) => 
        {
            startButton.enabled = true;
            Social.ShowLeaderboardUI();
        });

        /*
        startButton.GetComponentInChildren<Text>().text = "";
        Title.text = "";
        clipAudioSource.Play();
        Invoke("run", 1.0f);
         */
	}

   
    public void run()
    {
        //Load Level Async?
        Application.LoadLevel("Game");
        
    }
}
