using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int playerHP = 10;
    public int maxPlayerHP = 10;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }


    public void ReducePlayerHP(int amount)
    {
        playerHP -= amount;
        if (playerHP <= 0)
        {
            playerHP = 0;
            Debug.Log("プレイヤー死亡！");
            // ゲームオーバー処理など
        }
    }
}
