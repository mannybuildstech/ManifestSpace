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

    enum CameraMode { touchEnabled, flythrough, flyhome, ToNearestAsteroid };
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
            _flyThroughUpdate();
        }
    }

    void _touchControlUpdate()
    {
        // If there are two touches on the device...
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
            theCamera.orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;
            
            // Make sure the orthographic size never drops below zero.
            theCamera.orthographicSize = Mathf.Clamp(theCamera.orthographicSize, minZoomCameraSize, maxZoomCameraSize);

            
        }
        else if(Input.touchCount==1 || Input.touchCount>2)
        {
            Touch currentTouch = Input.GetTouch(0);
            Vector2 previousTouchVector = currentTouch.position - currentTouch.deltaPosition;
            Vector2 differenceVector = currentTouch.position - previousTouchVector;

            transform.position = new Vector3((transform.position.x - differenceVector.x),(transform.position.y - differenceVector.y), transform.position.z);
        }
    }
    
    void _flyThroughUpdate()
    {
        Vector3 home = new Vector3(GameManager.SharedInstance.CurrentHomePosition.x, GameManager.SharedInstance.CurrentHomePosition.y,gameObject.transform.position.z);
        Vector3 targetLocation = (_currentCameraMode==CameraMode.ToNearestAsteroid)?_nearestAsteroidLocation():home;

        gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position,home, Time.time*flythroughSpeed);
         
        if((Vector2)gameObject.transform.position == GameManager.SharedInstance.CurrentHomePosition)
        {
            /// Next level is in camera view let's signal its okay to start!
            if(_currentCameraMode==CameraMode.flythrough)
            {
                EventManager.PostEvent(EventManager.eCameraPannedToNewHomeEvent);
            }
            _currentCameraMode = CameraMode.touchEnabled;
        }
    }

    public void PanToNewSystemLocation()
    {
        if(GameManager.SharedInstance.levelIndex>0)
            _currentCameraMode = CameraMode.flythrough;
    }

    public void PanHome()
    {
        _currentCameraMode = CameraMode.flyhome;
    }

    public void PanToAsteroid()
    {
        _currentCameraMode = CameraMode.ToNearestAsteroid;
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

    void _restoreCameraControl()
    {
        _currentCameraMode = CameraMode.touchEnabled;
    }
}
