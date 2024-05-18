
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
    }

    public void PlaceDice(Udon_Dice dice)
    {
        int availableSlot = GetAvailableSlot();

        Debug.Log("Available slot is " + availableSlot.ToString());

        PlaceInSlot(dice, GetAvailableSlot());
        UpdateColumnValue();
    }

    int GetAvailableSlot()
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

    void PlaceInSlot(Udon_Dice dice, int slot)
    {
        //--tell the game manager what dice value we got and where we placed it
        playerData.gameManager.latestDiceValue = dice.diceValue;
        playerData.gameManager.latestColumnUsed = columnId;

        if(slot == 0)
        {
            dice1 = dice;
            dice.transform.position = slot1.transform.position;

            playerData.DicePlaced();


            return;
        }
        else if (slot == 1)
        {
            dice2 = dice;
            dice.transform.position = slot2.transform.position;

            playerData.DicePlaced();
            return;
        }
        else if(slot == 2)
        {
            dice3 = dice;
            dice.transform.position = slot3.transform.position;

            playerData.DicePlaced();
            return;
        }

    }

    public void RemoveDicesWithValue(int value)
    {
        if (dice1 != null) 
        {
            if(dice1.diceValue == value)
            {
                dice1.parentGameobject.SetActive(false); 
                dice1 = null;
            }
        }
        if (dice2 != null)
        {
            if (dice2.diceValue == value)
            {
                dice2.parentGameobject.SetActive(false);
                dice2 = null;
            }
        }
        if (dice3 != null)
        {
            if (dice3.diceValue == value)
            {
                dice3.parentGameobject.SetActive(false);
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
    
}
