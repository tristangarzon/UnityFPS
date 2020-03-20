using UnityEngine.Networking;
using UnityEngine;
using System.Collections;



[RequireComponent(typeof(PlayerSetup))]
public class PlayerManager : NetworkBehaviour {
    [SyncVar]
    private bool _isDead = false;
    public bool isDead
    {
        get { return _isDead; }
        protected set { _isDead = value; }
    }

    [SerializeField]
    private int maxHealth = 100;
    
    [SyncVar]
    private int currentHealth;

    public float GetHealthPct()
    {
        return (float)currentHealth / maxHealth;
    }

    [SerializeField]
    private Behaviour[] disableOnDeath;
    private bool[] wasEnabled;

    [SerializeField]
    private GameObject[] disableGameObjectsOnDeath;

    [SerializeField]
    private GameObject deathEffect;


    public void Setup()
    {
        wasEnabled = new bool[disableOnDeath.Length];
        for (int i = 0; i < wasEnabled.Length; i++)
        {
            wasEnabled[i] = disableOnDeath[i].enabled;
        }

        SetDefaults();
    }

    ///*
    private void Update()
    {
        if (!isLocalPlayer)
            return;

        if (Input.GetKeyDown(KeyCode.K))
        {
            RpcTakeDamage(99999);
        }
    }
   // */

    [ClientRpc]
    public void RpcTakeDamage (int amount)
    {
        if (isDead)
            return;

        currentHealth -= amount;

        Debug.Log(transform.name + " now has " + currentHealth + " health.");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;


       

        //Disable Components 
        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = false;
        }

        //Disable GameObjects
        for (int i = 0; i < disableGameObjectsOnDeath.Length; i++)
        {
            disableGameObjectsOnDeath[i].SetActive (false);
        }

        //Disable the Collider
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        //Spawn a Death Effect
      GameObject gfxIns = (GameObject)  Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(gfxIns, 3f);


        //Switch Cameras
        if (isLocalPlayer)
        {
            GameManager.instance.SetSceneCameraActive(true);
            GetComponent<PlayerSetup>().playerUIInstance.SetActive(false);
        }


        Debug.Log(transform.name + " is Dead");

        //CALL RESPAWN METHOD

        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn ()
    {
        yield return new WaitForSeconds(GameManager.instance.matchSettings.respawnTime);

        SetDefaults();
        Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        Debug.Log(transform.name + " respawned.");
    }


    public void SetDefaults ()
    {

        isDead = false;

        currentHealth = maxHealth;
        //Enable the Components 
        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = wasEnabled[i];
        }

        //Enable the gameobjects
        for (int i = 0; i < disableGameObjectsOnDeath.Length; i++)
        {
            disableGameObjectsOnDeath[i].SetActive(true);
        }

        //Enable the collider
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = true;


        if (isLocalPlayer)
        {
            GameManager.instance.SetSceneCameraActive(false);
            GetComponent<PlayerSetup>().playerUIInstance.SetActive(true);
        }
    }

}
