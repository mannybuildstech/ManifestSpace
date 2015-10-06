using UnityEngine;
using System.Collections;

public class PortalBehavior : MonoBehaviour 
{
    public GameObject ring;

    public void Start()
    {
        MusicPlayer.SharedInstance.portalBackgroundHum(true);

        DebriSpawner debri = GetComponent<DebriSpawner>();
        int minDebri=0;
        int maxDebri=0;
        int index = GameManager.SharedInstance.levelIndex;
        
        if(index>5 && index<10)
        {
            minDebri = index-4;
            maxDebri = index-2;
        }
        else if(index>=10 && index<=20)
        {
            minDebri = 6;
            maxDebri = 10;
        }
        else if (index>=20)
        {
            minDebri = 5;
            maxDebri = 10;
        }
        else
        {
            minDebri = 0;
            maxDebri = 0;
        }

        debri.minDebriCount = minDebri;
        debri.maxDebriCount = maxDebri;
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
