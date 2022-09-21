using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomPanelChange : MonoBehaviour
{
    public void GoToRoomBtn()
    {
        RoomManager.isRoom = true;
    }

    public void GoToRoomSelectBtn()
    {
        RoomManager.isRoom = false;
    }
}
