using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Wrapper.Modules;

public class Udon_NimbatBase : UdonSharpBehaviour
{
    public bool isMine
    {
        get
        {
            if (Networking.GetOwner(gameObject) == Networking.LocalPlayer)
            {           
                return true;
            }
      
            return false;
        }
    }
}
