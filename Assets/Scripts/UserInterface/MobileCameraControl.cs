using UnityEngine;
using System.Collections;

public class MobileCameraControl : MonoBehaviour 
{
    public float minZoomCameraSize;
    public float maxZoomCameraSize; 
    public float orthoZoomSpeed;        // The rate of change of the orthographic size in orthographic mode.
    public float dragSpeed;

    public float flythroughSpeed;

    private Camera theCamera;

    public enum CameraMode { touchEnabled, panNewHome, panHome, panAsteroid, panColony, panPortal};
    CameraMode _currentCameraMode;
    
    public void OnEnable()
    {
        EventManager.StartListening(EventManager.eNextHomeIsReadyEvent, _restoreCameraControl);
    }

    public void OnDisable()
    {
        EventManager.StartListening(EventManager.eNextHomeIsReadyEvent, _restoreCameraControl);
    }

    void Start()
    {
        _currentCameraMode = CameraMode.touchEnabled;
        theCamera = GetComponent<Camera>();
    }

    void Update()
    {
        if(_currentCameraMode==CameraMode.touchEnabled)
        {
            _touchControlUpdate();
        }
        else
        {
            _panModeUpdate();
        }
    }

    void _touchControlUpdate()
    {
        if (Input.touchCount == 2)
        {
            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
            
            // ... change the orthographic size based on the change in distance between the touches.

            float newOrthographicSize = theCamera.orthographicSize + deltaMagnitudeDiff * orthoZoomSpeed;
            
            // Make sure the orthographic size never drops below zero.
            newOrthographicSize = Mathf.Clamp(newOrthographicSize, minZoomCameraSize, maxZoomCameraSize);

            StartCoroutine(_changeCameraSize(newOrthographicSize));
        }
        else if(Input.touchCount==1 || Input.touchCount>2)
        {
            Touch currentTouch = Input.GetTouch(0);
            Vector2 previousTouchVector = currentTouch.position - currentTouch.deltaPosition;
            Vector2 differenceVector = (currentTouch.position - previousTouchVector)*dragSpeed;
            transform.position = new Vector3((transform.position.x - differenceVector.x), (transform.position.y - differenceVector.y), transform.position.z);
        }
    }
    
    void _panModeUpdate()
    {
        Vector3 targetLocation = _targetLocationForMode();
        gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position,targetLocation, Time.time*flythroughSpeed);
         
        if((Vector2)gameObject.transform.position == (Vector2)targetLocation)
        {
            Debug.Log(">> Camera Panned to Target Location:[" + targetLocation + "]");
            
            /// Next level is in camera view let's signal its okay to land humans!
            if(_currentCameraMode==CameraMode.panNewHome)
            {
                EventManager.PostEvent(EventManager.eCameraPannedToNewHomeEvent);
            }

            _currentCameraMode = CameraMode.touchEnabled;
        }
    }

    Vector3 _targetColonizedPlanet;
    Vector3 _targetLocationForMode()
    {
        Vector3 result;
        switch (_currentCameraMode)
        {
            case CameraMode.panAsteroid:
                result = _nearestAsteroidLocation();
                break;
            case CameraMode.panColony:
                result = new Vector3(_targetColonizedPlanet.x,_targetColonizedPlanet.y,gameObject.transform.position.z);
                break;
            case CameraMode.panPortal:
                result = _portalLocation();
                break;
            default:
                result = new Vector3(GameManager.SharedInstance.CurrentHomePosition.x, GameManager.SharedInstance.CurrentHomePosition.y, gameObject.transform.position.z);
                break;
        }
        return result;
    }

    public void StartPanMode(CameraMode mode)
    {
        _currentCameraMode = mode;
    }

    public void PanAsteroid()
    {
        StartPanMode(CameraMode.panAsteroid);
    }

    public void PanPortal()
    {
        StartPanMode(CameraMode.panPortal);
    }

    public void CycleColonies()
    {
        SolarSystemGenerator solarSystem = GameManager.SharedInstance.SpawnerObject.GetComponent<SolarSystemGenerator>();
        GameObject planetObject = solarSystem.GetNextColonizedPlanet();
        _targetColonizedPlanet = planetObject.transform.position;
        StartPanMode(CameraMode.panColony);

        planetObject.GetComponent<Planet>().SelectPlanet();
    }
   
    Vector3 _nearestAsteroidLocation()
    {
        Vector3 result = new Vector3(GameManager.SharedInstance.CurrentHomePosition.x, GameManager.SharedInstance.CurrentHomePosition.y, gameObject.transform.position.z);
        
        if (GameManager.SharedInstance.AsteroidThreatList.Count > 0)
        {
            GameObject asteroid = (GameObject)GameManager.SharedInstance.AsteroidThreatList[0];
            if (asteroid != null)
            {
                result = new Vector3(asteroid.transform.position.x, asteroid.transform.position.y, gameObject.transform.position.z);
            }
        }
        else
        {
            Debug.LogError("While panning to asteroid threat, unable to find its location. Panned home instead");
        }
        return result;
    }

    Vector3 _portalLocation()
    {
        GameObject portal = GameManager.SharedInstance.SpawnerObject.GetComponent<PortalSpawner>().CurrentPortal;
        return new Vector3(portal.transform.position.x,portal.transform.position.y,transform.position.z);
    }

    void _restoreCameraControl()
    {
        _currentCameraMode = CameraMode.touchEnabled;
    }
    
    private IEnumerator _changeCameraSize(float value)
    {
        yield return new WaitForEndOfFrame();
        Camera.main.orthographicSize = value;
    }
}
