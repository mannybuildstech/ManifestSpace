using UnityEngine;
using System.Collections;

using UnityEngine.UI;

using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;

public class startGame : MonoBehaviour 
{
    public Text Title;
    public Button startButton;

    public AudioSource clipAudioSource;

	// Use this for initialization
	void Start () 
    {
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
        // enables saving game progress.
        .EnableSavedGames()
        .Build();

    PlayGamesPlatform.InitializeInstance(config);
    // recommended for debugging:
    PlayGamesPlatform.DebugLogEnabled = true;
    // Activate the Google Play Games platform
    PlayGamesPlatform.Activate();
	}
	
	public void Game()
	{
        clipAudioSource.Play();
        startButton.enabled = false;
        Application.LoadLevel("ManifestSpaceMain");
	}
}
