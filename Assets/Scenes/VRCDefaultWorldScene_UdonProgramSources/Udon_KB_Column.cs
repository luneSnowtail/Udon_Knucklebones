
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using System.Collections.Generic;
using VRC.SDK3.Data;
using System.Linq;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Udon_KB_Column : UdonSharpBehaviour
{

    [UdonSynced, FieldChangeCallback(nameof(columnScore))]
    int _columnScore;
    public int columnScore
    {
        get => _columnScore;
        set
        {
            scoreTMP.text = value.ToString();
            _columnScore = value;
        }
    }

    [UdonSynced, FieldChangeCallback(nameof(latestDiceValue))]
    int _latestDiceValue;    
    public int latestDiceValue
    {
        get => _latestDiceValue;
        set
        {
            _latestDiceValue = value;
        }
    }

    [UdonSynced, FieldChangeCallback(nameof(latestTurn))]
    int _latestTurn;
    public int latestTurn
    {
        get => _latestTurn;
        set
        {
            _latestTurn = value;
        }
    }

    [UdonSynced, FieldChangeCallback(nameof(latestUpdateTurn))]
    int _latestUpdateTurn;
    public int latestUpdateTurn
    {
        get => _latestUpdateTurn;
        set
        {
            _latestUpdateTurn = value;
        }
    }

    public int latestOppositeScore;
    public Udon_KB_Column oppositeColumn;

    public TextMeshPro scoreTMP;
    public Udon_KB_PlayerData playerData;    

    public int columnId;

    public Transform slot1;
    public Transform slot2;
    public Transform slot3;

    public Udon_Dice dice1;
    public Udon_Dice dice2;
    public Udon_Dice dice3;    

    Udon_Dice[] diceList;
    Udon_Dice[] similarDice;

    private void Start()
    {
        diceList = new Udon_Dice[3];
        similarDice = new Udon_Dice[3];

        latestOppositeScore = oppositeColumn.columnScore;
    }

    public void Update()
    {

        if (!IsOwner())
            return;

        if(playerData.gameManager.currentPlayerTurn != playerData.playerTurnId && playerData.gameManager.currentPlayerTurn > 0)
        {
            //make sure that game manager waits for this to be updated, and the game should work 
            //maybe this can be done by syncing the last turn this was updated

            if(oppositeColumn.columnScore != latestOppositeScore)
            {
                latestOppositeScore = oppositeColumn.columnScore;
                latestDiceValue = 0;

                RemoveDicesWithValue(oppositeColumn.latestDiceValue);

                latestUpdateTurn = playerData.gameManager.currentGameTurn;

                UpdateColumnValue();
            }
            else
            {
                if(latestUpdateTurn != playerData.gameManager.currentGameTurn)
                {
                    latestUpdateTurn = playerData.gameManager.currentGameTurn;
                    RequestSerialization();
                }
            }
        }
        else
        {
            latestOppositeScore = oppositeColumn.columnScore;
        }
    }

    public int GetAvailableSlot()
    {        
        if(dice1 == null)
        {
            return 0;
        }
        if (dice2 == null)
        {
            return 1;
        }
        if (dice3 == null)
        {
            return 2;
        }

        //--this should never happen
        return 3;
    }

    public void PlaceInSlot(Udon_Dice dice, int slot)
    {
        latestDiceValue = dice.diceValue;
        latestTurn = playerData.gameManager.currentGameTurn;

        switch (slot)
        {
            case 0:
                dice1 = dice;
                dice.transform.position = slot1.transform.position;

                break;
            case 1:
                dice2 = dice;
                dice.transform.position = slot2.transform.position;

                break;
            case 2:
                dice3 = dice;
                dice.transform.position = slot3.transform.position;
                break;
        }

        UpdateColumnValue();
    }

    public bool IsSlotUsed(int slotID)
    {
        switch (slotID)
        {
            case 0:
                if (dice1)
                {
                    return true;
                }
                else
                {
                    return false;
                }             
            case 1:
                if (dice2)
                {
                    return true;
                }
                else
                {
                    return false;
                }               
            case 2:
                if (dice3)
                {
                    return true;
                }
                else
                {
                    return false;
                }                
        }

        return true;
    }

    public void RemoveDicesWithValue(int value)
    {
        if (dice1 != null) 
        {
            if(dice1.diceValue == value)
            {               
                dice1 = null;
            }
        }
        if (dice2 != null)
        {
            if (dice2.diceValue == value)
            {             
                dice2 = null;
            }
        }
        if (dice3 != null)
        {
            if (dice3.diceValue == value)
            {             
                dice3 = null;
            }
        }

        UpdateColumnValue();
    }

    public void UpdateColumnValue()
    {
        columnScore = 0;
        int similarDices = 0;

        diceList = new Udon_Dice[3];

        if (dice1)
            diceList[0] = dice1;
        if (dice2)
            diceList[1] = dice2;
        if (dice3)
            diceList[2] = dice3;

        for (int i = 1; i< 7; i++)
        {
            //--clear list of similar dice
            similarDice = new Udon_Dice[3];
            similarDices = 0;

            //--check dice value
            for(int j = 0; j< diceList.Length; j++) 
            {
                if (diceList[j] == null)
                    continue;

                //--if dice value match, add it to similar list
                if (diceList[j].diceValue == i)
                {
                    similarDice[similarDices] = diceList[j];
                    diceList[j] = null;
                    similarDices++;
                }
            }

            columnScore += (i * similarDices) * similarDices;
        }

        RequestSerialization();        
    }

    public int GetNumberOfUsedSlots()
    {
        int usedSlots = 0;

        if (dice1 != null)
            usedSlots++;
        if (dice2 != null)
            usedSlots++;
        if (dice3 != null)
            usedSlots++;
        return usedSlots;
    }

    public void ResetColumn()
    {
        dice1 = null;
        dice2 = null;
        dice3 = null;
        
        columnScore = 0;
    }



    bool IsOwner()
    {
        if (Networking.GetOwner(gameObject) == Networking.LocalPlayer)
            return true;
        return false;
    }
}
