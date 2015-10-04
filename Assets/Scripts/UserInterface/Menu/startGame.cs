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
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
	}
	
	public void Game()
	{
        clipAudioSource.Play();
        startButton.enabled = false;
        Social.localUser.Authenticate((bool success) => 
        {
            startButton.enabled = true;
            Application.LoadLevel("ManifestSpaceMain");//Load Level Async            
            
        });
	}
}
