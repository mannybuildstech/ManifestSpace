using UnityEngine;
using System.Collections;

public class SolarSystemSeed 
{
    /// <summary>
    /// determines difficulty of system
    /// </summary>
    public int SystemIndex;

    /// <summary>
    /// current level state
    /// </summary>
    public int HumanCount;
    public int PlanetCount = 1;

    /// <summary>
    /// Level Goals
    /// </summary>
    public float  StartingHumans;
    public int    RequiredPlanets;

    /// <summary>
    /// System
    /// </summary>
    public float SolarSystemRadius;
    public int   MinPlanetCount;
    public int   MaxPlanetCount;

    /// <summary>
    /// Planet
    /// </summary>
    public float MinPlanetScale;
    public float MaxPlanetScale;
    public float MinPlanetDistance;
    public float MinRotationSpeed;
    public float MaxRotationSpeed;

    /// <summary>
    /// Asteroid Threat Gameplay
    /// </summary>
    public float AsteroidThreatMinInterval;
    public float AsteroidThreatMaxInterval;

    /// <summary>
    /// Spaceship
    /// </summary>
    public float SpaceshipLifeTime;
    public float MaxPassengerCount;

    /// <summary>
    /// Debri System
    /// </summary>
    public float MinDebriCount;
    public float MaxDebriCount;
    public float MinOrbitRadius;
    public float MaxOrbitRadius;

    /// <summary>
    /// User Control
    /// </summary>
    public float MissileRechargeDuration;
    public float HumanLoadDuration;

    int maxPlanetsReq = 50;
    int reqPlanetsBase = 2;

    float solarSystemRadiusBase = 60.0f;

    //game starts with possibly up to 5 times the required planets and decreases choice as 
    //it progresses
    int maxPlanetSeedCoefficient = 5;

    //TODO find these values
    int maxAcceptablePlanetScaleDisparity = 3;
    float hardestPlanetSpinSpeed = 3.0f;

    float startingMaxAsteroidThreadInterval = 90.0f;

    public SolarSystemSeed solarSystemWithIndex(int systemIndex)
    {
        SolarSystemSeed result = new SolarSystemSeed();
        result.SystemIndex = systemIndex;

        result.StartingHumans  = 25;

        //-.05(x-32)^2+50
        //m = .05 (how many levels it takes to reach the maximum planet requirement
        //b = max planet requirement
        int planetReqIncrease = (int)(-.05 * Mathf.Pow((float)systemIndex - 32, 2) + maxPlanetsReq);
        
        //cap parabolar
        if (systemIndex > maxPlanetsReq)
            planetReqIncrease = maxPlanetsReq;

        result.RequiredPlanets = reqPlanetsBase + planetReqIncrease;

        //for every increase in required planets we add more gamespace so planets aren't so cramped
        result.SolarSystemRadius = solarSystemRadiusBase + planetReqIncrease*Random.Range(1.0f,100.0f);

        result.MinPlanetCount = result.RequiredPlanets;

        // the higher the level the less options you will have available to reach the goal
        float inverseOpportunity = -((float)systemIndex / 10) + maxPlanetSeedCoefficient;
        result.MaxPlanetCount = result.MinPlanetCount+(int)inverseOpportunity;  

        //start with more than minimum planet count (more options)
        // end with a number really close to planet count

        //consider natural log which inherently contains a celing !
        MinPlanetScale = .8f;
        float disparity = .01f * Mathf.Pow((float)systemIndex, 2);
        disparity = (disparity >= maxAcceptablePlanetScaleDisparity) ? maxAcceptablePlanetScaleDisparity : disparity;
        MaxPlanetScale = MinPlanetScale + disparity;

        //could do something more interesting here later on...
        MinPlanetDistance = .25f * MaxPlanetScale;

        // max possibile delta in rotation possibilities achieved by 3rd level
        MinRotationSpeed = .2f;
        MaxRotationSpeed = .6f*Mathf.Pow(systemIndex, 2) + .8f;

        //very slow increase in the intensity of asteroid threat gameplay
        AsteroidThreatMinInterval = 30.0f;
        AsteroidThreatMaxInterval = -.05f * Mathf.Pow(systemIndex, 2) + 90;
        if (AsteroidThreatMaxInterval < AsteroidThreatMinInterval)
            AsteroidThreatMaxInterval = AsteroidThreatMinInterval + 20.0f;

        SpaceshipLifeTime = 7.0f;
        MaxPassengerCount = 5.0f;

        MinDebriCount  = 1;
        MaxDebriCount  = 7;
        MinOrbitRadius = .5f*MinPlanetScale;
        MaxOrbitRadius = .5f*MaxPlanetScale;
        MissileRechargeDuration = 3.0f;
        HumanLoadDuration = 1.0f;

        return result;
    }
}
