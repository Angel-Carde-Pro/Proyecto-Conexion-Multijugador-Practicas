using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{

    public int price = 50;
    public TextMeshProUGUI priceNumber;
    public TextMeshProUGUI priceText;

    private PlayerManager playerManager;
    private bool playerIsInReach;

    public bool isHealthShop;
    public bool isAmmoShop;
    
    void Start()
    {
        priceText.text = price.ToString();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            BuyShop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerIsInReach = true;
            priceText.gameObject.SetActive(true);
            priceNumber.gameObject.SetActive(true);
            playerManager = other.GetComponent<PlayerManager>();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerIsInReach = false;
            priceText.gameObject.SetActive(false);
            priceNumber.gameObject.SetActive(false);
        }
    }

    public void BuyShop()
    {
        if (playerIsInReach)
        {
            if (playerManager.totalPoints >= price)
            {
                playerManager.UpdatePoints(-price);

                if (isHealthShop)
                {
                    playerManager.health = playerManager.healthCap;
                    playerManager.healthText.text = $"{playerManager.healthCap} HP";
                }

                if (isAmmoShop)
                {
                    foreach (Transform child in playerManager.weaponHolder.transform)
                    {
                        WeaponManager weaponManager = child.GetComponent<WeaponManager>();
                        weaponManager.currentAmmo = weaponManager.maxAmmo;
                        weaponManager.reserveAmmo = weaponManager.reserveAmmoCap;
                        StartCoroutine(weaponManager.Reload(weaponManager.reloadTime));
                    }
                }
                
            }
            else
            {
                Debug.Log("No tienes puntos suficientes");
            }
            
        }
    }
}
