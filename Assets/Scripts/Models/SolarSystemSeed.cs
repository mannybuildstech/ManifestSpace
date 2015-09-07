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
    public int HumanPopulation;
    public int ColonizedPlanetCount = 0;

    /// <summary>
    /// Level Goals
    /// </summary>
    public int  StartingHumans;
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
    public int MaxPassengerCount;

    /// <summary>
    /// Debri System
    /// </summary>
    public int   MinDebriCount;
    public int   MaxDebriCount;
    public float DebriOrbitRadiusMin;
    public float DebriOrbitRadiusMax;

    /// <summary>
    /// User Control
    /// </summary>
    public float MissileRechargeDuration;
    public float HumanLoadDuration;

    int maxPlanetsReq = 50;
    int reqPlanetsBase = 2;

    float solarSystemRadiusBase = 55.0f;
    
    int _levelDurationBase = 1 * 60;

    //game starts with possibly up to 5 times the required planets and decreases choice as 
    //it progresses
    int maxPlanetSeedCoefficient = 5;

    //TODO find these values
    int maxAcceptablePlanetScaleDisparity = 20;
    float minPlanetScaleBase = 3.5f;
    float hardestPlanetSpinSpeed = 5.0f;

    float startingMaxAsteroidThreadInterval = 90.0f;

    public float LevelDuration()
    {
        return _levelDurationBase
            + (SystemIndex *2);
    }

    public SolarSystemSeed(int systemIndex)
    {
        SystemIndex = systemIndex;

        //every other level you decrease by 5
        int decreaseRate = (int)(.2f * Mathf.Pow(systemIndex, 2));

        /*
         * takes too long for bigger levels...
         */
        StartingHumans  = 65+ -decreaseRate;
        StartingHumans = (StartingHumans < 25) ? 25 : StartingHumans; //clamp @ 25

        //-.05(x-32)^2+50
        //m = .05 (how many levels it takes to reach the maximum planet requirement
        //b = max planet requirement
        float x = ((float)systemIndex)-31.5f;
        int planetReqIncrease = (int)(-.05f*Mathf.Pow(x,2) + maxPlanetsReq);
        
        //cap parabolar
        if (planetReqIncrease >= maxPlanetsReq)
            planetReqIncrease = 0;

        RequiredPlanets = reqPlanetsBase + planetReqIncrease;
    
        //for every increase in required planets we add more gamespace so planets aren't so cramped
        SolarSystemRadius = solarSystemRadiusBase + (planetReqIncrease / 2)*10;
        if (SolarSystemRadius > 250.0f)
            SolarSystemRadius = 250.0f;

        MinPlanetCount = RequiredPlanets;

        // the higher the level the less options you will have available to reach the goal
        float inverseOpportunity = -.1f*((float)systemIndex / .5f) + maxPlanetSeedCoefficient;
        if (inverseOpportunity <= 2)
            inverseOpportunity = 2;
        MaxPlanetCount = MinPlanetCount+(int)inverseOpportunity;  

        //start with more than minimum planet count (more options)
        // end with a number really close to planet count

        //consider natural log which inherently contains a celing !
        MinPlanetScale = 3.5f;
        float disparity = .01f * Mathf.Pow((float)systemIndex, 2);
        disparity = (disparity >= maxAcceptablePlanetScaleDisparity) ? maxAcceptablePlanetScaleDisparity : disparity;
        MaxPlanetScale = minPlanetScaleBase + 1f + disparity;

        //could do something more interesting here later on...
        MinPlanetDistance = 2.5f + disparity;

        // max possibile delta in rotation possibilities achieved by 3rd level
        MinRotationSpeed = .5f;
        MaxRotationSpeed = .01f*Mathf.Pow(systemIndex, 2) + 1f;
        MaxRotationSpeed = (MaxRotationSpeed > hardestPlanetSpinSpeed) ? hardestPlanetSpinSpeed : MaxRotationSpeed; //cap value

        //very slow increase in the intensity of asteroid threat gameplay
        AsteroidThreatMinInterval = 30.0f;
        AsteroidThreatMaxInterval = -.05f * Mathf.Pow(systemIndex, 2) + 90;
        if (AsteroidThreatMaxInterval < AsteroidThreatMinInterval)
            AsteroidThreatMaxInterval = AsteroidThreatMinInterval + 20.0f;

        SpaceshipLifeTime = 7.0f;
        MaxPassengerCount = 5;

        /////DEBRI
        MinDebriCount  = 1;
        if(systemIndex<5)
        {
            MaxDebriCount = 3;
        }
        else if(systemIndex>5 && systemIndex<10)
        {
            MaxDebriCount = 5;
        }
        else if(systemIndex>10 && systemIndex<12)
        {
            MaxDebriCount = 6;
        }
        else
        {
            MaxDebriCount = 10;
        }

        if (MaxDebriCount > 10)
            MaxDebriCount = 10;

        DebriOrbitRadiusMin = .3f*MinPlanetScale;
        DebriOrbitRadiusMax = .5f*MaxPlanetScale;

        MissileRechargeDuration = 3.0f;
        HumanLoadDuration = 1.0f;

        Debug.Log("Solar System Parameters...");
        Debug.Log("HumanPopulation: "+HumanPopulation);
        Debug.Log("ColonizedPlanetCount: "+ColonizedPlanetCount);
        Debug.Log("StartingHumans: "+StartingHumans);
        Debug.Log("RequiredPlanets: "+RequiredPlanets);
        Debug.Log("SolarSystemRadius: "+SolarSystemRadius);
        Debug.Log("MinPlanetCount: "+MinPlanetCount);
        Debug.Log("MaxPlanetCount: "+MaxPlanetCount);
        Debug.Log("MinPlanetScale: "+MinPlanetScale);
        Debug.Log("MaxPlanetScale: "+MaxPlanetScale);
        Debug.Log("MinPlanetDistance: "+MinPlanetDistance);
        Debug.Log("MinRotationSpeed: "+MinRotationSpeed);
        Debug.Log("MaxRotationSpeed: "+MaxRotationSpeed);
        Debug.Log("MaxPassengerCount: " + MaxPassengerCount);
        Debug.Log("SpaceshipLifeTime: " + SpaceshipLifeTime);
        Debug.Log("MaxPassengerCount: "+MaxPassengerCount);
        Debug.Log("SpaceshipLifeTime: "+SpaceshipLifeTime);
        Debug.Log("MinDebriCount: "+MinDebriCount);
        Debug.Log("MaxDebriCount: "+MaxDebriCount);
        Debug.Log("DebriOrbitRadiusMin: "+DebriOrbitRadiusMin);
        Debug.Log("DebriOrbitRadiusMin: "+DebriOrbitRadiusMin);
        Debug.Log("DebriOrbitRadiusMax: "+DebriOrbitRadiusMax);
        Debug.Log("MissileRechargeDuration: "+MissileRechargeDuration);
        Debug.Log("HumanLoadDuration: "+HumanLoadDuration);
    }
}
