using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class NetworkingManager : MonoBehaviourPunCallbacks
{
    public Button multiplayerButton;
    void Start()
    {
        Debug.Log("Conexion a un servidor");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Unirnos a un lobby");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Esta listo para el multijugador");
        multiplayerButton.interactable = true;
    }

    public void FindWatch()
    {
        Debug.Log("Buscando una sala");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        MakeRoom();
    }

    public void MakeRoom()
    {
        int randomRoomName = Random.Range(0, 5000);

        RoomOptions roomOptions = new RoomOptions()
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = 6,
            PublishUserId = true
        };

        PhotonNetwork.CreateRoom($"RoomName_{randomRoomName}", roomOptions);
        Debug.Log($"Sala Creada: {randomRoomName}");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Cargando Scena de juego");
        PhotonNetwork.LoadLevel(2);
    }
}
