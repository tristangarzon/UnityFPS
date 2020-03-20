using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour {

    [SerializeField]
    RectTransform healthBarFill;
    [SerializeField]
    Text ammoText;

    private FPS controller;
    private PlayerManager player;
    private PlayerShoot weaponManager;



    public void SetPlayer (PlayerManager _player)
    {
        player = _player;
        controller = player.GetComponent<FPS>();
        weaponManager = player.GetComponent<PlayerShoot>();

    }

     void Update()
    {
        SetHealthAmount(player.GetHealthPct());
        SetAmmoAmount(weaponManager.currentAmmo);


    }




    void SetHealthAmount (float amount)
    {
        healthBarFill.localScale = new Vector3(1f, amount, 1f);
    }

    void SetAmmoAmount (int amount)
    {
        ammoText.text = amount.ToString();
    }

}
