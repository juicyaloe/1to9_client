using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using Newtonsoft.Json.Linq;

public static class APIs
{
    public static string id;
    public static bool isLogin = false;
    public static string token;
    public static IEnumerator login(WWWForm userInfo)
    {
        UnityWebRequest www = UnityWebRequest.Post("http://43.200.124.214/api/profile/login", userInfo);
        yield return www.SendWebRequest();

        if (www.error == null)
        {
            JObject response = JObject.Parse(www.downloadHandler.text);
            token = response.GetValue("accessToken").ToString();
            isLogin = true;
            Debug.Log(token);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            Debug.Log(www.responseCode);
            //switch (www.responseCode)
            //    case 400:
        }
    }

    public static IEnumerator register(WWWForm userInfo)
    {
        UnityWebRequest www = UnityWebRequest.Post("http://43.200.124.214/api/profile/register", userInfo);
        yield return www.SendWebRequest();

        if (www.error == null)
        {
            Debug.Log(www.downloadHandler.text);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            Debug.Log(www.responseCode);
        }
    }


    public static IEnumerator getInfo()
    {
        UnityWebRequest www = UnityWebRequest.Get("http://43.200.124.214/api/profile/token");
        www.SetRequestHeader("Authorization", token);

        JObject response = JObject.Parse(www.downloadHandler.text);
        id = response.GetValue("id").ToString();

        yield return www.SendWebRequest();

        if (www.error == null)
        {
            Debug.Log(www.downloadHandler.text);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            Debug.Log(www.responseCode);
        }
    }

    public static IEnumerator getRoomList()
    {
        UnityWebRequest www = UnityWebRequest.Get("http://43.200.124.214/api/room/all");
        yield return www.SendWebRequest();

        if (www.error == null)
        {
            JObject response = JObject.Parse(www.downloadHandler.text);
            Dictionary<int, string> rooms = new Dictionary<int, string>();
            foreach (JObject item in response.GetValue("content"))
            {
                int id = int.Parse(item.GetValue("id").ToString());
                string roomName = item.GetValue("name").ToString();
                rooms[id] = roomName;
            }

            RoomManager.Rooms = rooms;
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            Debug.Log(www.responseCode);
        }
    }
}
