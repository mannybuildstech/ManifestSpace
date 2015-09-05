using UnityEngine;
using System.Collections;

public class DebriBehavior : MonoBehaviour 
{
    public Sprite[] textures;
    public Vector3 orbitOrigin;

    float spinDirection;
    public float MinSpeed = 5;
    public float MaxSpeed = 15;
    float speed;

	// Use this for initialization
	void Start () 
    {
        //choose a sprite and apply it 
        int textureIndex = Random.Range(0, textures.Length);
        GetComponent<SpriteRenderer>().sprite = textures[textureIndex];
        spinDirection = (Random.Range(1,10)>5)?1:-1;
        speed = Random.Range(MinSpeed, MaxSpeed);
	}
	
	// Update is called once per frame
	void Update () 
    {
        transform.Rotate(Vector3.forward,Time.deltaTime*speed,Space.Self);
        transform.RotateAround(orbitOrigin,Vector3.forward,speed * Time.deltaTime*spinDirection);
	}

    public void OnCollisionEnter2D(Collision2D collision)
    {
		if(collision.gameObject.tag == "Ship")
		{
        	GameObject.Destroy(this.gameObject,.75f);
		}
        else if(collision.gameObject.tag == "Asteroid")
        {
            GetComponent<Rigidbody2D>().AddForce(collision.gameObject.GetComponent<Rigidbody2D>().velocity, ForceMode2D.Force); 
        }
    }
}
