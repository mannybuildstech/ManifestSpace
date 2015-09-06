using UnityEngine;
using System.Collections;

public class PortalSpawner : MonoBehaviour
{
    public GameObject PortalPrefab;
    public GameObject SpaceshipPrefab;

    public float PortalDistanceFromFurthestPlanet = 30.0f;
    
    public GameObject CurrentPortal;
    public GameObject CurrentTransitionShip;

    public void OnEnable()
    {
        EventManager.StartListening(EventManager.ePlanetsAquiredEvent, GenerateNextLevelPortal);
        EventManager.StartListening(EventManager.eCameraPannedToNewHomeEvent,GenerateLandingSequence);
    }

    public void OnDisable()
    {
        EventManager.StopListening(EventManager.ePlanetsAquiredEvent, GenerateNextLevelPortal);
        EventManager.StopListening(EventManager.eCameraPannedToNewHomeEvent, GenerateLandingSequence);
    }

    public void GenerateNextLevelPortal()
    {
        if (CurrentPortal == null)
        {
            GameObject furthestPlanet = gameObject.GetComponent<SolarSystemGenerator>().FurthestPlanetFromOrigin();

            float magnitudeIncrease = (furthestPlanet.transform.position.magnitude + PortalDistanceFromFurthestPlanet) / furthestPlanet.transform.position.magnitude;
            Vector2 portalPosition = furthestPlanet.transform.position * magnitudeIncrease;
            CurrentPortal = Instantiate(PortalPrefab, portalPosition, Quaternion.identity) as GameObject;
        }
    }

    public void GenerateLandingSequence()
    {
        GameObject home = gameObject.GetComponent<SolarSystemGenerator>().CurrentHomePlanet();
        float magnitudeIncrease = (home.transform.position.magnitude + (PortalDistanceFromFurthestPlanet/2)) / home.transform.position.magnitude;
        Vector2 portalPosition = home.transform.position * magnitudeIncrease;
        CurrentPortal = Instantiate(PortalPrefab, portalPosition, Quaternion.identity) as GameObject;

        //StartCoroutine(closePortal());
        Invoke("LaunchShip", 1.5f);
    }

    public void LaunchShip()
    {
        CurrentPortal.GetComponentInChildren<Collider2D>().enabled = false;
        CurrentPortal.GetComponentInChildren<MeshRenderer>().enabled = false;
        CurrentPortal.GetComponentInChildren<SpriteRenderer>().enabled = false;

        SpaceshipPrefab.GetComponent<Projectile>().currentProjectileType = Projectile.ProjectileType.portalspaceship;
        Vector3 shipPosition = new Vector3(CurrentPortal.transform.position.x, CurrentPortal.transform.position.y, 0.0f);
        CurrentTransitionShip = Instantiate(SpaceshipPrefab, shipPosition, Quaternion.identity) as GameObject;

        MusicPlayer.SharedInstance.playPortalEnteredSFX();

        Destroy(CurrentPortal);
    }
}
