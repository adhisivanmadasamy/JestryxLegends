using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int TeamScore, EnemyScore;
    public int TeamCount, EnemyCount;

    public TextMeshProUGUI TeamScoreText, EnemyScoreText;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TeamWon()
    {
        TeamScore += 1;
    }
}
