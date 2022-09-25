using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;

public static class APIs
{
    public static string url = "127.0.0.1:8000";
    // 회원 정보 변수
    public static string id;
    public static string email;
    public static string nickname;

    // 로그인 여부 변수
    public static bool isLogin = false;
    public static string token;

    // 방 목록 변수
    public static Dictionary<int, string> Rooms = new Dictionary<int, string>();
    // 방 인원 변수
    public static List<Dictionary<string, string>> currentRoomMember = new List<Dictionary<string, string>>();
    
    // 로그인 모듈
    public static IEnumerator login(WWWForm userInfo)
    {
        UnityWebRequest www = UnityWebRequest.Post("http://" + url + "/api/profile/login", userInfo);
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
        UnityWebRequest www = UnityWebRequest.Post("http://" + url + "/api/profile/register", userInfo);
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
        UnityWebRequest www = UnityWebRequest.Get("http://" + url + "/api/profile/token");
        www.SetRequestHeader("Authorization", token);

        yield return www.SendWebRequest();

        if (www.error == null)
        {
            JObject response = JObject.Parse(www.downloadHandler.text);
            JObject info = (JObject)response.GetValue("content");
            
            string _id = info.GetValue("id").ToString();
            string _email = info.GetValue("email").ToString();
            string _nickname = info.GetValue("nickname").ToString();

            id = _id; email = _email; nickname = _nickname;
        }
        else
        {
            if (www.responseCode == 419 || www.responseCode == 401)
            {
                Debug.Log("토큰에 문제가 있습니다. 다시 로그인해주세요.");
                SceneManager.LoadScene(0);
            }
            else
            {
                Debug.Log("서버 오류입니다.");
            } 
        }

        www.Dispose();
    }

    // 방의 목록을 RoomManager.Room에 넣기
    public static IEnumerator getRoomList()
    {
        UnityWebRequest www = UnityWebRequest.Get("http://" + url + "/api/room/all");
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
            if (www.responseCode == 419 || www.responseCode == 401)
            {
                Debug.Log("토큰에 문제가 있습니다. 다시 로그인해주세요.");
                SceneManager.LoadScene(0);
            }
            else
            {
                Debug.Log("서버 오류입니다.");
            }   
        }

        www.Dispose();
    }

    public static IEnumerator getRoomInfo(string roomName)
    {
        UnityWebRequest www = UnityWebRequest.Get("http://" + url + "/api/room/name/"+ roomName);
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
            
            currentRoomMember = userlist;
        }
        else
        {
            if (www.responseCode == 419 || www.responseCode == 401)
            {
                Debug.Log("토큰에 문제가 있습니다. 다시 로그인해주세요.");
                SceneManager.LoadScene(0);
            }
            else
            {
                Debug.Log("서버 오류입니다.");
            }
        }

        www.Dispose();
    }
}
