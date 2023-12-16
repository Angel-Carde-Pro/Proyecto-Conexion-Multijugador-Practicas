using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using TMPro;
using Photon.Pun;

public class WeaponManager : MonoBehaviour
{
    public GameObject playerCam;
    public float range = 100f;
    public float damage = 25f;
    public Animator playerAnimator;

    public ParticleSystem flashParticleSystem;
    public GameObject bloodParticleSystem;
    public GameObject concreteParticleSystem;

    public AudioClip shootClip;
    public AudioSource weaponAudioSource;

    public WeaponSway weaponSway;
    public float swaySensitivity;

    public GameObject crossHair;

    public float currentAmmo;
    public float maxAmmo;
    public float reloadTime;
    public bool isReloading;
    public float reserveAmmo;
    public float reserveAmmoCap;

    public TextMeshProUGUI currentAmmoText;
    public TextMeshProUGUI reserveAmmoText;

    public float fireRate;
    public float fireRateTimer;
    public bool isAutomatic;

    public string weaponType;
    public PhotonView photonView;
    public GameManager gameManager;

    private void Start()
    {
        weaponAudioSource = GetComponent<AudioSource>();
        swaySensitivity = weaponSway.swaySensitivity;

        UpdateAmmoTexts();

        reserveAmmoCap = reserveAmmo;
    }

    void Update()
    {
        if (PhotonNetwork.InRoom && !photonView.IsMine)
        {
            return;
        }

        if (!gameManager.isPaused && !gameManager.isGameOver)
        {
            if (playerAnimator.GetBool("isShooting"))
            {
                playerAnimator.SetBool("isShooting", false);
            }

            if (reserveAmmo <= 0 && currentAmmo <= 0)
            {
                Debug.Log("Te has quedado sin balas");
                return;
            }

            if (currentAmmo <= 0 && !isReloading)
            {
                Debug.Log("No tienes balas");
                StartCoroutine(Reload(reloadTime));
                return;
            }

            if (isReloading)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.R) && reserveAmmo > 0)
            {
                Debug.Log("Recarga manual de las balas");
                StartCoroutine(Reload(reloadTime));
                return;
            }

            if (fireRateTimer > 0)
            {
                fireRateTimer -= Time.deltaTime;
            }

            if (Input.GetButton("Fire1") && fireRateTimer <= 0 && isAutomatic)
            {
                Shoot();
                fireRateTimer = 1 / fireRate;
            }

            if (Input.GetButtonDown("Fire1") && fireRateTimer <= 0 && !isAutomatic)
            {
                Shoot();
                fireRateTimer = 1 / fireRate;
            }

            if (Input.GetButtonDown("Fire2"))
            {
                Aim();
            }

            if (Input.GetButtonUp("Fire2"))
            {
                if (playerAnimator.GetBool("isAiming"))
                {
                    playerAnimator.SetBool("isAiming", false);
                }

                weaponSway.swaySensitivity = swaySensitivity;
                crossHair.SetActive(true);
            }
        }
    }

    private void OnEnable()
    {
        playerAnimator.SetTrigger(weaponType);

        UpdateAmmoTexts();

    }

    private void OnDisable()
    {
        playerAnimator.SetBool("isReloading", false);
        isReloading = false;
        Debug.Log("Recarga interrumpida por cambio de arma");
        if (!playerAnimator.GetBool("isAiming"))
        {
            crossHair.SetActive(true);
        }
    }

    private void Shoot()
    {
        currentAmmo--;
        UpdateAmmoTexts();

        if (PhotonNetwork.InRoom)
        {
            //Se lo llama asi por que el sistema de particula es hijo del arma hay un problema ya que los rpc no busca dentro de los hijos solo busca al mismo nivel
            photonView.RPC("WeaponShootSFX", RpcTarget.All, photonView.ViewID);
        }
        else
        {
            ShootVFX(photonView.ViewID);
        }

        playerAnimator.SetBool("isShooting", true);

        RaycastHit hit;
        //Para obtener lo que toque el raycast en este caso se va a verificar si tiene script enemymanager del zombie
        if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit, range))
        {
            EnemyManager enemyManager = hit.transform.GetComponent<EnemyManager>();
            if (enemyManager != null)
            {
                enemyManager.Hit(damage);
                //Para instanciar la sangreen el punto en el se le disparo
                //Quaternion.LookRotation es para que la rotacion siempre  nos vea a nosotros
                //Normal es el vector normal que a seguido el rayo pero en sentido inverso
                GameObject particleInstance = Instantiate(bloodParticleSystem, hit.point, Quaternion.LookRotation(hit.normal));
                //Para que se quede pegado en el objeto en este caso el zombie es como un splash la particula se hace hijo de el padre zombie.
                particleInstance.transform.parent = hit.transform;
            }
            //Si da en  otro lado es por que le da a un objetos entonces se activa la particula del concreto
            else
            {
                Instantiate(concreteParticleSystem, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
    }

    public void ShootVFX(int viewID)
    {
        if (photonView.ViewID == viewID)
        {
            flashParticleSystem.Play();
            weaponAudioSource.PlayOneShot(shootClip, 1);
        }
    }

    private void Aim()
    {
        playerAnimator.SetBool("isAiming", true);
        weaponSway.swaySensitivity = swaySensitivity / 100;
        crossHair.SetActive(false);
    }

    public IEnumerator Reload(float rt)
    {
        isReloading = true;
        playerAnimator.SetBool("isReloading", true);
        crossHair.SetActive(false);
        yield return new WaitForSeconds(rt);
        playerAnimator.SetBool("isReloading", false);
        if (!playerAnimator.GetBool("isAiming"))
        {
            crossHair.SetActive(true);
        }
        float missingAmmo = maxAmmo - currentAmmo;

        if (reserveAmmo >= missingAmmo)
        {
            currentAmmo += missingAmmo;
            reserveAmmo -= missingAmmo;
        }
        else
        {
            currentAmmo += reserveAmmo;
            reserveAmmo = 0;
        }

        if (gameObject.activeSelf)
        {
            UpdateAmmoTexts();
        }

        isReloading = false;
    }

    public void UpdateAmmoTexts()
    {
        currentAmmoText.text = currentAmmo.ToString();
        reserveAmmoText.text = reserveAmmo.ToString();
    }
}
