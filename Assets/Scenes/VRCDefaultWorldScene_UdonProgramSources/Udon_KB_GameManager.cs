
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Udon_KB_GameManager : UdonSharpBehaviour
{
    //---------Debug Stuff
    public TextMeshPro debugText_owner;
    public TextMeshPro p2nameText;
    public TextMeshPro p1nameText;

    public TextMeshPro gameStateText;
    public TextMeshPro playerTurnText;


    //---------- Network serialized data

    [UdonSynced, FieldChangeCallback(nameof(player1Name))]
    public string _player1Name;
    public string player1Name
    {
        set
        {
            Debug.Log("player 1 regered");            
            p1nameText.text = value;
            _player1Name = value;

            if(IsOwner())
                CheckRegisteredPlayers();
        }
        get => _player1Name;
    }    //player 1 is also the owner of game manager

    [UdonSynced, FieldChangeCallback(nameof(player2Name))]
    public string _player2Name;
    public string player2Name      //player 2 only owns their game data objects
      {
        set
        {
            Debug.Log("player 2 registered");
            p2nameText.text = value;
            _player2Name = value;

            if (IsOwner())
                CheckRegisteredPlayers();
        }
        get => _player2Name;
    }    //player 1 is also the owner of game manager

    [UdonSynced, FieldChangeCallback(nameof(ownerName))]
    public string _ownerName;
    public string ownerName
    {
        set
        {
            debugText_owner.text = value;
            _ownerName = value;
        }

        get => _ownerName;
    }

    [UdonSynced, FieldChangeCallback(nameof(gameState))]
    int _gameState;
    public int gameState
    {
        set
        {
            _gameState = value;
            gameStateText.text = value.ToString();
            SetGameState(value);
        }

        get => _gameState;
    }

    [UdonSynced, FieldChangeCallback(nameof(currentPlayerTurn))]
    int _currentPlayerTurn;
    public int currentPlayerTurn   //0 means game has not started, 1 is player 1, 2 is player 2,
    {
        set
        {
            _currentPlayerTurn = value;
            playerTurnText.text = value.ToString();
            SetCurrentPlayer(value);
        }

        get => _currentPlayerTurn;
    }


    //------------

    public GameObject playButton;
    public Udon_KB_Button playButtonUdon;

    public GameObject resetButton;

    public TMPro.TextMeshPro victoryText;

    public Udon_KB_PlayerData player1Objects;
    public Udon_KB_PlayerData player2Objects;

    public Udon_KB_PlayerData currentPlayer
    {
        get
        {
            Debug.Log("requesting stuff for player id " + currentPlayerTurn.ToString());

            if(currentPlayerTurn == 1)
            {
                return player1Objects;
            }
            if(currentPlayerTurn == 2) 
            {
                return player2Objects;
            }
            return null;
        }
        set
        {

        }
    }
    public Udon_KB_PlayerData oppositePlayer;

    public int latestDiceValue;
    public int latestColumnUsed;

    

    bool requestUpdateSerialization;

    #region ========== UdonStuff
    public void RequestLocalOwnership()
    {
        Debug.Log("requesting ownership");

        playButtonUdon.RequestLocalOwnership();

        if ( Networking.GetOwner(gameObject) == Networking.LocalPlayer)
        {
            Debug.Log("you already own this object");
            ownerName = Networking.LocalPlayer.displayName;            
            RequestSerialization();
        }
        else
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        }

    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        Debug.Log("ownership transfered");        
        debugText_owner.text = player.displayName;
    }

    #endregion



    #region ============= Monobehaviour

    private void Start()
    {
        ResetTableLocal();
    }


    private void Update()
    {
        if (!IsOwner())
            return;

        if(player1Name != player1Objects.playerName)
        {
            player1Name = player1Objects.playerName;
            RequestSerialization();
        }

        if (player2Name != player2Objects.playerName)
        {
            player2Name = player2Objects.playerName;
            RequestSerialization();
        }
    }

    #endregion

    void CheckRegisteredPlayers()
    {
        if(player1Name != string.Empty && player2Name != string.Empty)
        {
            gameState = 0;
            RequestSerialization();
        }
    }

    bool IsOwner()
    {
        if (Networking.GetOwner(gameObject) == Networking.LocalPlayer)
            return true;
        return false;
    }

    public void ResetTableLocal()
    {
        //reset manually to not call serialization before time
        _gameState = -1;
        _currentPlayerTurn = 0;


        currentPlayer = null;
        oppositePlayer = null;
        latestDiceValue = 0; 
        latestColumnUsed = 0;

        victoryText.gameObject.SetActive(false);
        resetButton.SetActive(false);
        playButton.SetActive(false);

        player1Objects.ResetPlayerData();
        player2Objects.ResetPlayerData();
    }

    public void ResetSerializedData()
    {
        //--serialized data
        player1Name = string.Empty;
        player2Name = string.Empty;

        gameState = -1;
        currentPlayerTurn = 0;

        RequestSerialization();
    }

    public void StartGame()
    {
        resetButton.SetActive(false);
        playButton.SetActive(false);

        gameState = 1;

        _currentPlayerTurn = 1;
        currentPlayerTurn = 1;

        //--call here localy to prevent errors on first turn

        RequestSerialization();

        //SetCurrentPlayer(1);
        //SetGameState(1);
    }

    void SetCurrentPlayer(int player)
    {
        if(player == 1)
        {
            currentPlayer = player1Objects;
            oppositePlayer = player2Objects;            
        }
        else if( player == 2)
        {
            currentPlayer = player2Objects;
            oppositePlayer = player1Objects;            
        }          
    }

    public void SetGameState(int newState)
    {
        /* - OLD
        //-1 - game has not started
        // 0 - players registered but game has not started
        // 1 - current player gets a dice and has to throw it
        // 2 - player has to chose a column
        // 3 - scores are updated and we change the turn of the player
        // 4 - we switch player turn
        // 5 - game has ended
        */

        // -1 - game has not started, game is stopped, nothing is happening
        //  0 - players are registering to play but game has not started
        //  1 - a new player turn is assigned
        //  2 - player finished their turn, update scores, switch turn or end game
        //  3 - game ended, show scores and restart table for a new game

        switch (newState)
        {
            case -1:
                playButton.SetActive(false);
                resetButton.SetActive(false);
                break;
            case 0:

                if (IsOwner())
                {
                    playButton.SetActive(true);
                }
                resetButton.SetActive(false);
                break;

                /*
            case 1:

                if (!currentPlayer)
                {
                    player1Objects.SpawnAvailableDice();
                }
                else
                {
                    currentPlayer.SpawnAvailableDice();
                }

                break;
            case 2:

                break;
            case 3:
                oppositePlayer.columns[latestColumnUsed].RemoveDicesWithValue(latestDiceValue);

                currentPlayer.UpdateScores();
                oppositePlayer.UpdateScores();

                if(currentPlayer.GetNumberOfUsedSlots() == 9 || oppositePlayer.GetNumberOfUsedSlots() == 9)
                {
                    //SetGameState(5);
                    gameState = 5;
                }
                else
                {
                    //SetGameState(4);
                    gameState = 4;
                }


                break;
            case 4:
                SwitchPlayer();
                SetGameState(1);
                break;
            case 5:
                GetFinalScore();
                }*/

                break;
        }
    }

    void SwitchPlayer()
    {
        if(currentPlayerTurn == 1)
        {
            currentPlayerTurn = 2;
            RequestSerialization();
            return;
        }
        if(currentPlayerTurn == 2)
        {
            currentPlayerTurn = 1;
            RequestSerialization();
            return;
        }
    }

    void GetFinalScore()
    {
        player1Objects.UpdateScores();
        player2Objects.UpdateScores();
        resetButton.SetActive(true);


        victoryText.gameObject.SetActive(true);

        if (player1Objects.totalScore > player2Objects.totalScore)
        {
            //player 1 wins
            victoryText.text = "Player 1 wins";
        }
        else
        {
            //player 2 wins
            victoryText.text = "Player 2 wins";
        }

    }
}
