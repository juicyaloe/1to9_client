using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using System;
using WebSocketSharp;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;

public class CardManager : MonoBehaviour
{
    public static NoticeUI _notice;

    // 웹 소켓 + 메세지 컨트롤 큐
    WebSocket ws;
    private Queue<JObject> messageQueue = new Queue<JObject>();

    public GameObject myDeckGameObject;
    public GameObject mySelectedCardGameObject;
    public GameObject mySelectedCardGameObject_before;
    public GameObject enemySelectedCardGameObject;
    
    public GameObject givemecard_text;
    public GameObject backButton;

    public Text scoreText;

    GameObject[] myCard = new GameObject[9];

    void Start()
    {
        _notice = FindObjectOfType<NoticeUI>();

        backButton.SetActive(false);

        for (int i = 0; i < 9; i++)
        {
            myCard[i] = myDeckGameObject.transform.GetChild(i).gameObject;
            myCard[i].transform.GetChild(0).GetComponent<Text>().text = (i+1).ToString();
        }

        try {
            ws = new WebSocket("ws://" + APIs.url + "/socket/" + APIs.token);
        
            // 콜백 추가
            ws.OnMessage += messageFunc;
            ws.OnClose += closeFunc;

            // 연결
            ConnectSocket();
            Debug.Log("연결 완료!");

            _notice.SUB("정상적으로 접속했습니다!\n카드를 골라주세요~!");
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

    public void messageFunc(object sender, MessageEventArgs e)
    {
        JObject json = JObject.Parse(e.Data);

        messageQueue.Enqueue(json);
    }

     // 카드를 내는 메시지를 보내는 함수
    void SendActionMessage(int number)
    {
        var body = JObject.FromObject(new { userid = APIs.id, gameroomid = APIs.gameroomid, mynumber = number });
        var json = JObject.FromObject(new { type = "doAction", body = body });
        var str = json.ToString();
        ws.Send(str);
    }

    // Update is called once per frame
    void Update()
    {
        // 메세지 큐 정리
        if (messageQueue.Count > 0)
        {
            JObject response = messageQueue.Dequeue();
            string type = response.GetValue("type").ToString();

            if (type == "pleaseAction")
            {
                _notice.SUB("상대방이 숫자를 제출했습니다!");
            }
            else if (type == "nextRound")
            {
                JObject body = (JObject)response.GetValue("body");
                string sender = body.GetValue("sender").ToString();

                string winner = body.GetValue("winner").ToString();

                int mynumber = body.GetValue(APIs.id).ToObject<int>();
                int counternumber = body.GetValue(APIs.counterid).ToObject<int>();

                int mywin = body.GetValue(APIs.id+"win").ToObject<int>();
                int counterwin = body.GetValue(APIs.counterid+"win").ToObject<int>();
                int draw = body.GetValue("draw").ToObject<int>();

                if (sender == APIs.id)
                {
                    givemecard_text.SetActive(true);

                    Destroy(EventSystem.current.currentSelectedGameObject);

                    mySelectedCardGameObject_before.transform.GetChild(0).GetComponent<Text>().text = mynumber.ToString();
                    mySelectedCardGameObject.transform.GetChild(0).GetComponent<Text>().text = "?";
                    
                    enemySelectedCardGameObject.transform.GetChild(0).GetComponent<Text>().text = counternumber.ToString();   
                }
                else
                {
                    givemecard_text.SetActive(true);

                    mySelectedCardGameObject_before.transform.GetChild(0).GetComponent<Text>().text = mynumber.ToString();
                    mySelectedCardGameObject.transform.GetChild(0).GetComponent<Text>().text = "?";
                    
                    enemySelectedCardGameObject.transform.GetChild(0).GetComponent<Text>().text = counternumber.ToString(); 
                }

                if (winner == APIs.id)
                {
                    _notice.SUB("이겼습니다! 다음 카드를 내주세요!");
                    drawScoreText(mywin, counterwin);
                }
                else if (winner == APIs.counterid)
                {
                    _notice.SUB("졌습니다ㅠㅠ 다음 카드를 내주세요!");
                    drawScoreText(mywin, counterwin);
                }
                else
                {
                    _notice.SUB("비겼습니다! 다음 카드를 내주세요!");
                    drawScoreText(mywin, counterwin);
                }

                // 초기화 과정
            }
            else if (type == "gameEnd")
            {
                JObject body = (JObject)response.GetValue("body");
                string sender = body.GetValue("sender").ToString();

                string winner = body.GetValue("winner").ToString();

                int mynumber = body.GetValue(APIs.id).ToObject<int>();
                int counternumber = body.GetValue(APIs.counterid).ToObject<int>();

                int mywin = body.GetValue(APIs.id+"win").ToObject<int>();
                int counterwin = body.GetValue(APIs.counterid+"win").ToObject<int>();
                int draw = body.GetValue("draw").ToObject<int>();

                if (sender == APIs.id)
                {
                    givemecard_text.SetActive(true);

                    Destroy(EventSystem.current.currentSelectedGameObject);

                    mySelectedCardGameObject_before.transform.GetChild(0).GetComponent<Text>().text = mynumber.ToString();
                    mySelectedCardGameObject.transform.GetChild(0).GetComponent<Text>().text = "?";
                    
                    enemySelectedCardGameObject.transform.GetChild(0).GetComponent<Text>().text = counternumber.ToString();   
                }
                else
                {
                    givemecard_text.SetActive(true);

                    mySelectedCardGameObject_before.transform.GetChild(0).GetComponent<Text>().text = mynumber.ToString();
                    mySelectedCardGameObject.transform.GetChild(0).GetComponent<Text>().text = "?";
                    
                    enemySelectedCardGameObject.transform.GetChild(0).GetComponent<Text>().text = counternumber.ToString(); 
                }

                drawScoreText(mywin, counterwin);

                if (mywin > counterwin)
                {
                    _notice.SUB("최종 승리했습니다!");
                }
                else if(mywin < counterwin)
                {
                    _notice.SUB("최종 패배했습니다ㅠㅠ");
                }
                else
                {
                    _notice.SUB("최종 결과는 무승부입니다!");
                }

                givemecard_text.GetComponent<Text>().text = "게임이 끝났습니다. \n뒤로가기 버튼을 눌려 나가주세요.";
                backButton.SetActive(true);
            }
            else if (type == "actionDo")
            {
                JObject body = (JObject)response.GetValue("body");
                string msg = body.GetValue("message").ToString();
                int code = body.GetValue("code").ToObject<int>();

                if (code == 200)
                {
                    int mynumber = body.GetValue("mynum").ToObject<int>();

                    _notice.SUB("숫자를 정상적으로 제출했습니다!");

                    givemecard_text.SetActive(false);

                    mySelectedCardGameObject.transform.GetChild(0).GetComponent<Text>().text = mynumber.ToString();
                    Destroy(EventSystem.current.currentSelectedGameObject);
                }
                else if (code == 404)
                {
                    Debug.Log("현재 게임중이 아닙니다.");
                }
                else if(code == 400)
                {
                    string error = body.GetValue("error").ToString();
                    switch (error)
                    {
                        case "isDone":
                            _notice.SUB("이미 카드를 냈습니다!");
                            break;
                        case "wrongNumber":
                            Debug.Log("잘못된 숫자입니다!");
                            break;
                        case "notGameMember":
                            Debug.Log("이 게임의 참여자가 아닙니다!");
                            break;
                        default:
                            Debug.Log("알수없는 오류입니다!");
                            break;
                    }      
                }
                else 
                {
                    Debug.Log("서버 오류입니다.");
                }
            }
            else
            {
                Debug.Log(type + "유형의 메세지는 알 수 없는 메시지입니다.");
            }
        }
    }
    
    public void SelectCardBtn(){
        string numStr = EventSystem.current.currentSelectedGameObject.transform.GetChild(0).GetComponent<Text>().text;
        SendActionMessage(int.Parse(numStr));
    }

    void drawScoreText(int mywin, int counterrwin)
    {
        scoreText.text = mywin.ToString() + " vs " + counterrwin.ToString();
    }

    public void backButtonclicked()
    {
        SceneManager.LoadScene(1);
    }
}
