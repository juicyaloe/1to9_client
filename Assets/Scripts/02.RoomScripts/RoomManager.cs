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

    // 방 내부 관련 변수
    public static string currentRoomName = "";
    public Text currentRoomText;
    
    // 방에 들어갔을 때 컨트롤 변수
    public static bool isRoomSelected = false;
    public static string SelectedRoomName = "";

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

        // 초기화 세팅
        StartCoroutine(APIs.getInfo());
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

    void OnDestroy()
    {
        ws.Close();
    }

    // MessageControlFunc
    public void messageFunc(object sender, MessageEventArgs e)
    {
        JObject json = JObject.Parse(e.Data);

        messageQueue.Enqueue(json);
    }

    // 방을 만드는 메시지를 보내는 함수
    public void SendRoomCreateMessage()
    {
        var body = JObject.FromObject(new { id = APIs.id, roomname = roomnameField.text });
        var json = JObject.FromObject(new { type = "createRoom", body = body });
        var str = json.ToString();
        ws.Send(str);
    }

    // 방을 들어가는 메시지를 보내는 함수
    public void SendRoomVisitMessage()
    {
        var body = JObject.FromObject(new { id = APIs.id, roomname = SelectedRoomName });
        var json = JObject.FromObject(new { type = "visitRoom", body = body });
        var str = json.ToString();
        ws.Send(str);
    }

    // 방을 나가는 메시지를 부르는 함수
    public void SendRoomLeaveMessage()
    {
        var body = JObject.FromObject(new { id = APIs.id, roomname = currentRoomName });
        var json = JObject.FromObject(new { type = "leaveRoom", body = body });
        var str = json.ToString();
        ws.Send(str);
    }

    void Update()
    {
        // 방에 그냥 들어갔을 때 메시지를 보낸다.
        if(isRoomSelected)
        {
            isRoomSelected = false;
            SendRoomVisitMessage();
        }

        // 메세지 큐 정리
        if (messageQueue.Count > 0)
        {
            JObject response = messageQueue.Dequeue();
            string type = response.GetValue("type").ToString();

            if (type == "roomUpdate")
            {
                Debug.Log("방이 업데이트 되었습니다.");
                StartCoroutine(roomUpdate());
            }
            else if (type == "roomMemberUpdate")
            {
                Debug.Log("방의 인원이 업데이트 되었습니다.");
                StartCoroutine(RoomMemberUpdate());
            }
            else if (type == "roomCreate")
            {
                JObject body = (JObject)response.GetValue("body");
                string msg = body.GetValue("message").ToString();
                int code = body.GetValue("code").ToObject<int>();
;
                if (code == 201)
                {
                    currentRoomName = roomnameField.text;
                    GoRoomPanel();
                    StartCoroutine(RoomMemberUpdate());
                }
                else if (code == 400)
                {
                    Debug.Log("이미 만들어진 방입니다.");
                }
                else
                {
                    Debug.Log("서버 오류입니다.");
                }
            }
            else if (type == "roomVisit")
            {
                JObject body = (JObject)response.GetValue("body");
                string msg = body.GetValue("message").ToString();
                int code = body.GetValue("code").ToObject<int>();

                if (code == 200)
                {
                    currentRoomName = SelectedRoomName;
                    GoRoomPanel();
                    StartCoroutine(RoomMemberUpdate());
                }
                else if (code == 400)
                {
                    Debug.Log("이미 방에 들어왔습니다.");
                }
                else if (code == 404)
                {
                    Debug.Log("이미 없어진 방입니다.");
                }
                else
                {
                    Debug.Log("서버 오류입니다.");
                }
            }
            else if (type == "roomLeave")
            {
                JObject body = (JObject)response.GetValue("body");
                string msg = body.GetValue("message").ToString();
                int code = body.GetValue("code").ToObject<int>();

                if (code == 200 || code == 202 || code == 204)
                {
                    GoRoomSelectPanel();
                    StartCoroutine(roomUpdate());
                }
                else
                {
                    Debug.Log(msg);
                }         
            }
            else
            {
                Debug.Log(type + "유형의 메세지는 알 수 없는 메시지입니다.");
            }
        }
    }

    // 방 목록을 다시 그리는 함수
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
    }

    // 방 멤버를 다시 그리는 함수
    IEnumerator RoomMemberUpdate()
    {
        yield return StartCoroutine(APIs.getRoomInfo(currentRoomName));

        string roomMemberInformation = "";
        foreach (var item in APIs.currentRoomMember)
        {
            string id = item["id"];
            string email = item["email"];
            string nickname = item["nickname"];
            roomMemberInformation += nickname + "(" + id + ")" + "유저가 입장했습니다.\n";
        }

        currentRoomText.text = roomMemberInformation;
    }

    // 방 만들기 form을 켜고 끄는 함수
    public void RoomCreatePanelControl()
    {
        if (RoomCreatePanel.activeSelf == true)
        {
            RoomCreatePanel.SetActive(false);
        }
        else
        {
            RoomCreatePanel.SetActive(true);
            roomnameField.text = "";
        }
    }

    // 방 선택 화면으로 갈 때 부르는 함수
    public void GoRoomSelectPanel()
    {
        RoomPanel.SetActive(false);
        RoomCreatePanel.SetActive(false);
        RoomSelectPanel.SetActive(true);
    }

    // 방 화면으로 갈 때 부르는 함수
    public void GoRoomPanel()
    {
        RoomCreatePanel.SetActive(false);
        RoomSelectPanel.SetActive(false);
        RoomPanel.SetActive(true);
    }

}