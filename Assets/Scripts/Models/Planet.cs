﻿using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Planet : MonoBehaviour 
{
    public  enum PlanetStateEnum { virgin, colonized, destroyed };

    public float minRotationSpeed;
    public float maxRotationSpeed;
    public bool bool_PlanetVisited = false;
    
    public GameObject SpaceStationPrefab;
    private GameObject _spaceStationInstance;
    private CircleCollider2D _planetCollider;
    public GameObject PlanetGlow;

    public Text planetCount;

    public float float_RotationSpeed;
    int int_RotationDirection;

    PlanetStateEnum _currentPlanetState;
    public PlanetStateEnum CurrentPlanetState
    {
        get { return _currentPlanetState; }
    }

    bool cleanupMode = false;

    void Start()
    {
        _planetCollider = GetComponent<CircleCollider2D>();
       
        int_RotationDirection = (Random.Range(0, 2) == 0) ? -1 : 1;
        float_RotationSpeed = (Random.Range(minRotationSpeed * 100, maxRotationSpeed * 100) / 100) * int_RotationDirection;
    }

    void Update()
    {
        if (planetCount!=null)
        {
            gameObject.transform.Rotate(0, 0, float_RotationSpeed);            //rotate at constant speed
            planetCount.text = HumanCount.ToString(); //display current number of humans
        }
    }

    void OnMouseDown()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            SelectPlanet();
        }
    }

    public void SelectPlanet()
    {
        if (!cleanupMode && _currentPlanetState == PlanetStateEnum.colonized)
        {
            MusicPlayer.SharedInstance.playPlanetSelectSFX();
            SetSelectedState(true);
            
        }
    }

    public void DestroyChildren()
    {
        cleanupMode = true;
        Destroy(_spaceStationInstance);
        StartCoroutine(_destroychildren());
    }

    IEnumerator _destroychildren()
    {
        foreach (Transform child in gameObject.transform)
        {
            if (child != null)
                Destroy(child.gameObject);
            yield return null;
        }
        cleanupMode = false;
    }

    public float PlanetRadius
    {
        get { return _planetCollider.radius * gameObject.transform.localScale.y; }
    }

    int _humanCount;
    public int HumanCount
    {
        get { return _humanCount; }
        set 
        {
            if(value<=0)
            {
                _changeToPlanetState(PlanetStateEnum.virgin);
            }
            else if (value>0 & _humanCount==0)
            {
                _changeToPlanetState(PlanetStateEnum.colonized);
            }

            _humanCount = value; 
        }
    }

    private void _changeToPlanetState(PlanetStateEnum newState)
    {
        switch(newState)
        {
            case PlanetStateEnum.virgin:
            {
                _humanCount = 0;
                
                Destroy(_spaceStationInstance);
                
                if(_currentPlanetState==PlanetStateEnum.colonized)
                {
                    GameManager.SharedInstance.CurrentLevel.ColonizedPlanetCount--;
                    SetSelectedState(false);
                }
                break;
            }
            case PlanetStateEnum.colonized:
            {
                _planetCollider = GetComponent<CircleCollider2D>();
                
                //distance from center = planet radius + 1/4 of space station (25% of station is hidden behind planet)
                // multiplied by planet's local scale since station is a child
                Vector3 stationPosition = transform.position+(transform.up*(_planetCollider.radius+(SpaceStationPrefab.transform.localScale.y*.25f))*gameObject.transform.localScale.y);
                _spaceStationInstance = Instantiate(SpaceStationPrefab,stationPosition, Quaternion.identity) as GameObject; 
                _spaceStationInstance.transform.SetParent(gameObject.transform);
                break;
            }
            case PlanetStateEnum.destroyed:
            {
                break;
            }
        }
        _currentPlanetState = newState; 
    }


    #region public methods
    public void LaunchCrew()
    {
        if (_currentPlanetState == PlanetStateEnum.colonized)
        {
            this.gameObject.GetComponentInChildren<SpaceStation>().LaunchHumans(); 
        }
    }

    public void LaunchMissile()
    {
        this.gameObject.GetComponentInChildren<SpaceStation>().launchMissiles(); 
    }

    public void SetSelectedState(bool selected)
    {
        if (selected)
        {
            GameObject curSelected = GameManager.SharedInstance.CurrentSelectedPlanet;
            if(curSelected!=null)
            {
                curSelected.GetComponent<Planet>().PlanetGlow.SetActive(false); //deselect old
            }
            
            PlanetGlow.SetActive(selected); //select new
            GameManager.SharedInstance.CurrentSelectedPlanet = gameObject;
        }
        else
        {
            PlanetGlow.SetActive(selected); //select new
        }
    }

    #endregion
}
