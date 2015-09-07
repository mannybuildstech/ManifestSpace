using UnityEngine;
using System.Collections;

public class PortalBehavior : MonoBehaviour 
{
    public GameObject ring;

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

                MusicPlayer.SharedInstance.playPortalEnteredSFX();

                Destroy(transform.parent.gameObject, 5.0f);
                Invoke("enteredPortal", 1.0f);
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
