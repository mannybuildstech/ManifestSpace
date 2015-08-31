using UnityEngine;
using System.Collections;

public class AsteroidThreat : MonoBehaviour {

    public float populationDamageRatio = .5f;

    public Transform target;
    public float speed = 1.0f;

    public void Start()
    {

    }

    void Destroy()
    {
        EventManager.PostEvent(EventManager.eAsteroidDestroyedEvent);
        Destroy(this.gameObject);
    }

    void Update()
    {
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);
    }

	void OnCollisionEnter2D (Collision2D coll) 
	{
		if(coll.gameObject.tag == "Earth" || coll.gameObject.tag == "Planet")
		{
            Planet planetHit = coll.gameObject.GetComponent<Planet>();

            int prevHumanCount = planetHit.HumanCount;
            planetHit.HumanCount = (int)(prevHumanCount * populationDamageRatio);

            int livesLost = prevHumanCount - planetHit.HumanCount;
			GameManager.SharedInstance.HumanCount -= livesLost;

			Invoke("Destroy",2.0f);
		}

	}
	
}
