using UnityEngine;
using System.Collections;

public class AsteroidThreat : MonoBehaviour 
{
    public Vector3 target;
    public float speed = 3.0f;
    public float spinSpeed = 8.0f;
    float maxDestroyForce = 400.0f;

    public int asteroidIndex;

    public void Start()
    {
        asteroidIndex = GameManager.SharedInstance.AsteroidThreatList.Add(gameObject);
    }

    void Destroy()
    {
        GameManager.SharedInstance.AsteroidThreatList.Remove(gameObject);
        EventManager.PostEvent(EventManager.eAsteroidDestroyedEvent);
        Destroy(this.gameObject);
    }

    void Update()
    {
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target, step);
        transform.Rotate(Vector3.forward, Time.deltaTime * spinSpeed, Space.Self);
    }

	void OnCollisionEnter2D (Collision2D coll) 
	{
        Rigidbody2D myRigid = gameObject.GetComponent<Rigidbody2D>();
		if(coll.gameObject.tag == "Earth" || coll.gameObject.tag == "Planet")
		{
            Planet planetHit = coll.gameObject.GetComponent<Planet>();

            if (planetHit.HumanCount > 0)
            {
                int killedHumans = (int)planetHit.HumanCount/2;
                if ((planetHit.HumanCount - killedHumans) < 5)
                    killedHumans = planetHit.HumanCount;

                GameManager.SharedInstance.CurrentLevel.HumanPopulation -= killedHumans;
                planetHit.HumanCount -=killedHumans ;
            }

            myRigid.AddForce(myRigid.velocity*maxDestroyForce,ForceMode2D.Force);
            gameObject.GetComponent<Rigidbody2D>().AddTorque(Random.Range(1, maxDestroyForce/4),ForceMode2D.Force);

			Invoke("Destroy",.25f);
		}
        else if(coll.gameObject.tag == "Debris")
        {
            //Rigidbody2D debriBody = coll.gameObject.GetComponent<Rigidbody2D>();
            //debriBody.GetComponent<Rigidbody2D>().AddTorque(Random.Range(1, maxDestroyForce), ForceMode2D.Impulse);
        }
	}
	
}
