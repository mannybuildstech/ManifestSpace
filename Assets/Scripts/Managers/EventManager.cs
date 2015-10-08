using UnityEngine;
using System.Collections;

using UnityEngine.Events;
using System.Collections.Generic;

public class EventManager : MonoBehaviour
{
    private Dictionary<string, UnityEvent> eventDictionary;

    private static EventManager eventManager;

    public static string eAsteroidDangerEvent = "asteroidSpawnedEvent";
	public static string eAsteroidDestroyedEvent = "asteroidDestroyedEvent";
    
    // triggers the following:
    //      portalspawner places portal
    //      portal location indicator      
    //      timer countdown
    
    //      OPTIONAL
    // dramatic music
    // increase gameplay difficulty by:
    //  flipping planet rotations
    //  asteroids become very frequent
    //  other random events later on....
    public static string ePlanetsAquiredEvent        = "planetRequirementComplete";

    //triggers the 'win' window, once user hits next we spawn new planets...
    public static string ePortalEnteredEvent         = "portalEnteredEvent";

    // for levels >0 triggers camera move event
    public static string eSolarSystemDidFinishSpawning = "solarSystemReady";

    // cameracontrol triggers this event once pan animation is done
    // triggers destruction of old objects
    // triggers PortalSpawner to:
    //      generate a portal next to new home
    //      launch rocket from portal to earth so it can get colonized
    public static string eCameraPannedToNewHomeEvent = "pannedToNewHome";

    public static string eNextHomeIsReadyEvent = "colonizedNewHome";

    public static string eGamePausedEvent = "pause";
    public static string eGameResumeEvent = "resume";

    public static string ePowerUpReceived = "powerup";

    public static EventManager instance
    {
        get
        {
            if (!eventManager)
            {
                eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;

                if (!eventManager)
                {
                    Debug.LogError("No active EventManager object was located in the current scene");
                }
                else
                {
                    eventManager.Init();
                }

            }
            return EventManager.eventManager;
        }
    }

    public void Awake()
    {

    }

    public void Update()
    {

    }

    

    void Init()
    {
        if (eventDictionary == null)
        {
            eventDictionary = new Dictionary<string, UnityEvent>();
        }
    }

    public static void StartListening(string eventName, UnityAction listener)
    {
        UnityEvent thisEvent = null;

        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new UnityEvent();
            thisEvent.AddListener(listener);
            instance.eventDictionary.Add(eventName, thisEvent);
        }
    }

    public static void StopListening(string eventName, UnityAction listener)
    {
        if (eventManager == null) return;

        UnityEvent thisEvent = null;

        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
        else
        {
            Debug.LogError("The specified event [" + eventName + "] doesn't exist");
        }
    }

    public static void PostEvent(string eventName)
    {
        UnityEvent thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            //will call the functions of all the listeners that were watching this event
            Debug.LogFormat("EventManager>> event [{0}] was fired", eventName);
            thisEvent.Invoke();
        }
    }
}
