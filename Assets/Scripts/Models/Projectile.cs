using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour 
{
	public int NumPassengers;
	public float OxygenDurationSeconds;

	public bool ProjectileDead = false;

    private float shipDestroyForce = 75.0f;
    private float shipDestroySpin  = 75.0f;

    private float missileHitForce = 300.0f;
    private float missileHitSpin  = 80.0f;

    public void Start()
    {
        Debug.Log("Oxygenduration:" + OxygenDurationSeconds);
    }

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
        {
            ProjectileDead = true;
        }
			
	
		if(ProjectileDead)
		{
			MusicPlayer.SharedInstance.humansDiedSound();
            GameManager.SharedInstance.CurrentLevel.HumanPopulation -= NumPassengers;
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

            GameManager.SharedInstance.CurrentLevel.ColonizedPlanetCount += 1;
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
                GameManager.SharedInstance.CurrentLevel.HumanPopulation += 5;
            }

            MusicPlayer.SharedInstance.humansColonizedSound();

            Destroy(this.gameObject);
        }
        else if (col.transform.tag == "Debris" || col.transform.tag == "Asteroid")
        {
            GameManager.SharedInstance.CurrentLevel.HumanPopulation -= NumPassengers;
            MusicPlayer.SharedInstance.humansDiedSound();
            NumPassengers = 0;
            
            if(col.transform.tag=="Debris")
            {
                Rigidbody2D rigidbody2D = col.gameObject.GetComponent<Rigidbody2D>();
                Vector2 oppositeVelocity = (rigidbody2D.velocity) * -1;

                gameObject.GetComponent<Rigidbody2D>().AddForce((oppositeVelocity + gameObject.GetComponent<Rigidbody2D>().velocity) * shipDestroyForce, ForceMode2D.Force);
                gameObject.GetComponent<Rigidbody2D>().AddTorque(Random.Range(1, shipDestroySpin), ForceMode2D.Impulse);
            }
            Invoke("destroyShipAnimation",.25f);
        }
    }

    void destroyShipAnimation()
    {
        Destroy(this.gameObject);
    }

    void missileBlowAnimation()
    {
        Destroy(this.gameObject);
    }

    void missileCollisionHandler(Collision2D col)
    {
        if(col.transform.tag=="Earth" || col.transform.tag == "Planet")
        {
            MusicPlayer.SharedInstance.missileHitPlanetSound();
            Planet planet= col.gameObject.GetComponent<Planet>();
        
            if(planet.HumanCount>0)
            {
                int lostHumanz = (int)planet.HumanCount/4;
                GameManager.SharedInstance.CurrentLevel.HumanPopulation -= lostHumanz;
                planet.HumanCount = lostHumanz;
            }
        }
        else
        {
            if (col.transform.tag == "Asteroid")
            {
                MusicPlayer.SharedInstance.missileBlowSound();
            }
            else if(col.transform.tag == "Debris")
            {
                MusicPlayer.SharedInstance.missileHitSpaceJunkSound();
            }
            Rigidbody2D rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
            col.gameObject.GetComponent<Rigidbody2D>().AddForce((rigidbody2D.velocity + col.gameObject.GetComponent<Rigidbody2D>().velocity) * shipDestroyForce, ForceMode2D.Force);
            col.gameObject.GetComponent<Rigidbody2D>().AddTorque(Random.Range(1, shipDestroySpin), ForceMode2D.Impulse);
            Destroy(col.gameObject, .75f);
        }
        missileBlowAnimation();
    }

}
