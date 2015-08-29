using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreText : MonoBehaviour {

    public string deathCountTitle;
	public int deathCountMinimum, deathCountMaximum;
	public static int deathCountValue;

    public string planetCountTitle;
    public int planetCountMinimum, planetCountMaximum;
	public static int planetCountValue;

	public string populationCountTitle;
	public int populationCountMinimum, populationCountMaximum;
	public static int populationCountValue;
	
	public string asteroidCountTitle;
	public int asteroidCountMinimum, asteroidCountMaximum;
	public static int asteroidCountValue;

	public string scoreCountTitle;
	public int scoreCountMinimum, scoreCountMaximum;
	public static int scoreCountValue;

    private Text scoreText;

	// Use this for initialization
	void Start () {
		deathCountValue = deathCountMinimum;
        planetCountValue = planetCountMinimum;
        scoreText = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        if (deathCountValue < deathCountMinimum) { deathCountValue = deathCountMinimum; }
        if (deathCountValue > deathCountMaximum) { deathCountValue = deathCountMaximum; }

        if (planetCountValue < planetCountMinimum) { planetCountValue = planetCountMinimum; }
        if (planetCountValue > planetCountMaximum) { planetCountValue = planetCountMaximum; }

		if (populationCountValue < populationCountMinimum) { populationCountValue = populationCountMinimum; }
		if (populationCountValue > populationCountMaximum) { populationCountValue = planetCountMaximum; }

		if (asteroidCountValue < asteroidCountMinimum) { asteroidCountValue = asteroidCountMinimum; }
		if (asteroidCountValue > asteroidCountMaximum) { asteroidCountValue = planetCountMaximum; }

		if (scoreCountValue < scoreCountMinimum) { scoreCountValue = scoreCountMinimum; }
		if (scoreCountValue > scoreCountMaximum) { scoreCountValue = planetCountMaximum; }

		scoreText.text = deathCountTitle + deathCountValue + "\n" + planetCountTitle + planetCountValue + "\n"
			+ populationCountTitle + populationCountValue + "\n" + asteroidCountTitle + asteroidCountValue + "\n"  + "\n"
			+ scoreCountTitle + scoreCountValue;

	}
}
