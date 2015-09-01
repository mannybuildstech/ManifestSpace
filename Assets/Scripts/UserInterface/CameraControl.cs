using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {
	
	public float dragSpeed;
	public float originalCameraSize = 5;
	public float MaxZoomLimit = 50;
	public float ZoomMultiplier = 5;

    private Vector2 homePosition;

    public void Start()
    {
        homePosition = GameManager.SharedInstance.HomePosition;
    }
	
	void Update()
	{
	    if (Input.GetKey(KeyCode.LeftArrow)||Input.GetKey(KeyCode.A))
	    {
		    transform.position += Vector3.left * dragSpeed * Time.deltaTime;
	    }
	    if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
	    {
		    transform.position += Vector3.right * dragSpeed * Time.deltaTime;
	    }
	    if (Input.GetKey(KeyCode.W))
	    {
		    transform.position += Vector3.up * dragSpeed * Time.deltaTime;
	    }
	    if (Input.GetKey(KeyCode.S))
	    {
		    transform.position += Vector3.down * dragSpeed * Time.deltaTime;
	    }

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.E))
	    {
		    zoomIN();
	    }
	    if(Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.F))
	    {
		    zoomOut();   
	    }
			
	    float mouseWheel = Input.GetAxis("Mouse ScrollWheel");
			
	    if(mouseWheel<0 && !(Camera.main.orthographicSize<originalCameraSize))
	    {
		    Debug.Log("Mouse wheel: " + mouseWheel);
		    Camera.main.orthographicSize += mouseWheel * ZoomMultiplier;
	    }
			
	    if (mouseWheel > 0 && !(Camera.main.orthographicSize > MaxZoomLimit))
	    {
		    Debug.Log("Mouse wheel: " + mouseWheel);
		    Camera.main.orthographicSize += mouseWheel * ZoomMultiplier;
	    }
	}

	void zoomIN()
	{
		if (Camera.main.orthographicSize < originalCameraSize)
		{
		}
		else
		{
			Camera.main.orthographicSize -= ZoomMultiplier * Time.deltaTime;
		}
	}
	
	void zoomOut()
	{
		if (Camera.main.orthographicSize > MaxZoomLimit)
		{
		}
		else
		{
			Camera.main.orthographicSize += ZoomMultiplier * Time.deltaTime;
		}
	}

    public void CameraMoveHome()
    {
        gameObject.transform.position = new Vector3(homePosition.x, homePosition.y, gameObject.transform.position.z);
    }

    public void CameraMoveAsteroid()
    {
        if (GameManager.SharedInstance.asteroidThreatList.Count > 0)
        {
            GameObject asteroid = (GameObject)GameManager.SharedInstance.asteroidThreatList[0];

            if(asteroid!=null)
            {
                gameObject.transform.position = new Vector3(asteroid.transform.position.x, asteroid.transform.position.y, gameObject.transform.position.z);
            }
        }
    }
}
