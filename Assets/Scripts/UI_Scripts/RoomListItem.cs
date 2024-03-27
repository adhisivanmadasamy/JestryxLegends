using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Realtime;

public class RoomListItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    public RoomInfo info;


    public void SetUp(RoomInfo _info)
    {
        info = _info;
        text.text = _info.Name;
    }

    public void OnClick()
    {
        Launcher.Instance.JoinRoom(info);
        Debug.Log("Joined the room: "+ info.Name);
    }
  
}
