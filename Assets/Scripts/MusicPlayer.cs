using UnityEngine;
using System.Collections;

public class MusicPlayer : MonoBehaviour {
	
	public AudioClip humanLaunch;
	public AudioClip humanDeath;
	public AudioClip colonized;
	public AudioClip missileLaunch;
	public AudioClip missileBlow;
	public AudioClip rocketThrust;
	public AudioClip asteroidWarningClip;

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
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void humanLaunchSound()
	{
		audioSource.PlayOneShot(humanLaunch);
	}

	public void humansDiedSound()
	{
		audioSource.PlayOneShot(humanDeath);
	}

	public void humansColonizedSound()
	{
		audioSource.PlayOneShot(colonized);
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
		audioSource.PlayOneShot(rocketThrust);	
	}

	public void rocketThrustEnd()
	{
		audioSource.Stop();	
	}

	public void asteroidWarning()
	{
		audioSource.PlayOneShot(asteroidWarningClip);	
	}
}
