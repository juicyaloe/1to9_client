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
    public GameObject enemySelectedCardGameObject;

    public Text scoreText;

    int myScore = 0;
    int enemyScore = 0;

    int myCurrentNum = 0;
    int enemyCurrentNum = 0;

    GameObject[] myCard = new GameObject[9];

    void Start()
    {
        _notice = FindObjectOfType<NoticeUI>();

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

    // Update is called once per frame
    void Update()
    {
        OnStage();

        // 메세지 큐 정리
        if (messageQueue.Count > 0)
        {
            JObject response = messageQueue.Dequeue();
            string type = response.GetValue("type").ToString();

            if (type == "roomUpdate")
            {
                Debug.Log("방이 업데이트 되었습니다.");
            }
            else
            {
                Debug.Log(type + "유형의 메세지는 알 수 없는 메시지입니다.");
                string error = response.GetValue("error").ToString();
                Debug.Log(error);
            }
        }
    }

    void OnStage(){
        if(myCurrentNum != 0 && enemyCurrentNum != 0)
        {   
            if(myCurrentNum > enemyCurrentNum)
                myScore++;
            else if(myCurrentNum < enemyCurrentNum)
                enemyScore++;
            scoreText.text = myScore.ToString() + " vs " + enemyScore.ToString();
            myCurrentNum = 0;
            enemyCurrentNum = 0;
        }
    }

    void getEnemyInfo()
    {
        enemyCurrentNum = UnityEngine.Random.Range(1, 10);
        enemySelectedCardGameObject.transform.GetChild(0).GetComponent<Text>().text = enemyCurrentNum.ToString();
    }
    
    public void SelectCardBtn(){
        string numStr = EventSystem.current.currentSelectedGameObject.transform.GetChild(0).GetComponent<Text>().text;
        myCurrentNum = int.Parse(numStr);
        mySelectedCardGameObject.transform.GetChild(0).GetComponent<Text>().text = numStr;
        Destroy(EventSystem.current.currentSelectedGameObject);

        getEnemyInfo();
    }

}
