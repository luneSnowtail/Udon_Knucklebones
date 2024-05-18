
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using System.Collections;


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
            scoreText.text = value.ToString();
            _totalScore = value;
        }
    }

    [UdonSynced, FieldChangeCallback(nameof(localState))]
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

    VRCPlayerApi currentOwner;

    #region =============== Dice sync vars

    [UdonSynced, FieldChangeCallback(nameof(dice1Active))]
    bool _dice1Active;
    public bool dice1Active
    {
        get => _dice1Active;
        set
        {
            _dice1Active = value;
            playerDices[0].gameObject.SetActive(value);
        }
    }

    [UdonSynced, FieldChangeCallback(nameof(dice2Active))]
    bool _dice2Active;
    public bool dice2Active
    {
        get => _dice2Active;
        set
        {
            _dice2Active = value;
            playerDices[1].gameObject.SetActive(value);
        }
    }

    [UdonSynced, FieldChangeCallback(nameof(dice3Active))]
    bool _dice3Active;
    public bool dice3Active
    {
        get => _dice3Active;
        set
        {
            _dice3Active = value;
            playerDices[2].gameObject.SetActive(value);
        }
    }

    [UdonSynced, FieldChangeCallback(nameof(dice4Active))]
    bool _dice4Active;
    public bool dice4Active
    {
        get => _dice4Active;
        set
        {
            _dice4Active = value;
            playerDices[3].gameObject.SetActive(value);
        }
    }

    [UdonSynced, FieldChangeCallback(nameof(dice5Active))]
    bool _dice5Active;
    public bool dice5Active
    {
        get => _dice5Active;
        set
        {
            _dice5Active = value;
            playerDices[4].gameObject.SetActive(value);
        }
    }

    [UdonSynced, FieldChangeCallback(nameof(dice6Active))]
    bool _dice6Active;
    public bool dice6Active
    {
        get => _dice6Active;
        set
        {
            _dice6Active = value;
            playerDices[5].gameObject.SetActive(value);
        }
    }

    [UdonSynced, FieldChangeCallback(nameof(dice7Active))]
    bool _dice7Active;
    public bool dice7Active
    {
        get => _dice7Active;
        set
        {
            _dice7Active = value;
            playerDices[6].gameObject.SetActive(value);
        }
    }

    [UdonSynced, FieldChangeCallback(nameof(dice8Active))]
    bool _dice8Active;
    public bool dice8Active
    {
        get => _dice8Active;
        set
        {
            _dice8Active = value;
            playerDices[7].gameObject.SetActive(value);
        }
    }

    [UdonSynced, FieldChangeCallback(nameof(dice9Active))]
    bool _dice9Active;
    public bool dice9Active
    {
        get => _dice9Active;
        set
        {
            _dice9Active = value;
            playerDices[8].gameObject.SetActive(value);
        }
    }

    #endregion 

    //--- local variables
    public int column1Score;
    public int column2Score;
    public int column3Score;

    public int playerTurnId;        //--which player turn this data belongs to

    public Udon_KB_GameManager gameManager;
    public Udon_KB_PlayerData oppositePlayer;

    public Transform diceSpawner;

    public Udon_KB_Column[] columns;
    public Transform[] playerDices;

    public TextMeshPro scoreText;
    public TextMeshPro playerNameTMP;
    public TextMeshPro debugTurnText;

    Udon_Dice currentDice;



    public void RequestLocalOwnership()
    {
        Debug.Log("requesting ownership");

        Networking.SetOwner(Networking.LocalPlayer, this.gameObject);        

        if(Networking.GetOwner(gameObject) == Networking.LocalPlayer)
        {
            Debug.Log("you already own the object");

            currentOwner = Networking.LocalPlayer;
            playerName = currentOwner.displayName;
            RequestSerialization();
        }

        for(int i = 0; i< playerDices.Length; i++)
        {
            Networking.SetOwner(Networking.LocalPlayer, playerDices[i].GetComponentInChildren<Udon_Dice>().gameObject);
        }

        for(int i = 0; i<columns.Length; i++)
        {
            Networking.SetOwner(Networking.LocalPlayer, columns[i].gameObject);
        }
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        Debug.Log("ownership transfered");
        currentOwner = player;

        if (IsOwner())
        {
            playerName = player.displayName;
            RequestSerialization();
        }
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {     
        if(player == currentOwner)
        {
            localState = -1;
        }
    }


    void ResetNetworkData()
    {
        playerName = string.Empty;
        totalScore = 0;
        localState = -1;

        RequestSerialization();
    }

    private void Update()
    {
        if (gameManager.currentPlayerTurn != playerTurnId)
            return;

        if (gameManager.gameState < 0)
            return;

        if (!IsOwner())
            return;
        
        if(localState > 0 && currentOwner == null)
        {
            localState = -1;
            RequestSerialization();
            return;
        }

        // 0 -its not this player turn yet,
        // 1-spawn dice,
        // 2-dice rolled select column,
        // 3-dice placed update scores,
        // 4-my turn is finished, waiting for opposite player to discard dices if needed
        // 5-this turn is over, go back to state 0 and change player

        if (dice1Active && !playerDices[0].gameObject.activeInHierarchy)
        {
            dice1Active = false;
            RequestSerialization();
        }
        if (dice2Active && !playerDices[1].gameObject.activeInHierarchy)
        {
            dice2Active = false;
            RequestSerialization();
        }
        if (dice3Active && !playerDices[2].gameObject.activeInHierarchy)
        {
            dice3Active = false;
            RequestSerialization();
        }
        if (dice4Active && !playerDices[3].gameObject.activeInHierarchy)
        {
            dice4Active = false;
            RequestSerialization();
        }
        if (dice5Active && !playerDices[4].gameObject.activeInHierarchy)
        {
            dice5Active = false;
            RequestSerialization();
        }
        if (dice6Active && !playerDices[5].gameObject.activeInHierarchy)
        {
            dice6Active = false;
            RequestSerialization();
        }
        if (dice7Active && !playerDices[6].gameObject.activeInHierarchy)
        {
            dice7Active = false;
            RequestSerialization();
        }
        if (dice8Active && !playerDices[7].gameObject.activeInHierarchy)
        {
            dice8Active = false;
            RequestSerialization();
        }
        if (dice9Active && !playerDices[8].gameObject.activeInHierarchy)
        {
            dice9Active = false;
            RequestSerialization();
        }

        switch (localState)
        {
            case 0:
                if(gameManager.gameState == 1)
                {
                    localState = 1;

                    column1Score = columns[0].columnScore;
                    column2Score = columns[1].columnScore;
                    column3Score = columns[2].columnScore;


                    SpawnAvailableDice();
                    RequestSerialization();
                }
                break;
            case 1:
                if(currentDice.diceValue != 0)
                {
                    localState = 2;
                    RequestSerialization();
                }
                break;
            case 2:                       
                if(column1Score != columns[0].columnScore || column2Score != columns[1].columnScore || column3Score != columns[2].columnScore)
                {
                    column1Score = columns[0].columnScore;
                    column2Score = columns[1].columnScore;
                    column3Score = columns[2].columnScore;

                    localState = 3;
                    RequestSerialization();
                }
                break;
            case 3:
                totalScore = 0;
                for (int i = 0; i < columns.Length; i++)
                {
                    totalScore += columns[i].columnScore;
                }

                if(GetNumberOfUsedSlots() == 9)
                {
                    localState = 5;
                    RequestSerialization();
                    return;
                }

                //--wait for the opposite columns to report they updated and discarded dices properly before finishing our turn
                if (
                    oppositePlayer.columns[0].latestUpdateTurn == gameManager.currentGameTurn && 
                    oppositePlayer.columns[1].latestUpdateTurn == gameManager.currentGameTurn && 
                    oppositePlayer.columns[2].latestUpdateTurn == gameManager.currentGameTurn)
                {
                    localState = 4;
                }

                RequestSerialization();

                break;
            case 4:
                if (GetNumberOfUsedSlots() == 9)
                {
                    localState = 5;
                    RequestSerialization();
                }
                else
                {
                    if (gameManager.gameState == 3)
                    {
                        localState = 0;
                        RequestSerialization();
                    }
                }
                break;

            case 5:
                //i filled the 9 slots, end game now

                break;
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
            totalScore += columns[i].columnScore;
        }        
    }

    public void HideAllDices()
    {
        for(int i = 0 ; i < playerDices.Length; i++)
        {
            playerDices[i].gameObject.SetActive(false);
        }
    }

    public Udon_Dice SpawnAvailableDice()
    {
        for (int i = 0; i < playerDices.Length; i++)
        {            
            if (!playerDices[i].gameObject.activeInHierarchy)
            {
                playerDices[i].gameObject.SetActive(true);

                currentDice = playerDices[i].GetComponentInChildren<Udon_Dice>();
                currentDice.GetComponentInChildren<Rigidbody>().MovePosition(diceSpawner.position);

                dice1Active = playerDices[0].gameObject.activeInHierarchy;
                dice2Active = playerDices[1].gameObject.activeInHierarchy;
                dice3Active = playerDices[2].gameObject.activeInHierarchy;
                dice4Active = playerDices[3].gameObject.activeInHierarchy;
                dice5Active = playerDices[4].gameObject.activeInHierarchy;
                dice6Active = playerDices[5].gameObject.activeInHierarchy;
                dice7Active = playerDices[6].gameObject.activeInHierarchy;
                dice8Active = playerDices[7].gameObject.activeInHierarchy;
                dice9Active = playerDices[8].gameObject.activeInHierarchy;
                
                RequestSerialization();

                return currentDice;
            }
        }                  
        return null;        
    }

    int GetNumberOfUsedSlots()
    {
        int usedSlots = 0;

        for(int i = 0; i< columns.Length; i++)
        {
            usedSlots += columns[i].GetNumberOfUsedSlots();
        }

        return usedSlots;
    }

    public void SetLocalState(int newState)
    {
        if (!IsOwner())
        {
            Debug.Log("requested to set a state but i do not own this object");
            return;
        }

        Debug.Log("setting new state as " + newState.ToString());

        localState = newState;
        RequestSerialization();
    }

    public void ResetPlayerDataLocal()
    {
        playerName = string.Empty;
        totalScore = 0;
        localState = 0;

        HideAllDices();
        for( int i = 0; i < columns.Length; i++)
        {
            columns[i].ResetColumn();
        }
        totalScore = 0;

        UpdateScores();
    }


}
