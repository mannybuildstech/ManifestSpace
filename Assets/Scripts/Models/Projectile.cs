using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour 
{
    public GameObject PrefabExplosion;

    public enum ProjectileType {missile, spaceship, portalspaceship};
    public ProjectileType currentProjectileType;

	public int NumPassengers;
	public float OxygenDurationSeconds;

    public float LandingSpeed = .5f;

    private float shipDestroyForce = 75.0f;
    private float shipDestroySpin  = 75.0f;

    public Sprite missileSprite;
    public Sprite shipSprite;
    
    public void Start()
    {
        if(currentProjectileType==ProjectileType.missile)
        {
            GetComponent<SpriteRenderer>().sprite = missileSprite;
            GetComponent<Animator>().enabled = true;
        }
        else
        {
            GetComponent<SpriteRenderer>().sprite = shipSprite;
        }
    }

    // Update is called once per frame
	void Update () 
	{
        if (currentProjectileType == ProjectileType.missile || currentProjectileType == ProjectileType.spaceship)
        {
            Vector3 dir = this.GetComponent<Rigidbody2D>().velocity;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg; //gets angle of projectile launch based on velocity given by the spacestation when we were instantiated
            this.transform.rotation = Quaternion.AngleAxis(angle - 90.0f, Vector3.forward); //why do we need to -90 deg?

            OxygenDurationSeconds -= Time.deltaTime;
            if (OxygenDurationSeconds < 0)
            {
                MusicPlayer.SharedInstance.humansDiedSound();
                GameManager.SharedInstance.CurrentSolarSystemSeed.HumanPopulation -= NumPassengers;
                Destroy(this.gameObject);
            }
        }
        else
        {
            Vector2 diff = (Vector2)gameObject.transform.position - GameManager.SharedInstance.CurrentHomePosition;
            diff.Normalize();
            float angleOfVector = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            gameObject.transform.rotation = Quaternion.Euler(0f, 0f, angleOfVector + 90);
            transform.position = Vector2.MoveTowards(transform.position, GameManager.SharedInstance.CurrentHomePosition, Time.time * .015f);
        }
	}

	void OnCollisionEnter2D(Collision2D col)
	{
        switch(currentProjectileType)
        {
            case ProjectileType.missile:
                missileCollisionHandler(col);  
                break;
            case ProjectileType.spaceship:
                spaceShipCollisionHandler(col);
                break;
            case ProjectileType.portalspaceship:
                portalShipCollisionHandler(col);
                break;
            default:
                break;
        }
	}

    void spaceShipCollisionHandler(Collision2D col)
    {
        if (col.transform.tag == "Earth" || col.transform.tag == "Planet")
        {
            Planet collidedPlanet = col.gameObject.GetComponent<Planet>();
            
            bool wasVisitedAndLost = collidedPlanet.bool_PlanetVisited && collidedPlanet.HumanCount == 0;
            bool firstTimeVisited = collidedPlanet.bool_PlanetVisited;

            if(collidedPlanet.CurrentPlanetState==Planet.PlanetStateEnum.virgin)
                GameManager.SharedInstance.CurrentSolarSystemSeed.ColonizedPlanetCount ++;
            
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
                GameManager.SharedInstance.CurrentSolarSystemSeed.HumanPopulation += 5;
            }

            MusicPlayer.SharedInstance.playColonizedSound();

            Destroy(this.gameObject);
        }
        else if (col.transform.tag == "Debris" || col.transform.tag == "Asteroid")
        {
            GameManager.SharedInstance.CurrentSolarSystemSeed.HumanPopulation -= NumPassengers;
            MusicPlayer.SharedInstance.humansDiedSound();
            NumPassengers = 0;
            
            if(col.transform.tag=="Debris")
            {
                Rigidbody2D rigidbody2D = col.gameObject.GetComponent<Rigidbody2D>();
                Vector2 oppositeVelocity = (rigidbody2D.velocity) * -1;

                gameObject.GetComponent<Rigidbody2D>().AddForce((oppositeVelocity + gameObject.GetComponent<Rigidbody2D>().velocity) * shipDestroyForce, ForceMode2D.Force);
                gameObject.GetComponent<Rigidbody2D>().AddTorque(Random.Range(1, shipDestroySpin), ForceMode2D.Impulse);
            }

            Destroy(Instantiate(PrefabExplosion, col.contacts[0].point, Quaternion.identity), 1.0f);
            Destroy(gameObject);
        }
    }

    void portalShipCollisionHandler(Collision2D col)
    {
        if (col.gameObject.tag == "Earth" || col.gameObject.tag == "Planet")
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
            EventManager.PostEvent(EventManager.eNextHomeIsReadyEvent);
            Planet collidedPlanet = col.gameObject.GetComponent<Planet>();
            GameManager.SharedInstance.CurrentSolarSystemSeed.ColonizedPlanetCount += 1;
            collidedPlanet.HumanCount += GameManager.SharedInstance.CurrentSolarSystemSeed.StartingHumans;
            
            GameManager.SharedInstance.CurrentSolarSystemSeed.HumanPopulation += GameManager.SharedInstance.CurrentSolarSystemSeed.StartingHumans;
            MusicPlayer.SharedInstance.playColonizedSound();
            Destroy(this.gameObject,.75f);
        }
    }

    void destroyShipAnimation()
    {
        Destroy(this.gameObject);
    }

    void missileBlowAnimation()
    {
        Destroy(Instantiate(PrefabExplosion, gameObject.transform.position, Quaternion.identity), 1.0f);
        Destroy(this.gameObject);
    }

    void missileCollisionHandler(Collision2D col)
    {
        if(col.transform.tag=="Earth" || col.transform.tag == "Planet")
        {
            MusicPlayer.SharedInstance.missileBlowSound();
            Planet planet= col.gameObject.GetComponent<Planet>();
        
            if(planet.HumanCount>0)
            {
                int lostHumanz = (int)planet.HumanCount/4;
                GameManager.SharedInstance.CurrentSolarSystemSeed.HumanPopulation -= lostHumanz;
                planet.HumanCount -= lostHumanz;
            }
        }
        else
        {
            MusicPlayer.SharedInstance.missileBlowSound();
            
            Rigidbody2D rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
            //col.gameObject.GetComponent<Rigidbody2D>().AddForce((rigidbody2D.velocity + col.gameObject.GetComponent<Rigidbody2D>().velocity) * shipDestroyForce, ForceMode2D.Force);
            col.gameObject.GetComponent<Rigidbody2D>().AddTorque(Random.Range(1, shipDestroySpin), ForceMode2D.Impulse);
            Destroy(col.gameObject, .35f);
        }
        missileBlowAnimation();
    }

}
