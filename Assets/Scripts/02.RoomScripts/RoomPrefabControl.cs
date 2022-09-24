using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomPrefabControl : MonoBehaviour
{
    public void GoToRoom()
    {
        RoomManager.SelectedRoomName = this.transform.GetChild(0).GetComponent<Text>().text;
        RoomManager.isRoomSelected = true;
    }
}
