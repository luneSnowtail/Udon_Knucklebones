
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

public class Udon_DiceDebug : UdonSharpBehaviour
{
    public Udon_Dice dice;
    
    public TextMeshPro textMeshPro;


    private void Update()
    {
        transform.position = dice.transform.position + (Vector3.up * .2f);
        textMeshPro.text = dice.diceValue.ToString();

    }
}
