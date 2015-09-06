using UnityEngine;
using System.Collections;

public class MusicPlayer : MonoBehaviour 
{	
	
    public AudioClip humanDeath;

    public AudioClip trumpets;
    public AudioClip hurray;

    public AudioClip humanLaunch;
	public AudioClip missileLaunch;

	public AudioClip missileBlow;
	public AudioClip alarmThreat;

    public AudioClip swoosh;
    public AudioClip portal;

	AudioSource audioSource;

	//currently assuming the singleton object is instantiated before it gets accessed!
	public static MusicPlayer SharedInstance
	{
		get
		{
			return MusicPlayer.mInstance;
		}
	}
	
	private static MusicPlayer mInstance;
	
	void Awake()
	{
		mInstance = this;
	}

	// Use this for initialization
	void Start () {
		audioSource = GetComponent<AudioSource>();
	}
	
    //TODO:
    //1) randomize pitch & volume for each sound played
    //2) contain a pool of 3-5 sounds fx for each sound event for variety
    //3) randomize selection of sfx or choose based on state of game, example: "phewww that was a hard one", "we did it!!!", "Ohh noo", "why me!?!?"

    public void planetAchievementSound()
    {
        audioSource.PlayOneShot(trumpets);
    }

    public void playPortalEnteredSFX()
    {
        audioSource.PlayOneShot(portal);
    }

    public void playLevelWinSFX()
    {
        audioSource.PlayOneShot(hurray);
    }

    public void playPlanetSelectSFX()
    {
        audioSource.PlayOneShot(swoosh);
    }

	public void humanLaunchSound()
	{
		audioSource.PlayOneShot(humanLaunch);
	}

	public void humansDiedSound()
	{
		audioSource.PlayOneShot(humanDeath);
	}

    public void playColonizedSound()
    {
        
    }

	public void humansColonizedSound()
	{
        audioSource.PlayOneShot(humanLaunch);
	}

	public void missileLaunchSound()
	{
        audioSource.PlayOneShot(missileLaunch);
	}

	public void missileBlowSound()
	{
		audioSource.PlayOneShot(missileLaunch);	
	}
	
	public void rocketThrustSoundStart()
	{
	}

	public void rocketThrustEnd()
	{
		audioSource.Stop();	
	}

	public void asteroidWarning()
	{
		audioSource.PlayOneShot(alarmThreat);	
	}
}
