using UnityEngine.Networking;
using UnityEngine;
using System.Collections;

public class PlayerShoot : NetworkBehaviour {

    private const string PLAYER_TAG = "Player";

    public ParticleSystem muzzleFlash;
    public PlayerWeapon weapon;
    public GameObject impactEffect;
    public float impactForce = 30f;
    public float fireRate = 8f;

    public int maxAmmo = 25;
    public int currentAmmo;
    public float reloadTime = 2f;
    private bool isReloading = false;


    private float nextTimeToFire = 0f;

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private LayerMask mask;

    private void Start()
    {
        if (cam == null)
        {
            currentAmmo = maxAmmo;
            Debug.LogError("Player Shoot: No Camera referenced");
            this.enabled = false;
        }
    }

     void Update()
    {
        if (isReloading)
            return;

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }

    }

    IEnumerator Reload()
    {
        isReloading = true;
    
        Debug.Log("Reloading");
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
    }

 //Is Called on the server when a player shoots
    [Command]
    void CmdOnShoot ()
    {
        RpcDoShootEffect();
    }

    //Is called on all clients when we need to do a shoot effect
    [ClientRpc]
    void RpcDoShootEffect()
    {
        muzzleFlash.Play();
    }

    //Is Called on the server when something is hit
    //Takes in the hit point and the normal of the surface
    [Command]
    void CmdOnHit (Vector3 pos, Vector3 normal)
    {
        RpcDoHitEffect(pos, normal);
    }

    //Is called on all clients
    [ClientRpc]
    void RpcDoHitEffect(Vector3 pos, Vector3 normal)
    {

      GameObject impactGo = Instantiate(impactEffect, pos, Quaternion.LookRotation(normal));
        Destroy(impactGo, 2f);
    }


    [Client]
    void Shoot ()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        //We are Shooting, call the OnShoot method on the server
        CmdOnShoot();



        muzzleFlash.Play();
        currentAmmo--;
        RaycastHit hit;
        
       if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, weapon.range, mask))
        {
            if (hit.collider.tag == PLAYER_TAG)
            {
                CmdPlayerShot(hit.collider.name, weapon.damage);
            }

            //Hit an object, call the OnHit method on the server
            CmdOnHit(hit.point, hit.normal);

            if  (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(hit.normal * impactForce);
            }

           GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impactGO, 2f);
        }
    }

    [Command]
    void CmdPlayerShot (string playerID, int damage)
    {
        Debug.Log(playerID + "has been shot");

       PlayerManager player =  GameManager.GetPlayer(playerID);
        player.RpcTakeDamage(damage);
    }
}
