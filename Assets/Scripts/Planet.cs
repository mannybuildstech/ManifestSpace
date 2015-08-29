using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class Planet : MonoBehaviour {

	private float float_RotationSpeed;
	private int int_RotationDirection;

	public int int_NumberOfHumans;
	private int int_StartingHumans = 50;

	public enum PlanetState{empty, inProgress, colonized, destroyed};
	public PlanetState planetState;

	public bool bool_PlanetVisited = false;

	public Text PopulationDisplayText;

	// Use this for initialization
	void Start () {

		if(this.gameObject.tag == "Earth")
		{
			planetState = PlanetState.colonized;
			int_NumberOfHumans = int_StartingHumans;
			GameManager.SharedInstance.HumanCount += int_StartingHumans;
		}
		else
		{
			planetState = PlanetState.empty;
			int_NumberOfHumans = 0;
		}

		float_RotationSpeed = Random.Range(1f, 2.75f);
		int_RotationDirection = Random.Range(0,2);

		if(int_RotationDirection == 0)
		{
			int_RotationDirection = -1;
		}
		else
		{
			int_RotationDirection = 1;
		}

		float_RotationSpeed *= int_RotationDirection;

	
	}
	
	// Update is called once per frame
	void Update () {

		this.gameObject.transform.Rotate(0,0,float_RotationSpeed) ;

		//display current number of humans
		this.GetComponentInChildren<TextMesh>().text = int_NumberOfHumans.ToString();
	}

	void OnMouseDown()
	{
		if(GameManager.SharedInstance.currentLaunchMode == GameManager.LaunchMode.HumanMode)
		{

			if(planetState == PlanetState.colonized)
			{
				if(int_NumberOfHumans > 0)
				{
					if(int_NumberOfHumans == 5)
					{
						Destroy(this.gameObject.transform.GetChild(1).gameObject);
						GameManager.SharedInstance.PlanetCount -= 1;
						planetState = PlanetState.empty;
					}
					this.gameObject.GetComponentInChildren<SpaceStation>().Launch();

				}
			}
		}
		else if(GameManager.SharedInstance.currentLaunchMode == GameManager.LaunchMode.MissileMode)
		{
			this.gameObject.GetComponentInChildren<SpaceStation>().Launch();
		}
	}
}
