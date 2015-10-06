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
    int humanPopulation = 0;

    public int HumanPopulation
    {
        get { return humanPopulation; }
        set 
        {
            if(value<humanPopulation)
            {
                HumanDeaths = humanPopulation - value;
            }
            humanPopulation = value; 
        }
    }

    int colonizedPlanets = 0;

    public int ColonizedPlanetCount
    {
        get { return colonizedPlanets; }
        set 
        {
            if(value<colonizedPlanets)
            {
                LostColonies = colonizedPlanets - value;
            }
            colonizedPlanets = value; 
        }
    }

    //analytics
    public int HumanDeaths;
    public int LostColonies;

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
    
    int _levelDurationBase = 25;

    //game starts with possibly up to 5 times the required planets and decreases choice as 
    //it progresses
    int maxPlanetSeedCoefficient = 5;

    //TODO find these values
    int maxAcceptablePlanetScaleDisparity = 20;
    float minPlanetScaleBase = 4.0f;
    float hardestPlanetSpinSpeed = 5.0f;

    float startingMaxAsteroidThreadInterval = 90.0f;

    public float LevelDuration()
    {
        return _levelDurationBase
            + (SystemIndex*2);
    }

    public SolarSystemSeed(int systemIndex)
    {
        SystemIndex = systemIndex;

        //Required Humans
        int decreaseRate = (int)(.2f * Mathf.Pow(systemIndex, 2));
        StartingHumans  = 65+ -decreaseRate;
        StartingHumans = (StartingHumans < 25) ? 25 : StartingHumans; //clamp @ 25

        //Required Planets
        float planetReqIncrease =  systemIndex;
        RequiredPlanets = (int)(reqPlanetsBase + planetReqIncrease);
        if (RequiredPlanets >= maxPlanetsReq)
            RequiredPlanets = maxPlanetsReq;
        
        //Solar System Radius
        SolarSystemRadius = solarSystemRadiusBase + (systemIndex / 2)*10;
        if (SolarSystemRadius > 250.0f)
            SolarSystemRadius = 250.0f;

        MinPlanetCount = RequiredPlanets;

        // the higher the level the less options you will have available to reach the goal
        float inverseOpportunity = -.1f*((float)systemIndex / .5f) + maxPlanetSeedCoefficient;
        if (inverseOpportunity <= 2)
            inverseOpportunity = 2;
        MaxPlanetCount = MinPlanetCount+(int)inverseOpportunity;  

        MinPlanetScale = minPlanetScaleBase;
        float disparity = .01f * Mathf.Pow((float)systemIndex, 2);
        disparity = (disparity >= maxAcceptablePlanetScaleDisparity) ? maxAcceptablePlanetScaleDisparity : disparity;
        MaxPlanetScale = MinPlanetScale + 5.0f + disparity;

        //could do something more interesting here later on...
        MinPlanetDistance = 5.0f + disparity;

        // planet speeds
        float potentialSpeedIncreaseRate = .01f * Mathf.Pow(systemIndex, 2);
        MinRotationSpeed = .75f + potentialSpeedIncreaseRate;
        MaxRotationSpeed =  1f + potentialSpeedIncreaseRate;
        MinRotationSpeed = (MinRotationSpeed >= (hardestPlanetSpinSpeed - 1)) ? hardestPlanetSpinSpeed - 1 : MinRotationSpeed;
        MaxRotationSpeed = (MaxRotationSpeed > hardestPlanetSpinSpeed) ? hardestPlanetSpinSpeed : MaxRotationSpeed; 

        //very slow increase in the intensity of asteroid threat gameplay
        AsteroidThreatMinInterval = 25.0f;
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
            MaxDebriCount = 4;
        }
        else if(systemIndex>10 && systemIndex<12)
        {
            MaxDebriCount = 6;
        }
        else
        {
            MaxDebriCount = 7;
        }

        DebriOrbitRadiusMin = .3f*MinPlanetScale;
        DebriOrbitRadiusMax = .5f*MaxPlanetScale;

        /*
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
         * */
    }
}
