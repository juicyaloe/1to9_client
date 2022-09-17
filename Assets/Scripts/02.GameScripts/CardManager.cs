using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class CardManager : MonoBehaviour
{
    public GameObject myDeckGameObject;
    public GameObject mySelectedCardGameObject;
    public GameObject enemySelectedCardGameObject;

    public Text scoreText;

    int myScore = 0;
    int enemyScore = 0;

    int myCurrentNum = 0;
    int enemyCurrentNum = 0;

    GameObject[] myCard = new GameObject[9];
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 9; i++)
        {
            myCard[i] = myDeckGameObject.transform.GetChild(i).gameObject;
            myCard[i].transform.GetChild(0).GetComponent<Text>().text = (i+1).ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        OnStage();
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
        enemyCurrentNum = Random.Range(1, 10);
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
