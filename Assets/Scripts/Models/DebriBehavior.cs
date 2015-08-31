using UnityEngine;
using System.Collections;

public class DebriBehavior : MonoBehaviour 
{
    public Sprite[] textures;
    public Vector3 orbitOrigin;

    Collider2D collision;

    float spinDirection;
    float minSpeed = 5;
    float maxSpeed = 20;

	int DebrisSpeed;

    float speed;

	// Use this for initialization
	void Start () 
    {
        collision = GetComponent<Collider2D>();
        //choose a sprite and apply it 
        int textureIndex = Random.Range(0, textures.Length);
        GetComponent<SpriteRenderer>().sprite = textures[textureIndex];

        spinDirection = (Random.Range(1,10)>5)?1:-1;
        //spinDirection = -1;
        speed = Random.Range(minSpeed, maxSpeed);
        //Debug.Log("spin direction:" + spinDirection);

		DebrisSpeed = Random.Range(1,4);
	}
	
	// Update is called once per frame
	void Update () 
    {
        if(orbitOrigin!=null)
        {
            transform.Rotate(Vector3.forward,Time.deltaTime*speed*DebrisSpeed,Space.Self);
            transform.RotateAround(orbitOrigin,Vector3.forward,speed * Time.deltaTime*spinDirection);
        }
            
	}

    public void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("collided with object");
		if(collision.gameObject.tag == "Ship")
		{
        	GameObject.Destroy(this.gameObject);
		}
    }
}
