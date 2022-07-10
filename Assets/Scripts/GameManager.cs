using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Transform scoreContents;
    public GameObject scorePanel;

    public static Dictionary<int, Team> teams = new Dictionary<int, Team>();

    public void Awake()
    {
        instance = this;
    }
    public static void GainScore(int teamNumber)
    {
        TMPro.TMP_Text Text = teams[teamNumber].teamText;
        int newScore = int.Parse(Text.text) + 1;
        Text.text = newScore.ToString();
        if (newScore == 10)
        {
            Debug.Log("Oyun bitti! kazanan takým numarasý = " + teamNumber);
            // TO DO : ???
        }
    }

    public static void AddTeam(TeamBase _teamBase)
    {
        teams.Add(_teamBase.teamNumber, new Team(_teamBase));
    }

    public class Team
    {
        public TeamBase teamBase;
        public Image image;
        public TMPro.TMP_Text teamText;

        public Team(TeamBase teamBase)
        {
            this.teamBase = teamBase;
            GameObject myContent = Instantiate(instance.scorePanel, instance.scoreContents);
            image = myContent.GetComponentInChildren<Image>();
            teamText = myContent.GetComponentInChildren<TMPro.TMP_Text>();
            image.color = teamBase.teamColor;
        }
    }
}
