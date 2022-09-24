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
    public GameObject RoomCreatePanel;
    public InputField roomnameField;

    // 방 목록 그리기
    public Transform RoomTransform;
    public GameObject RoomObject;


    public static string currentRoomName = "";

    public static bool isRoomButtonClicked = false;
    public static string roomName = "";

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
        StartCoroutine(roomUpdate());
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
        switch (e.Code)
        {
            case 1001:
                Debug.Log("유효기간이 지난 토큰입니다.");
                break;
            case 1002:
                Debug.Log("유효하지 않은 토큰입니다.");
                break;
            case 1005:
                Debug.Log("정상종료 되었습니다.");
                break;
            case 1006:
                Debug.Log("서버오류로 종료되었습니다.");
                break;
            default:
                Debug.Log(e.Code);
                break;
        }
    }

    // MessageControlFunc
    public void messageFunc(object sender, MessageEventArgs e)
    {
        JObject json = JObject.Parse(e.Data);

        messageQueue.Enqueue(json);
    }

    void Update()
    {
        if(isRoomButtonClicked)
        {
            isRoomButtonClicked = false;

            var body = JObject.FromObject(new { id = APIs.id, roomname = roomName });
            var json = JObject.FromObject(new { type = "visitRoom", body = body });
            var str = json.ToString();
            ws.Send(str);
            Debug.Log("방 들어가기" + APIs.id);
        }
        if (messageQueue.Count > 0)
        {
            JObject response = messageQueue.Dequeue();
            string type = response.GetValue("type").ToString();
            if (type == "roomUpdate")
            {
                Debug.Log("룸 업데이트");
                StartCoroutine(roomUpdate());
            }
            else if (type == "roomCreate")
            {
                JObject body = (JObject)response.GetValue("body");
                string msg = body.GetValue("message").ToString();
                int code = body.GetValue("code").ToObject<int>();

                Debug.Log(msg + code);
                if (code == 201)
                {
                    currentRoomName = roomnameField.text;
                    GoRoomPanel();
                }
            }
            else if (type == "roomVisit")
            {
                JObject body = (JObject)response.GetValue("body");
                string msg = body.GetValue("message").ToString();
                int code = body.GetValue("code").ToObject<int>();
                Debug.Log(msg);
                if (code == 200)
                {
                    currentRoomName = roomName;
                    GoRoomPanel();
                }
            }
            else if (type == "roomLeave")
            {
                JObject body = (JObject)response.GetValue("body");
                string msg = body.GetValue("message").ToString();
                int code = body.GetValue("code").ToObject<int>();
                if (code == 200 || code == 202 || code == 204)
                {
                    Debug.Log("Exit Room");

                    RoomPanel.SetActive(false);
                    RoomCreatePanel.SetActive(false);
                    RoomSelectPanel.SetActive(true);

                    StartCoroutine(roomUpdate());

                }
                Debug.Log(msg);
            }
            else if (type == "roomMemberUpdate")
            {

            }
            else
            {
                Debug.Log(type + " 알수 없는 메시지입니다.");
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

        for (int i = 0; i < RoomTransform.childCount; i++)
        {
            Destroy(RoomTransform.GetChild(i).gameObject);
        }

        RoomTransform.DetachChildren();
        
        foreach (KeyValuePair<int, string> item in APIs.Rooms)
        {
            GameObject roomObj = RoomObject;
            roomObj.transform.GetChild(0).GetComponent<Text>().text = item.Value;
            roomObj.transform.GetChild(0).GetComponent<Text>().fontSize = 40;
            Instantiate(roomObj, RoomTransform);
        }
        Debug.Log(RoomTransform.childCount + "개의 방 생성");
    }








    // 방 선택 화면으로 갈 때 부르는 함수
    public void GoToRoomSelect()
    {

        var body = JObject.FromObject(new { id = APIs.id, roomname = currentRoomName });
        var json = JObject.FromObject(new { type = "leaveRoom", body = body });
        var str = json.ToString();
        ws.Send(str);
        Debug.Log("방 나가기 버튼 눌림");
    }

    // 방 만들기 form을 켜고 끄는 함수
    public void RoomCreatePanelControl()
    {
        if (RoomCreatePanel.activeSelf == true)
        {
            RoomCreatePanel.SetActive(false);
            roomnameField.text = "";
        }
        else
        {
            RoomCreatePanel.SetActive(true);
        }
    }

    // 방을 만들고 들어가는 함수
    public void GoToRoomCreate()
    {
        var body = JObject.FromObject(new { id = APIs.id, roomname = roomnameField.text });
        var json = JObject.FromObject(new { type = "createRoom", body = body });
        var str = json.ToString();
        ws.Send(str);
        Debug.Log("방 만들기");
    }

    public void GoRoomPanel()
    {
        RoomPanel.SetActive(true);
        RoomCreatePanel.SetActive(false);
        RoomSelectPanel.SetActive(false);
    }


}