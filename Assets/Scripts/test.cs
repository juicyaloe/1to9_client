using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class test : MonoBehaviour
{
    public GameObject myDeck;
    GameObject[] myCard = new GameObject[9];
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 9; i++)
        {
            myCard[i] = myDeck.transform.GetChild(i).gameObject;
            myCard[i].transform.GetChild(0).GetComponent<Text>().text = i.ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
