using UnityEngine;
using System.Collections;

public class ObjectManager : MonoBehaviour {

	private	 Transform    transform;
	private  Transform    relativeTransform;
	private	 int		  oneInA;
	public   string	      condition;
	public	 GameObject	  collisionWithObject;
	public	 string	      action;
	public	 int		  odds;
	public   GameObject   thisObject;
	public	 float	      xRangeMin;
	public	 float	      xRangeMax;
	public	 float	      yRangeMin;
	public	 float	      yRangeMax;
	public	 float	      zRangeMin;
	public	 float	      zRangeMax;
    public   GameObject   relativeObject;
	public	 bool	      relative;
    public   float        force;
    public   float        explosionRadius;
    public   bool         selfDestruct;
    public   float        timer;
    private  bool         startTimer;
    private  string       previousCondition;
    private  float        waitTime;



	// Use this for initialization
	void Start ()
    {
        timer = 0F;
        startTimer = false;
        previousCondition = "";
        if (condition.Equals("Start"))
        {
            previousCondition = "Start";
            condition = "Always";
        }
	}

	void OnTriggerEnter ()
    {
        if (condition.Equals("On Trigger Enter"))
        {
            previousCondition = "On Trigger Enter";
            condition = "Always";
        }
	}

	void OnCollisionEnter (Collision col)
    {
        if (condition.Equals("On Collision Enter"))
        {
            previousCondition = "On Collision Enter";
            condition = "Always";
        }
	}

    void OnMouseDown()
    {
        if (condition.Equals("On Mouse Press"))
        {
            previousCondition = "On Mouse Press";
            condition = "Always";
        }
    }

	void Update ()
    {
        if (startTimer == true) { timer += 0.017F; }
        if (condition.Equals("Always"))
        {
            startTimer = true;
            if (timer >= waitTime)
            {  
                oneInA = 1;
                if (oneInA == Random.Range(1, odds))
                {
                    if (action.Equals("Create") || action.Equals("Change"))
                    {
                        if (relative == true)
                        {
                            transform = relativeObject.GetComponent<Transform>();
                            Instantiate(thisObject, new Vector3(transform.position.x + Random.Range(xRangeMin, xRangeMax), transform.position.y + Random.Range(yRangeMin, yRangeMax), transform.position.z + Random.Range(zRangeMin, zRangeMax)), thisObject.transform.rotation);
                        }
                        else
                        {
                            transform = thisObject.GetComponent<Transform>();
                            Instantiate(thisObject, new Vector3(Random.Range(xRangeMin, xRangeMax), Random.Range(yRangeMin, yRangeMax), Random.Range(zRangeMin, zRangeMax)), thisObject.transform.rotation);
                        }
                    }
                    if (action.Equals("Change") && GameObject.Find(relativeObject.name) != null)
                    {
                        DestroyObject(relativeObject);
                    }
                    if (action.Equals("Destroy") && GameObject.Find(thisObject.name) != null)
                    {
                        DestroyObject(thisObject);
                    }
                    if (action.Equals("Teleport"))
                    {
						transform = thisObject.GetComponent<Transform>();
                        if (relative == true)
                        {
							relativeTransform = relativeObject.GetComponent<Transform>();
							transform.position = new Vector3(relativeTransform.position.x + Random.Range(xRangeMin, xRangeMax), relativeTransform.position.y + Random.Range(yRangeMin, yRangeMax), relativeTransform.position.z + Random.Range(zRangeMin, zRangeMax));
                        }
                        else
                        {
                            transform.position = new Vector3(Random.Range(xRangeMin, xRangeMax), Random.Range(yRangeMin, yRangeMax), Random.Range(zRangeMin, zRangeMax));
                        }
                    }
					if (action.Equals("2D Launch"))
					{
						this.gameObject.GetComponent<Rigidbody2D>().AddForce(transform.up * force);
					}
                    if (previousCondition.Equals("Start")) {condition = "Start";}
                    if (previousCondition.Equals("On Trigger Enter")) {condition = "On Trigger Enter";}
                    if (previousCondition.Equals("On Collision Enter")) {condition = "On Collision Enter";}
                    if (previousCondition.Equals("On Mouse Press")) {condition = "On Mouse Press";}
                    if (previousCondition.Equals("")) { timer = 0F; }
                    else
                    {
                        startTimer = false;
                        previousCondition = "Always";
                    }
                }
            }
        }
	}
}