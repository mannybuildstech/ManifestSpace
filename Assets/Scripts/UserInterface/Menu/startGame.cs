using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class startGame : MonoBehaviour 
{
    public Text Title;
    public Button startButton;

    public AudioSource clipAudioSource;

	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

	public void Game()
	{
        startButton.GetComponentInChildren<Text>().text = "";
        Title.text = "";
        clipAudioSource.Play();
        Invoke("run", 1.0f);
	}

   
    public void run()
    {
        Application.LoadLevel("Game");
        
    }
}
