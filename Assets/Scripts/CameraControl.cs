using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {
	
	public float dragSpeed;
	private Vector3 dragLocation;
	
	bool moveTowardsMode = false;
	Vector2 asteroidPosition;
	
	public float originalCameraSize = 5;
	public float MaxZoomLimit = 50;
	public float ZoomMultiplier = 5;
	
	public void PanNearCurrentAsteroid()
	{
		asteroidPosition = GameManager.SharedInstance.CurrentAsteroids[0].transform.position;
		if(asteroidPosition!=null)
			moveTowardsMode = true;
	}
	
	void Update()
	{
		/// Automatic panning towards the current asteroid thread
		if(moveTowardsMode)
		{
			transform.position = new Vector3(asteroidPosition.x,asteroidPosition.y,transform.position.z);
			//transform.position +=(Vector3)Vector2.MoveTowards(transform.position, new Vector3(asteroidPosition.x,asteroidPosition.y,transform.position.z), 30 * Time.deltaTime);
			if(true)
				moveTowardsMode = false;
		}
		/// Allow user to drag around the map
		else
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
			
			if(Input.GetKey(KeyCode.UpArrow))
			{
				zoomIN();
			}
			if(Input.GetKey(KeyCode.DownArrow))
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
			
			
			
			/*
            if (Input.GetMouseButtonDown(0))
            {
                dragLocation = Input.mousePosition;
                return;
            }

            if (!Input.GetMouseButton(0)) return;

            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragLocation);
            //Vector3 move = new Vector3(pos.x * dragSpeed, 0, pos.y * dragSpeed);
            //transform.Translate(new Vector3(-pos.x*dragSpeed,-pos.y*dragSpeed,-pos.z), Space.World);

            transform.position += new Vector3(-pos.x, -pos.y, -pos.z) * 25.0f;
             */
		}
		
	}
	
	void zoomIN()
	{
		if (Camera.main.orthographicSize < originalCameraSize)
		{
			//Camera.main.orthographicSize = originalCameraSize;
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
			//Camera.main.orthographicSize = MaxZoomLimit;
		}
		else
		{
			Camera.main.orthographicSize += ZoomMultiplier * Time.deltaTime;
		}
	}
}
