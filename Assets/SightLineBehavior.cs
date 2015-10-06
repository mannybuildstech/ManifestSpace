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
            RaycastHit2D hit = Physics2D.Raycast(transform.parent.position, transform.parent.up);
            
            if(hit.collider!=null)
            {
                if(hit.collider.gameObject.tag=="Debris")
                {
                    DebriBehavior debri = hit.collider.gameObject.GetComponent<DebriBehavior>();
                    MusicPlayer.SharedInstance.missileBlowSound();
                    Destroy(Instantiate(PrefabExplosion, debri.transform.position, Quaternion.identity), 1.0f);
                }
            }
            if ((Time.time - flameStartTime) >= 8.0f)
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
        flameMode = true;
    }

}
