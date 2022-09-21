using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AccountManager : MonoBehaviour
{
    public GameObject RegisterPanel;
    public GameObject LoginPanel;

    public InputField ID_InputField;
    public InputField PW_InputField;

    public InputField Create_ID_InputField;
    public InputField Create_PW_InputField;
    public InputField Create_PW2_InputField;
    public InputField Create_Email_InputField;
    public InputField Create_Nickname_InputField;

    public static string Response;
    private void Update()
    {
        if(APIs.isLogin)
        {
            //SceneManager.LoadScene(1);
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(APIs.getInfo());
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

        if (_pw != _pw2)
        {
            Debug.Log("비밀번호가 다름");
            return;
        }

        WWWForm userInfo = new WWWForm();
        userInfo.AddField("id", _id);
        userInfo.AddField("password", _pw);
        userInfo.AddField("email", _email);
        userInfo.AddField("nickname", _nickname);

        StartCoroutine(APIs.register(userInfo));

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
}
