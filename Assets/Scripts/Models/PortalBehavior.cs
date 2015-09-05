using UnityEngine;
using System.Collections;

public class PortalBehavior : MonoBehaviour 
{
    public GameObject ring;

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag=="Ship")
        {
            Destroy(collision.gameObject);
            GetComponent<Animator>().SetTrigger("ClosePortalAnimation");
            ring.GetComponent<Animator>().SetTrigger("CloseTrigger");

            //TODO MusicPlayer.Play ? ? ? 

            Destroy(transform.parent.gameObject, 5.0f);
            Invoke("enteredPortal", 1.0f);
        }
    }

    public void enteredPortal()
    {
        EventManager.PostEvent(EventManager.ePortalEnteredEvent);
        Destroy(ring);
        Destroy(gameObject);
    }
}
