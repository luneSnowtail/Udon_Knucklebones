
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Udon_InstanceOwnerCrown : UdonSharpBehaviour
{

    VRCPlayerApi instanceOwner;

    private void Update()
    {
        if (instanceOwner == null)
        {
            instanceOwner = Networking.GetOwner(gameObject);
            return;
        }

        transform.position = instanceOwner.GetBonePosition(HumanBodyBones.Head) + (Vector3.up * .3f);
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        instanceOwner = Networking.GetOwner(gameObject);        
    }
}
