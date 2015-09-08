using UnityEngine;
using System.Collections;

public class MusicPlayer : MonoBehaviour 
{
    public AudioSource audioSource;
    public AudioSource PitchChangingSource;

    public AudioClip humanDeath;

    public AudioClip portalHum;

    public AudioClip trumpets;
    public AudioClip hurray;

	public AudioClip launch;

	public AudioClip missileBlow;
	public AudioClip alarmThreat;

    public AudioClip swoosh;
    public AudioClip portal;
    public AudioClip colonizedSound;

    public AudioClip TimeIsRunningOut;

    public AudioClip popSound;
    public AudioClip astroidCrash;

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
        audioSource.volume = .3f;
        audioSource.PlayOneShot(hurray);

    }

    public void playPlanetSelectSFX()
    {
        audioSource.PlayOneShot(swoosh);
    }

	public void humanLaunchSound()
	{
		audioSource.PlayOneShot(launch);
	}

	public void humansDiedSound()
	{
        PitchChangingSource.pitch = Random.Range(.5f, 2.5f);
        PitchChangingSource.pitch = Random.Range(.3f, 1.3f);
		audioSource.PlayOneShot(humanDeath);
	}

    public void playColonizedSound()
    {
        PitchChangingSource.volume = 1.0f;
        PitchChangingSource.pitch += .3f;
        if (PitchChangingSource.pitch >= 3.0f)
            PitchChangingSource.pitch = 2.0f;
        PitchChangingSource.PlayOneShot(colonizedSound);        
    }

    public void asteroidBlowSound()
    {
        PitchChangingSource.volume = Random.Range(.5f,1.0f);
        PitchChangingSource.pitch = Random.Range(1f, 2.0f);
        PitchChangingSource.PlayOneShot(astroidCrash);
    }

	public void missileLaunchSound()
	{
        audioSource.PlayOneShot(launch);
	}

	public void missileBlowSound()
	{
        audioSource.PlayOneShot(missileBlow);
	}
	
	public void asteroidWarning()
	{
		audioSource.PlayOneShot(alarmThreat);	
	}

    public void portalBackgroundHum(bool start)
    {
        if(start)
        {
            PitchChangingSource.volume = 1.5f;
            PitchChangingSource.pitch = .5f;
            PitchChangingSource.PlayOneShot(portalHum);
        }
        else
        {
            PitchChangingSource.volume = 1.5f;
            PitchChangingSource.pitch = 1.5f;
            PitchChangingSource.PlayOneShot(portalHum);
        }
    }

    public void PlayPlanetRatingSound(int ratingLevel)
    {
        switch (ratingLevel)
        {
            case 0:
                PitchChangingSource.pitch = .5f;
                break;
            case 1:
                PitchChangingSource.pitch = 1f;
                break;
            case 2:
                PitchChangingSource.pitch = 2f;
                break;
        }
        PitchChangingSource.PlayOneShot(popSound);
    }
}
