using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public float health = 100f;
    public float healthCap;
    public TextMeshProUGUI healthText;

    public GameManager gameManager;
    public GameObject playerCamera;

    public CanvasGroup hitPanel;

    private float shakeTime = 1;
    private float shakeDuration = 0.5f;
    private Quaternion playerCameraOriginalRotation;

    public GameObject weaponHolder;
    private int activeWeaponIndex;
    private GameObject activeWeapon;

    public int totalPoints;
    public TextMeshProUGUI pointsText;
    //Para que unicamente yo tenga el control de mis scripts ose si disparo solo yo y no tener el control y que disparen todos a la vez
    public PhotonView photonView;

    private void Start()
    {
        playerCameraOriginalRotation = playerCamera.transform.localRotation;

        WeaponSwitch(0);
        totalPoints = 0;
        UpdatePoints(0);

        healthCap = health;
    }

    private void Update()
    {
        //Verificar si esta en la scene y si el phtonview es mio
        if (PhotonNetwork.InRoom && !photonView.IsMine)
        {
            playerCamera.gameObject.SetActive(false);
            return;
        }

        if (hitPanel.alpha > 0)
        {
            hitPanel.alpha -= Time.deltaTime;
        }

        if (shakeTime < shakeDuration)
        {
            shakeTime += Time.deltaTime;
            CameraShake();
        }

        else if (playerCamera.transform.localRotation != playerCameraOriginalRotation)
        {
            playerCamera.transform.localRotation = playerCameraOriginalRotation;
            Debug.Log("Se a centrado la camara");
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0 || Input.GetKeyDown(KeyCode.Q))
        {
            WeaponSwitch(activeWeaponIndex + 1);
        }

    }

    public void Hit(float damage)
    {
        if (!PhotonNetwork.InRoom)
        {
            photonView.RPC(nameof(PlayerTakeDamage), RpcTarget.All, damage, photonView.ViewID);
        }
        else
        {
            PlayerTakeDamage(damage, photonView.ViewID);
        }
    }

    [PunRPC]
    public void PlayerTakeDamage(float damage, int viewID)
    {
        if (photonView.ViewID == viewID)
        {
            health -= damage;
            healthText.text = $"{health} HP";

            if (health <= 0)
            {
                gameManager.GameOver();
            }
            else
            {
                shakeTime = 0;
                hitPanel.alpha = 1f;
            }
        }
    }

    public void CameraShake()
    {
        playerCamera.transform.localRotation = Quaternion.Euler(Random.Range(-2f, 2f), 0, 0);
    }

    public void WeaponSwitch(int weaponIndex)
    {
        int index = 0;
        int amountOfWeapons = weaponHolder.transform.childCount;

        if (weaponIndex > amountOfWeapons - 1)
        {
            weaponIndex = 0;
        }

        foreach (Transform child in weaponHolder.transform)
        {
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
            }

            if (index == weaponIndex)
            {
                child.gameObject.SetActive(true);
                activeWeapon = child.gameObject;
            }

            index++;
        }

        activeWeaponIndex = weaponIndex;

        if (photonView.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("weaponIndex", weaponIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    public void UpdatePoints(int pointsToAdd)
    {
        totalPoints += pointsToAdd;
        pointsText.text = $"Points: {totalPoints}";
    }


    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        //Comprobar si el cambio solicitado no es mio y ademas si el targeplayer coincide con el owner con el photon view y el changepropses ! de null
        if (!photonView.IsMine && targetPlayer == photonView.Owner && changedProps["weaponIndex"]!= null)
        {
            WeaponSwitch((int)changedProps["weaponIndex"]);
        }
    }

    //Este metodo se lo llama desde WeaponManager
    [PunRPC]
    public void WeaponShootSFX(int viewID)
    {
        activeWeapon.GetComponent<WeaponManager>().ShootVFX(viewID);
    }
}
