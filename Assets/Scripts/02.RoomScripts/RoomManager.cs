using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using WebSocketSharp;
using Newtonsoft.Json.Linq;

using System;

public class RoomManager : MonoBehaviour
{
    // 웹 소켓 + 메세지 컨트롤 큐
    WebSocket ws;
    private Queue<JObject> messageQueue = new Queue<JObject>();

   
    // Panel
    public GameObject RoomSelectPanel;
    public GameObject RoomPanel;

    // 현재 방 상태인지 판단하는 변수
    public static bool isRoom = false;

    // ?? 알아서 정리하시길
    public Transform RoomTransform;
    public GameObject RoomObject;

    public Text PlayerInfoText;

    void Start()
    {
        try {
            ws = new WebSocket("ws://43.200.124.214/socket/" + APIs.token);
        
            // 콜백 추가
            ws.OnMessage += messageFunc;
            ws.OnClose += closeFunc;

            // 연결
            ConnectSocket();
        }
        catch {
            // catch 경우 없음
        }
    }

    // 연결 종료시 call 되는 콜백들
    void ConnectSocket()
    {
        try {
            ws.Connect();
        }
        catch (Exception e){
            // 비정상 종료
            Debug.Log(e.ToString());
            Debug.Log("서버 오류");
        }
    }

    public void closeFunc(object sender, CloseEventArgs e)
    {
        // 정상 종료
        Debug.Log(e.Code);

        //e.Code : 1001번->유효기간이 지난 토큰
        //e.Code : 1002번->없는 토큰
        //e.Code : 1006번->서버 에러
    }

    // MessageControlFunc
    public void messageFunc(object sender, MessageEventArgs e)
    {
        JObject json = JObject.Parse(e.Data);

        messageQueue.Enqueue(json);
    }

    void Update()
    {
        // Room in or out
        if (isRoom) GoToRoom();
        else GoToRoomSelect();

        if (messageQueue.Count > 0)
        {
            JObject response = messageQueue.Dequeue();
            string type = response.GetValue("type").ToString();

            if (type == "roomUpdate")
            {
                Debug.Log("룸 업데이트");
                StartCoroutine(roomUpdate());
            }
            else if (type == "test")
            {
                Debug.Log("테스트 메시지가 왔어요~");
                StartCoroutine(roomUpdate());
            }
            else
            {
                Debug.Log("알수 없는 메시지입니다.");
            }
        }

        // test code
        if (Input.GetKeyDown(KeyCode.Space))
            StartCoroutine(roomUpdate());
    }

    void OnDestroy()
    {
        ws.Close();
    }

    IEnumerator roomUpdate()
    {
        yield return StartCoroutine(APIs.getRoomList());

        Debug.Log(RoomTransform.childCount + "개의 방 제거");
        for (int i = 0; i < RoomTransform.childCount; i++)
        {
            Destroy(RoomTransform.GetChild(i).gameObject);
        }
        
        foreach (KeyValuePair<int, string> item in APIs.Rooms)
        {
            GameObject roomObj = RoomObject;
            roomObj.transform.GetChild(0).GetComponent<Text>().text = item.Value;
            Instantiate(roomObj, RoomTransform);
        }
        Debug.Log(RoomTransform.childCount + "개의 방 생성");

        Debug.Log("ROOM UPDATE FINISH");
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
}