using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class AsteroidWarningBehavior : MonoBehaviour 
{
    public void Start()
    {
        GetComponent<Image>().enabled = false;
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.M))
        {
            GetComponent<Image>().enabled = true;
        }
    }

    public void OnEnable()
    {
        EventManager.StartListening(EventManager.eAsteroidSpawnedEvent, newAsteroid);
        EventManager.StartListening(EventManager.eAsteroidDestroyedEvent, lessAsteroids);
    }

    public void OnDisable()
    {
        EventManager.StopListening(EventManager.eAsteroidSpawnedEvent, newAsteroid);
        EventManager.StopListening(EventManager.eAsteroidDestroyedEvent, lessAsteroids);
    }

    public void newAsteroid()
    {
        GetComponent<Image>().enabled = true;
    }

    public void lessAsteroids()
    {
        if (GameManager.SharedInstance.CurrentAsteroids.Length == 0)
            GetComponent<Image>().enabled = false;
    }
}
