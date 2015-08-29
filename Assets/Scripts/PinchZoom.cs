using UnityEngine;
using System.Collections;

public class PinchZoom : MonoBehaviour 
{
    public float minZoomCameraSize = .2f;
    public float orthoZoomSpeed = 1.5f;        // The rate of change of the orthographic size in orthographic mode.
    public float dragSpeed = .5f;
    Camera camera;

    void Start()
    {
        camera = GetComponent<Camera>();
    }

    void Update()
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
            camera.orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;
            
            // Make sure the orthographic size never drops below zero.
            camera.orthographicSize = Mathf.Max(camera.orthographicSize, 0.1f);

            Mathf.Clamp(camera.orthographicSize, minZoomCameraSize, (float)GameManager.SharedInstance.SolarSystemRadius);
        }
        else
        {
            Touch currentTouch = Input.GetTouch(0);
            Vector2 previousTouchVector = currentTouch.position - currentTouch.deltaPosition;
            Vector2 differenceVector = currentTouch.position - previousTouchVector;

            transform.position = new Vector3((transform.position.x - differenceVector.x),(transform.position.y - differenceVector.y), transform.position.z);
        }
    }
}
