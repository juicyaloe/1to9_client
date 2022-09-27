using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoticeUI : MonoBehaviour
{
    [Header("SubNotice")]
    public GameObject subbox;
    public Text subintext;
    public Animator subani;

    private WaitForSeconds _UIDelay1 = new WaitForSeconds(0.7f);
    private WaitForSeconds _UIDelay2 = new WaitForSeconds(0.3f);

    // Start is called before the first frame update
    void Start()
    {
        subbox.SetActive(false);
    }

    public void SUB(string message)
    {
        subintext.text = message;
        subbox.SetActive(false);
        StopCoroutine(SUBDelay());
        StartCoroutine(SUBDelay());
    }

    IEnumerator SUBDelay()
    {
        subbox.SetActive(true);
        subani.SetBool("isOn", true);
        yield return _UIDelay1;
        subani.SetBool("isOn", false);
        yield return _UIDelay2;
        subbox.SetActive(false);
    }
}
