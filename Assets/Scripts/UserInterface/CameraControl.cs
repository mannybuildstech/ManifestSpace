using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour
{
    public float dragSpeed;
    public float originalCameraSize = 5;
    public float MaxZoomLimit = 50;
    public float ZoomMultiplier = 5;

    public GameObject spaceBackgroundObject;
    public Material backgroundMaterial;
    public Vector3 previousCamPos;

    public void Start()
    {
        backgroundMaterial = spaceBackgroundObject.GetComponent<Renderer>().material;
        previousCamPos = Vector3.zero;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
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
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.F))
        {
            zoomOut();
        }

        float mouseWheel = Input.GetAxis("Mouse ScrollWheel");

        if (mouseWheel < 0 && !(Camera.main.orthographicSize < originalCameraSize))
        {
            Debug.Log("Mouse wheel: " + mouseWheel);
            StartCoroutine(_changeCameraSize(Camera.main.orthographicSize + mouseWheel * ZoomMultiplier));
        }

        if (mouseWheel > 0 && !(Camera.main.orthographicSize > MaxZoomLimit))
        {
            Debug.Log("Mouse wheel: " + mouseWheel);
            StartCoroutine(_changeCameraSize(Camera.main.orthographicSize + mouseWheel * ZoomMultiplier));
        }
        
        Vector2 curOffset = backgroundMaterial.GetTextureOffset("_MainTex");

        Vector3 camPosDelta = transform.position - previousCamPos;
        backgroundMaterial.SetTextureOffset("_MainTex", curOffset+(Vector2)(camPosDelta)*.005f);
        previousCamPos = transform.position;
    }
    
    void zoomIN()
    {
        if (Camera.main.orthographicSize < originalCameraSize)
        {
        }
        else
        {
            StartCoroutine(_changeCameraSize(Camera.main.orthographicSize - ZoomMultiplier * Time.deltaTime));
        }
    }

    void zoomOut()
    {
        if (Camera.main.orthographicSize > MaxZoomLimit)
        {
        }
        else
        {
            StartCoroutine(_changeCameraSize(Camera.main.orthographicSize + ZoomMultiplier * Time.deltaTime));
        }
    }

    private IEnumerator _changeCameraSize(float value)
    {
        yield return new WaitForEndOfFrame();
        Camera.main.orthographicSize = value;
    }
}
