using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	public int NumPassengers;
	public float OxygenDurationSeconds = 7;
	public bool ProjectileDead = false;

    // Update is called once per frame
	void Update () 
	{
		Vector3 dir = this.GetComponent<Rigidbody2D>().velocity;

        //gets angle of projectile launch based on velocity given by the spacestation when we were instantiated
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        //why do we need to -90 deg?
		this.transform.rotation = Quaternion.AngleAxis(angle-90.0f, Vector3.forward);

		OxygenDurationSeconds -= Time.deltaTime;

		if(OxygenDurationSeconds < 0)
			ProjectileDead = true;
	
		if(ProjectileDead)
		{
			MusicPlayer.SharedInstance.humansDiedSound();
            GameManager.SharedInstance.HumanCount -= NumPassengers;
            Destroy(this.gameObject);
		}
	}

	void OnCollisionEnter2D(Collision2D col)
	{
        if(NumPassengers>0)
        {
            spaceShipCollisionHandler(col);
        }
        else
        {
            missileCollisionHandler(col);
        }
	}

    void spaceShipCollisionHandler(Collision2D col)
    {
        if (col.transform.tag == "Earth" || col.transform.tag == "Planet")
        {
            Planet collidedPlanet = col.gameObject.GetComponent<Planet>();
            bool wasVisitedAndLost = collidedPlanet.bool_PlanetVisited && collidedPlanet.HumanCount == 0;
            bool firstTimeVisited = collidedPlanet.bool_PlanetVisited;

            GameManager.SharedInstance.PlanetCount += 1;
            collidedPlanet.HumanCount += NumPassengers;
            NumPassengers = 0;

            if (wasVisitedAndLost && firstTimeVisited)
            {
                //TODO analytics awards more humans.. could query game manager for the number of humans to be awarded based on:
                //  distance travelled by spaceship
                //  how much smaller the second planet is from the first one
                //  difference in rotation rate between start & end planet
                col.gameObject.GetComponent<Planet>().bool_PlanetVisited = true;
                col.gameObject.GetComponent<Planet>().HumanCount += 5;
                GameManager.SharedInstance.HumanCount += 5;
            }

            MusicPlayer.SharedInstance.humansColonizedSound();

            Destroy(this.gameObject);
        }
        else if (col.transform.tag == "Debris" || col.transform.tag == "Asteroid")
        {
            GameManager.SharedInstance.HumanCount -= NumPassengers;
            MusicPlayer.SharedInstance.humansDiedSound();
            NumPassengers = 0;
            Destroy(this.gameObject);
        }
    }

    void missileCollisionHandler(Collision2D col)
    {
        if (col.transform.tag == "Asteroid")
        {
            MusicPlayer.SharedInstance.missileBlowSound();
            Destroy(col.gameObject);
            Destroy(this.gameObject);
        }

        else if (col.transform.tag == "Debris")
        {
            MusicPlayer.SharedInstance.missileHitSpaceJunkSound();
            Destroy(col.gameObject);
            Destroy(this.gameObject);
        }

        else if (col.transform.tag == "Earth" || col.transform.tag == "Planet")
        {
            MusicPlayer.SharedInstance.missileHitPlanetSound();
            Destroy(this.gameObject);
        }
    }
}
