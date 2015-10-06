using UnityEngine;
using System.Collections;

public class SightLineBehavior : MonoBehaviour 
{
    float flameStartTime;
    bool flameMode = false;

    LineRenderer line;

    public GameObject PrefabExplosion;

	// Use this for initialization
	void Start () {
        line = GetComponent<LineRenderer>(); 
	}
	
	// Update is called once per frame
	void Update () 
    {
	    if(flameMode)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.right, out hit, Mathf.Infinity))
            {
                if(hit.collider.gameObject.tag=="Debris")
                {
                    DebriBehavior debri = hit.collider.gameObject.GetComponent<DebriBehavior>();
                    MusicPlayer.SharedInstance.missileBlowSound();
                    Destroy(Instantiate(PrefabExplosion, debri.transform.position, Quaternion.identity), 1.0f);
                }
            }

            if(Time.time-flameStartTime>5.0f)
            {
                flameMode = false;
                line.SetColors(Color.yellow, Color.yellow);
                line.SetWidth(.3f, .025f);
            }
        }
	}

    public void LaunchFlameThrower()
    {
        line.SetColors(Color.red, Color.red);
        line.SetWidth(.6f, .05f);
        flameStartTime = Time.time;
    }

}
