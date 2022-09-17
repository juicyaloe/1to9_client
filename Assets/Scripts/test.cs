using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WebSocketSharp;
using Newtonsoft.Json.Linq;

public class test : MonoBehaviour
{
    private WebSocket ws;

    // Start is called before the first frame update
    void Start()
    {
        ws = new WebSocket("ws://43.200.124.214/socket/");
        ws.Connect();

        ws.OnMessage += (sender, e) => {
            JObject json = JObject.Parse(e.Data);
            var a = json.GetValue("data");

            Debug.Log("주소 :  "+((WebSocket)sender).Url+", 데이터 : "+a);
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (ws == null)
        {
            Debug.Log("연결 x");
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            var json = JObject.FromObject(new {data = "안녕"});
            var str = json.ToString();
            ws.Send(str);
        }
    }

    void OnDestroy(){
        Debug.Log("Closed");
        ws.Close();
    }
}