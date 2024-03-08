using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class Launcher : MonoBehaviourPunCallbacks
{

    [SerializeField] TMP_InputField RoomNameInputfield;
    [SerializeField] TMP_Text roomName;
    [SerializeField] TMP_Text errorText;

   

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Connecting to master server");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master server");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        MenuManager.instance.OpenMenu("main");
    }

    public void CreateRoom()
    {
        if(string.IsNullOrEmpty(RoomNameInputfield.text))
        {
            return;
        }
        PhotonNetwork.CreateRoom(RoomNameInputfield.text);
        MenuManager.instance.OpenMenu("loading");
    }

    public override void OnJoinedRoom()
    {
        MenuManager.instance.OpenMenu("inroom");
        roomName.text = PhotonNetwork.CurrentRoom.Name;
 
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        MenuManager.instance.OpenMenu("error");
        errorText.text = message;   
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        MenuManager.instance.OpenMenu("main");
    }
}
