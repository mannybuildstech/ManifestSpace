using UnityEngine;
using System.Collections;

public class AddScore : MonoBehaviour {

	public int deathIncrement;
	public int deathMultiplier;

	public int planetIncrement;
	public int planetMultiplier;

	public int populationIncrement;
	public int populationMultiplier;

	public int asteroidIncrement;
	public int asteroidMultiplier;

	public int scoreIncrement;
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp("q"))
		{
			ScoreText.deathCountValue += deathIncrement;
			ScoreText.scoreCountValue += (deathIncrement * deathMultiplier);
		}
		if (Input.GetKeyUp("a"))
		{
			ScoreText.deathCountValue -= deathIncrement;
			ScoreText.scoreCountValue += (deathIncrement * deathMultiplier);
		}
		if (Input.GetKeyUp("w"))
		{
			ScoreText.planetCountValue += planetIncrement;
			ScoreText.scoreCountValue += (planetIncrement * planetMultiplier);
		}
		if (Input.GetKeyUp("s"))
		{
			ScoreText.planetCountValue -= planetIncrement;
			ScoreText.scoreCountValue += (planetIncrement * planetMultiplier);
		}
		if (Input.GetKeyUp("e"))
		{
			ScoreText.populationCountValue += populationIncrement;
			ScoreText.scoreCountValue += (populationIncrement * populationMultiplier);
		}
		if (Input.GetKeyUp("d"))
		{
			ScoreText.populationCountValue -= populationIncrement;
			ScoreText.scoreCountValue += (populationIncrement * populationMultiplier);
		}
		if (Input.GetKeyUp("r"))
		{
			ScoreText.asteroidCountValue += asteroidIncrement;
			ScoreText.scoreCountValue += (asteroidIncrement * asteroidMultiplier);
		}
		if (Input.GetKeyUp("f"))
		{
			ScoreText.asteroidCountValue -= asteroidIncrement;
			ScoreText.scoreCountValue += (asteroidIncrement * asteroidMultiplier);
		}
	}
}
