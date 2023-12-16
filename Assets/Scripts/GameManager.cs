using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using Photon.Realtime;

//El MonoBehaviourPunCallbacks trae todas las funcionalidades de MonoBehaviour pero ademas unas cuantas de pun
public class GameManager : MonoBehaviourPunCallbacks
{
    public int enemiesAlive;
    public int round;
    public GameObject[] spawnPoints;
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI roundsSurvivedText;

    public GameObject gameOverPanel;
    public GameObject pausePanel;

    public Animator fadePanelAnimator;

    public bool isPaused;
    public bool isGameOver;
    public PhotonView photonView;

    private void Start()
    {
        isPaused = false;
        isGameOver = false;
        Time.timeScale = 1;

        spawnPoints = GameObject.FindGameObjectsWithTag("Spawners");
        DisplayNextRound(round);
    }

    void Update()
    {
        //Si estoy online hago la logica de generar enemigos de caso contrario cada jugador tendria diferentes rondas
        //el master es el q va a jugar
        if (PhotonNetwork.InRoom || PhotonNetwork.IsMasterClient && photonView.IsMine)
        {
            if (enemiesAlive == 0)
            {
                round++;
                NextWave(round);
                if (PhotonNetwork.InRoom)
                {
                    //Un objeto de tipo Hashtable tiene clave y tiene valor
                    Hashtable hash = new Hashtable();
                    //Con esto guardamos la ronda actual en la que estamos
                    hash.Add("currentRound", round);
                    //Y aqui suministramos ese puntaje al resto de jugadores con el setcustom..
                    PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
                }
                else
                {
                    DisplayNextRound(round);
                }
            }
        }

        //Cada uno es libre de pausar su juego osea solo abir el panel si esta en online
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausePanel.activeSelf)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void NextWave(int round)
    {
        for (int i = 0; i < round; i++)
        {
            int randomPos = Random.Range(0, spawnPoints.Length);
            GameObject spawnPoint = spawnPoints[randomPos];

            GameObject enemyInstance;

            //Si estamos online se instanciaran los enemigos para todos

            if (PhotonNetwork.InRoom)
            {
                enemyInstance = PhotonNetwork.Instantiate("Zombie", spawnPoint.transform.position, Quaternion.identity);
            }
            else
            {
                //Castear a gameObject con el as
                enemyInstance = Instantiate(Resources.Load("Zombie"), spawnPoint.transform.position, Quaternion.identity) as GameObject;
            }

            enemyInstance.GetComponent<EnemyManager>().gameManager = GetComponent<GameManager>();
            enemiesAlive++;
        }
    }

    public void GameOver()
    {
        gameOverPanel.SetActive(true);
        roundsSurvivedText.text = round.ToString();

        if (!PhotonNetwork.InRoom)
        {
            Time.timeScale = 0;
        }

        Cursor.lockState = CursorLockMode.None;
        isGameOver = true;
    }

    public void RestartGame()
    {
        if (!PhotonNetwork.InRoom)
        {
            Time.timeScale = 1;
        }
        SceneManager.LoadScene(1);
    }

    public void BackToMainMenu()
    {
        if (!PhotonNetwork.InRoom)
        {
            Time.timeScale = 1;
        }
        AudioListener.volume = 1;
        fadePanelAnimator.SetTrigger("FadeIn");
        Invoke("LoadMainMenuScene", 0.5f);
    }

    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene(0);
    }

    public void Pause()
    {
        pausePanel.SetActive(true);
        AudioListener.volume = 0;
        if (!PhotonNetwork.InRoom)
        {
            Time.timeScale = 0;
        }
        Cursor.lockState = CursorLockMode.None;
        isPaused = true;
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
        AudioListener.volume = 1;
        if (!PhotonNetwork.InRoom)
        {
            Time.timeScale = 1;
        }
        Cursor.lockState = CursorLockMode.Locked;
        isPaused = false;
    }

    private void DisplayNextRound(int round)
    {
        roundText.text = $"Round: {round}";
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (photonView.IsMine)
        {
            //Para saber si este valor ya esta guardado osea si ya existe
            if (changedProps["currentRound"] != null)
            {
                //El DisplayNextRound del gamemanager el q esta arribita xd, recibe un valor int entonces lo casteamos
                DisplayNextRound((int)changedProps["currentRound"]);
            }
        }
    }
}
