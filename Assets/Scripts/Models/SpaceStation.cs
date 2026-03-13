using UnityEngine;
using System.Collections;

public class SpaceStation : MonoBehaviour 
{
	public GameObject ProjectilePrefab;
    public Sprite LaunchPointShipSprite;
    public float LaunchPointOffset = 2.0f;
    public int LaunchPointSortingOrderOffset = 1;
    
    public GameObject SightLine;
    
    private GameObject ProjectileInstance;
    private GameObject LaunchPointShipInstance;

	public int MaxNumPassengers;
	private Vector3 vec3_Rotation;

    public float HumanLaunchForce   = 1000.0f;
    public float MissileLaunchForce = 1000.0f;

	void Start () 
	{
		vec3_Rotation = new Vector3(0,0,0);
		this.transform.localRotation = Quaternion.Euler(vec3_Rotation);
        EnsureLaunchPointShip();
	}
	
	void Update () 
	{

	}

    public void ConfigureSightLine(bool visible)
    {
        SightLine.SetActive(visible);
    }

    public void LaunchHumans()
    {
        ProjectilePrefab.GetComponent<Projectile>().currentProjectileType = Projectile.ProjectileType.spaceship;
        ProjectilePrefab.tag = "Ship";

        ProjectileInstance = Instantiate(ProjectilePrefab, this.transform.position + (this.transform.up * LaunchPointOffset), Quaternion.identity) as GameObject;
        ProjectileInstance.GetComponent<Rigidbody2D>().AddForce(this.transform.up * HumanLaunchForce);

        if (this.gameObject.GetComponentInParent<Planet>().HumanCount < MaxNumPassengers)
        {
            ProjectileInstance.GetComponent<Projectile>().NumPassengers = this.gameObject.GetComponentInParent<Planet>().HumanCount;
            this.gameObject.GetComponentInParent<Planet>().HumanCount = 0;
        }
        else
        {
            this.gameObject.GetComponentInParent<Planet>().HumanCount -= MaxNumPassengers;
            ProjectileInstance.GetComponent<Projectile>().NumPassengers = MaxNumPassengers;
        }
            
        MusicPlayer.SharedInstance.humanLaunchSound();
    }

    public void launchMissiles(bool powerUpEnabled)
    {
        ProjectilePrefab.GetComponent<Projectile>().currentProjectileType = Projectile.ProjectileType.missile;

        if(!powerUpEnabled)
        {
            ProjectileInstance = Instantiate(ProjectilePrefab, this.transform.position + (this.transform.up * LaunchPointOffset), Quaternion.identity) as GameObject;
            ProjectileInstance.GetComponent<Projectile>().NumPassengers = 0;
            ProjectileInstance.GetComponent<Rigidbody2D>().AddForce(this.transform.up * MissileLaunchForce);
            MusicPlayer.SharedInstance.missileLaunchSound();
        }
        else
        {
            ConfigureSightLine(true);
            GetComponentInChildren<SightLineBehavior>().LaunchFlameThrower();
        }
	}

    private void EnsureLaunchPointShip()
    {
        if (LaunchPointShipInstance != null)
            return;

        Sprite launchSprite = LaunchPointShipSprite;
        if (launchSprite == null && ProjectilePrefab != null)
        {
            SpriteRenderer projectileSprite = ProjectilePrefab.GetComponent<SpriteRenderer>();
            if (projectileSprite != null)
            {
                launchSprite = projectileSprite.sprite;
            }
        }

        if (launchSprite == null)
            return;

        LaunchPointShipInstance = new GameObject("LaunchPointShip");
        LaunchPointShipInstance.transform.SetParent(this.transform, false);
        LaunchPointShipInstance.transform.localPosition = Vector3.up * LaunchPointOffset;
        LaunchPointShipInstance.transform.localRotation = Quaternion.identity;

        SpriteRenderer launchRenderer = LaunchPointShipInstance.AddComponent<SpriteRenderer>();
        launchRenderer.sprite = launchSprite;

        SpriteRenderer stationRenderer = GetComponent<SpriteRenderer>();
        if (stationRenderer != null)
        {
            launchRenderer.sortingLayerID = stationRenderer.sortingLayerID;
            launchRenderer.sortingOrder = stationRenderer.sortingOrder + LaunchPointSortingOrderOffset;
        }
    }
}
