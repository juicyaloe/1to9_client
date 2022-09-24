using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using Newtonsoft.Json.Linq;

public static class APIs
{
    // 회원 정보 변수
    public static string id;
    public static string email;
    public static string nickname;

    // 로그인 여부 변수
    public static bool isLogin = false;
    public static string token;

    // 방 목록 변수
    public static Dictionary<int, string> Rooms = new Dictionary<int, string>();
    
    // 로그인 모듈
    public static IEnumerator login(WWWForm userInfo)
    {
        UnityWebRequest www = UnityWebRequest.Post("http://43.200.124.214/api/profile/login", userInfo);
        yield return www.SendWebRequest();

        if (www.responseCode == 200)
        {
            JObject response = JObject.Parse(www.downloadHandler.text);

            Debug.Log("로그인 성공");
            token = response.GetValue("accessToken").ToString();
            isLogin = true;
        }
        else if (www.responseCode == 401)
        {
            AccountManager._notice.SUB("틀린 비밀번호입니다.");
        }
        else if (www.responseCode == 404)
        {
            AccountManager._notice.SUB("등록되지 않은 ID입니다.");
        }
        else
        {
            AccountManager._notice.SUB("서버 오류입니다.");
        }

        www.Dispose();
    }

    // 회원가입 모듈
    public static IEnumerator register(WWWForm userInfo)
    {
        UnityWebRequest www = UnityWebRequest.Post("http://43.200.124.214/api/profile/register", userInfo);
        yield return www.SendWebRequest();

        if (www.responseCode == 201)
        {
            AccountManager._notice.SUB("회원가입이 성공했습니다! 로그인 화면으로 돌아가 로그인 해주세요.");
        }
        else if (www.responseCode == 400)
        {
            JObject response = JObject.Parse(www.downloadHandler.text);
            string errmessage = response.GetValue("error").ToString();

            switch (errmessage)
            {
                case "repeatedId":
                    AccountManager._notice.SUB("중복된 아이디입니다.");
                    break;
                case "repeatedEmail":
                    AccountManager._notice.SUB("중복된 이메일입니다.");
                    break;
                case "repeatedNickname":
                    AccountManager._notice.SUB("중복된 닉네임입니다.");
                    break;
                default:
                    AccountManager._notice.SUB("오류입니다.");
                    break;

            }
            Debug.Log(errmessage);
        }
        else
        {
            AccountManager._notice.SUB("서버 오류입니다.");
        }

        www.Dispose();
    }


    // token에 해당하는 회원 정보를 static id, email, nickname에 저장
    public static IEnumerator getInfo()
    {
        UnityWebRequest www = UnityWebRequest.Get("http://43.200.124.214/api/profile/token");
        www.SetRequestHeader("Authorization", token);

        yield return www.SendWebRequest();

        if (www.error == null)
        {
            Debug.Log(www.downloadHandler.text);

            JObject response = JObject.Parse(www.downloadHandler.text);
            JObject info = (JObject)response.GetValue("content");
            
            string _id = info.GetValue("id").ToString();
            string _email = info.GetValue("email").ToString();
            string _nickname = info.GetValue("nickname").ToString();

            id = _id; email = _email; nickname = _nickname;
        }
        else
        {
            Debug.Log(www.responseCode);
            Debug.Log(www.downloadHandler.text);    
        }

        www.Dispose();
    }

    // 방의 목록을 RoomManager.Room에 넣기
    public static IEnumerator getRoomList()
    {
        UnityWebRequest www = UnityWebRequest.Get("http://43.200.124.214/api/room/all");
        www.SetRequestHeader("Authorization", token);

        yield return www.SendWebRequest();

        if (www.error == null)
        {
            JObject response = JObject.Parse(www.downloadHandler.text);
            Dictionary<int, string> rooms = new Dictionary<int, string>();

            foreach (JObject item in response.GetValue("contents"))
            {
                int id = int.Parse(item.GetValue("id").ToString());
                string roomName = item.GetValue("name").ToString();
                rooms[id] = roomName;
            } 

            Rooms = rooms;
        }
        else
        {
            Debug.Log(www.responseCode);
            Debug.Log(www.downloadHandler.text);     
        }

        www.Dispose();
    }

    public static IEnumerator getRoomInfo(string roomName)
    {
        UnityWebRequest www = UnityWebRequest.Get("http://43.200.124.214/api/room/name/"+ roomName);
        www.SetRequestHeader("Authorization", token);
        
        yield return www.SendWebRequest();

        if (www.error == null)
        {
            JObject response = JObject.Parse(www.downloadHandler.text);
            List<Dictionary<string, string>> userlist = new List<Dictionary<string, string>>();
            foreach (JObject item in response.GetValue("contents"))
            {
                Dictionary<string, string> userinfo = new Dictionary<string, string>();

                userinfo["id"] = item.GetValue("id").ToString();
                userinfo["email"] = item.GetValue("email").ToString();
                userinfo["nickname"] = item.GetValue("nickname").ToString();
                userlist.Add(userinfo);
            }
            RoomManager.currentRoomMember = userlist;
        }
        else
        {
            Debug.Log(www.responseCode);
            Debug.Log(www.downloadHandler.text);
        }

        www.Dispose();
    }
}
