using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour
{
    public GameObject player;

    private GameObject[] playerInScene;
    public Animator enemyAnimator;

    public float damage = 20f;
    public float health = 100f;

    public GameManager gameManager;

    public Slider healthBar;

    //Si el player esta al alcanze
    public bool playerInReach;
    //Delay para el tiempo de ataque
    public float attackDelayTimer;
    //Que tan pronto debe empezar la animacion de ataque
    public float howMuchEarlierStartAttackAnim;
    //Delay entre los ataques
    public float delayBetweenAttacks;

    public AudioSource enemyAudioSource;
    public AudioClip[] growlAudioClips;

    public int points = 20;

    public PhotonView photonView;

    void Start()
    {
        playerInScene = GameObject.FindGameObjectsWithTag("Player");
        enemyAudioSource = GetComponent<AudioSource>();
        healthBar.maxValue = health;
        healthBar.value = health;
    }

    void Update()
    {
        if (!enemyAudioSource.isPlaying)
        {
            enemyAudioSource.clip = growlAudioClips[Random.Range(0, growlAudioClips.Length)];
            enemyAudioSource.Play();
        }

        //Toda esta logica lo va  a llevar el master
        if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
        {
            return;
        }
        GetClosesPlayer();

        if (player != null)
        {
            GetComponent<NavMeshAgent>().destination = player.transform.position;

            //Para que mire constantemente al player
            healthBar.transform.LookAt(player.transform);
        }

        if (GetComponent<NavMeshAgent>().velocity.magnitude > 1)
        {
            enemyAnimator.SetBool("isRunning", true);
        }
        else
        {
            enemyAnimator.SetBool("isRunning", false);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject == player)
        {
            playerInReach = true;
        }
    }

    //Mientras este en contacto con el player ejecutar esto (es como un update)
    private void OnCollisionStay(Collision other)
    {
        if (playerInReach)
        {
            attackDelayTimer += Time.deltaTime;

            if (attackDelayTimer >= delayBetweenAttacks - howMuchEarlierStartAttackAnim && attackDelayTimer <= delayBetweenAttacks)
            {
                enemyAnimator.SetTrigger("isAttacking");
            }

            if (attackDelayTimer >= delayBetweenAttacks)
            {
                player.GetComponent<PlayerManager>().Hit(damage);
                attackDelayTimer = 0;
            }
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject == player)
        {
            playerInReach = false;
            attackDelayTimer = 0;
        }
    }

    public void Hit(float damage)
    {
        //Cuando un zombie recibe daño hay que comunicar a todos los jugadores
        photonView.RPC(nameof(TakeDamage), RpcTarget.All, damage, photonView.ViewID);
    }

    //Para poder utilizar RPC hay q poner esta etiqueta
    [PunRPC]
    public void TakeDamage(float damage, int viewID)
    {
        //Para el unico zombie que va a recibir el daño por todos los zombies tienen este script
        if (photonView.ViewID == viewID)
        {
            health -= damage;
            healthBar.value = health;

            if (health <= 0)
            {
                healthBar.gameObject.SetActive(false);
                enemyAnimator.SetTrigger("isDead");

                if (!PhotonNetwork.InRoom || (PhotonNetwork.IsMasterClient && photonView.IsMine))
                {
                    gameManager.enemiesAlive--;

                }

                player.GetComponent<PlayerManager>().UpdatePoints(points);

                Destroy(gameObject, 10f);
                Destroy(GetComponent<NavMeshAgent>());
                Destroy(GetComponent<EnemyManager>());
                Destroy(GetComponent<CapsuleCollider>());
            }
        }
    }

    private void GetClosesPlayer()
    {
        float minDIstance = Mathf.Infinity;
        //Para calcular la distancia del zombie de cada uno de los jugadores
        Vector3 currentPosition = transform.position;
        foreach (GameObject p in playerInScene)
        {
            //Par evitar errores de null references osea si no hay ningun jugador
            if (p != null)
            {
                //Calcular la distancia
                float distance = Vector3.Distance(p.transform.position, currentPosition);
                //Si el otro jugar esta mas cerca sobre escribiremos esa distancia minima
                if (distance < minDIstance)
                {
                    player = p;
                    minDIstance = distance;
                }
            }
        }
    }
}
