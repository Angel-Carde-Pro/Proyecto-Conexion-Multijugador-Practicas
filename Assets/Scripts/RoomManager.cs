using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    public static RoomManager sharedInstance;
    private void Awake()
    {
        if (sharedInstance == null)
        {
            sharedInstance = this;
            DontDestroyOnLoad(sharedInstance);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        //Suscribirse al evento onsceneloaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        //para evitar perdidas de memoria
        //Desuscribirse del evento onsceneloaded
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnDestroy()
    {
        //para evitar perdidas de memoria
        //Desuscribirse del evento onsceneloaded
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        //Posicion en donde se van a spaunear los jugadores
        Vector3 spawnPosition = new Vector3(Random.Range(-3f, 3f), 2, Random.Range(-3f, 3f));

        //Comprobar si nos conectamos en modo offline o en modo online
        if (PhotonNetwork.InRoom)
        {
            //Para instanciar con photon este hay q hacerlo con un string es un poco diferente
            PhotonNetwork.Instantiate("First_Person_Player", spawnPosition, Quaternion.identity);
        }
        else
        {
            Instantiate(Resources.Load("First_Person_Player"), spawnPosition, Quaternion.identity);
        }
    }
}
