using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class AccountManager : MonoBehaviour
{
    // 알림 창 구현
    public static NoticeUI _notice;

    // 패널 구현
    public GameObject RegisterPanel;
    public GameObject LoginPanel;

    // 로그인 입력 필드 구현
    public InputField ID_InputField;
    public InputField PW_InputField;

    // 회원가입 입력 필드 구현
    public InputField Create_ID_InputField;
    public InputField Create_PW_InputField;
    public InputField Create_PW2_InputField;
    public InputField Create_Email_InputField;
    public InputField Create_Nickname_InputField;

    void Start()
    {
        _notice = FindObjectOfType<NoticeUI>();
    }

    private void Update()
    {
        if(APIs.isLogin)
        {
            SceneManager.LoadScene(1);
        }
    }

    public void LoginBtn()
    {
        string _id = ID_InputField.text;
        string _pw = PW_InputField.text;

        WWWForm userInfo = new WWWForm();
        userInfo.AddField("id", _id);
        userInfo.AddField("password", _pw);
    
        StartCoroutine(APIs.login(userInfo));
    }

    public void RegisterBtn()
    {
        string _id = Create_ID_InputField.text;
        string _pw = Create_PW_InputField.text;
        string _pw2 = Create_PW2_InputField.text;
        string _email = Create_Email_InputField.text;
        string _nickname = Create_Nickname_InputField.text;

        if(ValidateRegister(_id, _pw, _pw2, _email, _nickname))
        {
            WWWForm userInfo = new WWWForm();
            userInfo.AddField("id", _id);
            userInfo.AddField("password", _pw);
            userInfo.AddField("email", _email);
            userInfo.AddField("nickname", _nickname);

            StartCoroutine(APIs.register(userInfo));
        }
    }

    public void ChangeModeBtn()
    {
        if(LoginPanel.activeInHierarchy)
        {
            LoginPanel.SetActive(false);
            RegisterPanel.SetActive(true);
        }
        else
        {
            LoginPanel.SetActive(true);
            RegisterPanel.SetActive(false);
        }
    }

    bool ValidateRegister(string id, string pw, string pw2, string email, string nickname)
    {
        List<string> errorlist = new List<string>();

        if(!Regex.IsMatch(id, @"^[0-9a-z]{3,12}"))
        {
            errorlist.Add("숫자, 영소문자를 이용해 3자이상 12자 이하 아이디를 만들어주세요.");
        }

        if (pw != pw2)
        {
            errorlist.Add("첫번째 비밀번호와 두번째 비밀번호가 일치하지 않습니다.");
        }

        if (!(pw.Length >= 8 && pw.Length <= 20))
        {
            errorlist.Add("8자이상 20자 이하 비밀번호를 만들어주세요.");
        }


        if (!Regex.IsMatch(email, @"[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?"))
        {
            errorlist.Add("올바른 이메일 형식이 아닙니다.");
        }

        if(nickname.Length < 3)
        {
            errorlist.Add("3자 이상의 닉네임을 만들어주세요.");
        }

        // 여기에 오류 창 띄우도록 구현
        string data = "";
        errorlist.ForEach((err) => data += err + '\n');
        
        if (errorlist.Count > 0)
        {
            _notice.SUB(data);
            return false;
        }
        else return true;
    }
}
