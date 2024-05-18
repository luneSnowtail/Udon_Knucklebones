
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Wrapper.Modules;

public class Udon_KB_Button : UdonSharpBehaviour
{
    public Udon_KB_GameManager gameManager;
    public int buttonType = 0;    

    public Udon_KB_PlayerData targetPlayerData;

    public override void Interact()
    {
        Debug.Log("i was clicked");

        switch (buttonType)
        {
            case 0:                
                gameManager.StartGame();
                break;
            case 1:                
                //--button to reset the game
                gameManager.ResetTableLocal();
                break;

            case 2:
                //request ownership for player 1
                //player 1 also owns the game manager
                gameManager.RequestLocalOwnership();
                targetPlayerData.RequestLocalOwnership();
                break;
            case 3:
                //request ownership for player 2
                targetPlayerData.RequestLocalOwnership();
                break;
            case 4:
                //leave the game


                break;
        }
    }

    public void RequestLocalOwnership()
    {
        Debug.Log("requesting ownership");


        if (Networking.GetOwner(gameObject) == Networking.LocalPlayer)
        {
            Debug.Log("you already own" + gameObject.name + " object");                        
        }
        else
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        }

    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        Debug.Log("ownership transfered");        
    }



}
