using UnityEngine;
using System.Collections;

public class PortalBehavior : MonoBehaviour 
{
    public GameObject ring;

    public void Start()
    {
        MusicPlayer.SharedInstance.portalBackgroundHum(true);
    }

    public void OnDestroy()
    {
        MusicPlayer.SharedInstance.portalBackgroundHum(true);
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag=="Ship")
        {
            Projectile projectile = collision.gameObject.GetComponent<Projectile>();
            if (projectile.currentProjectileType == Projectile.ProjectileType.spaceship)
            {
                Destroy(collision.gameObject);
                GetComponent<Animator>().SetTrigger("ClosePortalAnimation");
                ring.GetComponent<Animator>().SetTrigger("CloseTrigger");

                Destroy(transform.parent.gameObject, 5.0f);
                Invoke("enteredPortal", 1.0f);
                MusicPlayer.SharedInstance.portalBackgroundHum(true);
            }
        }
    }

    public void enteredPortal()
    {
        EventManager.PostEvent(EventManager.ePortalEnteredEvent);
        Destroy(ring);
        Destroy(gameObject);
    }
}
