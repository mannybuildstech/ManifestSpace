using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class Planet : MonoBehaviour 
{
    public  enum PlanetStateEnum { virgin, colonized, destroyed };

    public GameObject SelectedPlanetGlow;

    public float minRotationSpeed;
    public float maxRotationSpeed;
    public bool bool_PlanetVisited = false;
    
    public GameObject SpaceStationPrefab;
    private GameObject _spaceStationInstance;
    private CircleCollider2D _planetCollider;

    bool cleanupMode = false;

    void Start()
    {
        _planetCollider = GetComponent<CircleCollider2D>();

        int_RotationDirection = (Random.Range(0, 2) == 0) ? -1 : 1;
        float_RotationSpeed = (Random.Range(minRotationSpeed * 100, maxRotationSpeed * 100) / 100) * int_RotationDirection;
    }

    void Update()
    {
        if(!cleanupMode)
        {
            gameObject.transform.Rotate(0, 0, float_RotationSpeed);            //rotate at constant speed
            GetComponentInChildren<TextMesh>().text = HumanCount.ToString(); //display current number of humans
        }
    }

    void OnMouseDown()
    {
        if(!cleanupMode)
        {
            if (_currentPlanetState == PlanetStateEnum.colonized)
            {
            if (GameManager.SharedInstance.CurrentSelectedPlanet != null)
                GameManager.SharedInstance.CurrentSelectedPlanet.GetComponent<Planet>().SetSelectedState(false);

            GameManager.SharedInstance.CurrentSelectedPlanet = gameObject;
            SetSelectedState(true);
            }
        }
    }

    public void DestroyChildren()
    {
        cleanupMode = true;
        if (_spaceStationInstance != null)
            Destroy(_spaceStationInstance);
        StartCoroutine(_destroychildren());
    }

    IEnumerator _destroychildren()
    {
        foreach (Transform child in gameObject.transform)
        {
            if (child != null)
                GameObject.Destroy(child.gameObject);
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
            if(value==0)
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

    public float float_RotationSpeed;
	int int_RotationDirection;

    PlanetStateEnum _currentPlanetState;
    public PlanetStateEnum CurrentPlanetState
    {
        get { return _currentPlanetState; }
    }

    private void _changeToPlanetState(PlanetStateEnum newState)
    {
        switch(newState)
        {
            case PlanetStateEnum.virgin:
            {
                _humanCount = 0;
                if(_spaceStationInstance)
                    Destroy(_spaceStationInstance);
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
            if (HumanCount > 0)
            {
                if (HumanCount == 5)
                {
                    Destroy(this.gameObject.transform.GetChild(1).gameObject);
                    GameManager.SharedInstance.CurrentLevel.ColonizedPlanetCount -= 1;
                    _currentPlanetState = PlanetStateEnum.virgin;
                }
                this.gameObject.GetComponentInChildren<SpaceStation>().launchHumans(); 
            }
        }
    }

    public void LaunchMissile()
    {
        this.gameObject.GetComponentInChildren<SpaceStation>().launchMissiles(); 
    }

    public void SetSelectedState(bool selected)
    {
        if(SelectedPlanetGlow!=null)
        {
            SelectedPlanetGlow.SetActive(selected);
        }
        else
        {
            Debug.Log("why is glow object null?");
        }
        
    }

    #endregion
}
