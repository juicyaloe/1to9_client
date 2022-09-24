using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomPrefabControl : MonoBehaviour
{
    // Start is called before the first frame update

    public void GoToRoom()
    {
        RoomManager.roomName = this.transform.GetChild(0).GetComponent<Text>().text;
        RoomManager.isRoomButtonClicked = true;
    }
}
