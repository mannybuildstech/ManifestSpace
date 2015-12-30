using UnityEngine;
using System.Collections;

public class SessionEndedPanelAnim : MonoBehaviour {

    public Animator anim;

    public void ShowPanel()
    {
        anim.SetBool("ShowPanel", true);
    }

    public void HidePanel()
    {
        anim.SetBool("ShowPanel", false);
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
