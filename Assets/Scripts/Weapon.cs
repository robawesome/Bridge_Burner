using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour {

    public float fireRate = 0;
    public int Damage = 10;
    public LayerMask toHit;
    public float effectSpawnRate = 10;

    // Handle Cam Shake
    public bool makeShakey = false;
    public float camShakeAmt = 0.05f;
    public float camShakeTime = 0.1f;
    CameraShake camShake;

    public Transform MuzzleFlashPrefab;
    public Transform BulletTrailPrefab;
    public Transform HitPrefab;

    public string weaponShootSFX = "DefaultPew";
    
    float timeToSpawnEffect = 0;
    float timeToFire = 0;
    Transform firePoint;

    //Caching
    AudioManager audioManager;

	void Awake () {
        firePoint = transform.Find("FirePoint");
        if (firePoint == null)
        {
            Debug.LogError("NO FIREPOINT!!");
        }
	}

    void Start()
    {
        camShake = MasterControlProgram.mcp.GetComponent<CameraShake>();
        if (camShake == null)
            Debug.LogError("NO CAMERA SHAKE SCRIPT YO! MCP NEEDS THAT DUMMY!");

        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("NO AUDIOMANAGER IN SCENE YO");
        }
    }

    // Update is called once per frame
    void Update () {
		if (fireRate == 0)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot();
            }
        }
        else
        {
            if (Input.GetButton ("Fire1") && Time.time > timeToFire)
            {
                timeToFire = Time.time + 1 / fireRate;
                Shoot();
            }
        }
	}

    void Shoot()
    {
        Vector2 mousePos = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        Vector2 firePointPos = new Vector2(firePoint.position.x, firePoint.position.y);
        RaycastHit2D hit = Physics2D.Raycast(firePointPos, mousePos - firePointPos, 100, toHit);

        Debug.DrawLine(firePointPos, (mousePos-firePointPos)*100, Color.cyan);
        if (hit.collider != null)
        {
            Debug.DrawLine(firePointPos, hit.point, Color.red);
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.DamageEnemy(Damage);
                // Debug.Log("We hit " + hit.collider.name + " and did " + Damage + " damage.");
            }
        }

        if (Time.time >= timeToSpawnEffect)
        {
            Vector3 hitPos;
            Vector3 hitNormal;

            if (hit.collider == null)
            {
                hitPos = (mousePos - firePointPos) * 30;
                hitNormal = new Vector3(9999, 9999, 9999);
            }
            else
            {
                hitPos = hit.point;
                hitNormal = hit.normal;
            }

            Effect(hitPos, hitNormal);
            timeToSpawnEffect = Time.time + 1 / effectSpawnRate;
        }
    }

    void Effect(Vector3 hitPos, Vector3 hitNormal)
    {   
        Transform trail = Instantiate(BulletTrailPrefab, firePoint.position, firePoint.rotation) as Transform;
        LineRenderer lr = trail.GetComponent<LineRenderer>();

        if (lr != null)
        {
            lr.SetPosition(0, firePoint.position);
            lr.SetPosition(1, hitPos);
        }

        Destroy(trail.gameObject, 0.02f);

        if (hitNormal != new Vector3(9999, 9999, 9999))
        {
            Transform hitParticle = Instantiate(HitPrefab, hitPos, Quaternion.FromToRotation(Vector3.right, hitNormal)) as Transform;
            Destroy(hitParticle.gameObject, 1f);
        }

        Transform clone = Instantiate(MuzzleFlashPrefab, firePoint.position, firePoint.rotation) as Transform;
        clone.parent = firePoint;
        float size = Random.Range(0.6f, 0.9f);
        clone.localScale = new Vector3(size, size, size);
        Destroy(clone.gameObject, 0.02f);

        //Camera Shake
        if (makeShakey == true)
        camShake.Shake(camShakeAmt, camShakeTime);

        //play shoot audio
        audioManager.PlaySound(weaponShootSFX);
    }
}
