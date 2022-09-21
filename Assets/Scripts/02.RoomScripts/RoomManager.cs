using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using WebSocketSharp;
using Newtonsoft.Json.Linq;

public class RoomManager : MonoBehaviour
{
    WebSocket ws;
    public bool isUpdate = false;

    public static Dictionary<int, string> Rooms = new Dictionary<int, string>();
    //Panel
    public GameObject RoomSelectPanel;
    public GameObject RoomPanel;


    public static bool isRoom = false;

    public Transform RoomTransform;
    public GameObject RoomObject;
    public static string[] RoomNameList = { "qweqew", "123", "aaaa" };

    public Text PlayerInfoText;

    void Start()
    {
        ws = new WebSocket("ws://43.200.124.214/socket/"+APIs.token);
        ws.Connect();

        ws.OnMessage += messageFunc;

        ws.OnClose += (sender, e) => {
            Debug.Log(e.Code);

        };
        //e.Code : 1001번->유효기간이 지난 토큰
        //e.Code : 1002번->없는 토큰
        //e.Code : 1006번->서버 에러
    }



    public void messageFunc(object sender, MessageEventArgs e)
    {
        JObject json = JObject.Parse(e.Data);

        if (json.GetValue("type").ToString() == "roomUpdate")
        {
            isUpdate = true;
        }

        if (json.GetValue("type").ToString() == "test")
        {
            Debug.Log("HI");
        }
    }

    void Update()
    {
        if (isRoom) GoToRoom();
        else GoToRoomSelect();

        if (Input.GetKeyDown(KeyCode.Space))
            roomUpdate();

        if (isUpdate)
        {
            roomUpdate();
            isUpdate = false;
        }
    }








    void roomUpdate()
    {
        Debug.Log("UPDATE!");
        StartCoroutine(drawRoomList());

    }

    void setPlayer()
    {
        PlayerInfoText.text = "PLAYER";
    }




    public void GoToRoom()
    {
        RoomSelectPanel.SetActive(false);
        RoomPanel.SetActive(true);

    }

    public void CreateRoom()
    {
        isRoom = true;
        var body = JObject.FromObject(new { id = APIs.id, roomname = APIs.id + "의 방" });
        var json = JObject.FromObject(new { type = "createRoom", body = body });
        var str = json.ToString();
        ws.Send(str);
    }
    
    public void GoToRoomSelect()
    {
        RoomPanel.SetActive(false);
        RoomSelectPanel.SetActive(true);
    }

    IEnumerator drawRoomList()
    {
        Debug.Log("drawstart");
        yield return StartCoroutine(APIs.getRoomList());
        Debug.Log(RoomTransform.childCount);

        for (int i = 0; i < RoomTransform.childCount; i++)
        {
            Destroy(RoomTransform.GetChild(i).gameObject);
        }
        
        Debug.Log(RoomTransform.childCount);
        foreach (KeyValuePair<int, string> item in Rooms)
        {
            Debug.Log("for");
            GameObject roomObj = RoomObject;
            roomObj.transform.GetChild(0).GetComponent<Text>().text = item.Value;
            Instantiate(roomObj, RoomTransform);
        }
    }
}