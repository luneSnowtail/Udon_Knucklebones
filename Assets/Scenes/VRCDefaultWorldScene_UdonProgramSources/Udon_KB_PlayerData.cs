
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;


[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Udon_KB_PlayerData : UdonSharpBehaviour
{

    //--- sync variables

    [UdonSynced, FieldChangeCallback(nameof(playerName))]
    string _playerName;
    public string playerName
    {
        set
        {
            Debug.Log("player regered");            
            _playerName = value;
            playerNameTMP.text = value;
        }
        get => _playerName;
    }

    [UdonSynced, FieldChangeCallback(nameof(totalScore))]
    int _totalScore = 0;
    public int totalScore
    {
        get => _totalScore;
        set
        {
            _totalScore = value;
        }
    }

    // 0 -its not this player turn yet,
    // 1-spawn dice,
    // 2-dice rolled select column,
    // 3-dice placed update scores,
    // 4-my turn is finished ready to switch player or end game
    [UdonSynced, FieldChangeCallback(nameof(totalScore))]
    int _localState = 0;            
    public int localState
    {
        get => _localState;
        set
        {
            debugTurnText.text = value.ToString();
            _localState = value;
        }
    }

    //--- local variables
    public Udon_KB_GameManager gameManager;
    public Transform diceSpawner;

    public Udon_KB_Column[] columns;
    public Transform[] playerDices;

    public TextMeshPro scoreText;
    public TextMeshPro playerNameTMP;
    public TextMeshPro debugTurnText;



    public void RequestLocalOwnership()
    {
        Debug.Log("requesting ownership");
        Networking.SetOwner(Networking.LocalPlayer, this.gameObject);        

        if(Networking.GetOwner(gameObject) == Networking.LocalPlayer)
        {
            Debug.Log("you already own the object");
            playerName = Networking.LocalPlayer.displayName;
            RequestSerialization();
        }

        for(int i = 0; i< playerDices.Length; i++)
        {
            Networking.SetOwner(Networking.LocalPlayer, playerDices[i].gameObject);
        }
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        Debug.Log("ownership transfered");

        if (IsOwner())
        {
            playerName = player.displayName;
            RequestSerialization();
        }
    }

    bool IsOwner()
    {
        if (Networking.GetOwner(gameObject) == Networking.LocalPlayer)
            return true;
        return false;
    }

    public void UpdateScores()
    {
        totalScore = 0;
        for (int i =0; i < columns.Length; i++)
        {
            columns[i].UpdateColumnValue();
            totalScore += columns[i].columnScore;
        }

        scoreText.text = totalScore.ToString();
    }

    public void HideAllDices()
    {
        for(int i = 0 ; i < playerDices.Length; i++)
        {
            playerDices[i].gameObject.SetActive(false);
        }
    }

    public void SpawnAvailableDice()
    {
        for (int i = 0; i < playerDices.Length; i++)
        {
            if (!playerDices[i].gameObject.activeInHierarchy)
            {
                playerDices[i].gameObject.SetActive(true);
                playerDices[i].GetComponentInChildren<Rigidbody>().MovePosition(diceSpawner.position);   
                return;
            }
        }
    }

    public void DiceThrowed()
    {
        gameManager.SetGameState(2);
    }

    public void DicePlaced()
    {
        gameManager.SetGameState(3);
    }

    public int GetNumberOfUsedSlots()
    {
        int usedSlots = 0;

        for(int i = 0; i< columns.Length; i++)
        {
            usedSlots += columns[i].GetNumberOfUsedSlots();
        }

        return usedSlots;
    }

    public void ResetPlayerData()
    {
        playerName = string.Empty;

        RequestSerialization();

        HideAllDices();

        for( int i = 0; i < columns.Length; i++)
        {
            columns[i].ResetColumn();
        }

        totalScore = 0;

        UpdateScores();
    }
}
