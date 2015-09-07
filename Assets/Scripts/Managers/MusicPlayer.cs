using UnityEngine;
using System.Collections;

public class MusicPlayer : MonoBehaviour 
{
    public AudioSource audioSource;
    public AudioSource PitchChangingSource;

    public AudioClip humanDeath;

    public AudioClip trumpets;
    public AudioClip hurray;

    public AudioClip humanLaunch;
	public AudioClip missileLaunch;

	public AudioClip missileBlow;
	public AudioClip alarmThreat;

    public AudioClip swoosh;
    public AudioClip portal;
    public AudioClip colonizedSound;

    public AudioClip TimeIsRunningOut;

    public float _colonizedPitch;

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
        _colonizedPitch = 1.0f;
	}
	
    //TODO:
    //1) randomize pitch & volume for each sound played
    //2) contain a pool of 3-5 sounds fx for each sound event for variety
    //3) randomize selection of sfx or choose based on state of game, example: "phewww that was a hard one", "we did it!!!", "Ohh noo", "why me!?!?"

    public void planetAchievementSound()
    {
        audioSource.PlayOneShot(trumpets);
    }

    public void playRunningOutOfTimeSFX()
    {
     //   audioSource.PlayOneShot(TimeIsRunningOut);
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
        PitchChangingSource.pitch += .3f;
        if (PitchChangingSource.pitch >= 2.0f)
            PitchChangingSource.pitch = 1.0f;

        PitchChangingSource.PlayOneShot(colonizedSound);        
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
