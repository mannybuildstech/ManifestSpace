using UnityEngine;
using System.Collections;

public class ExplosionDestroyer : MonoBehaviour {

	// Use this for initialization
	void Start () {
        MusicPlayer.SharedInstance.Explosion();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(collision.gameObject);
    }
}
