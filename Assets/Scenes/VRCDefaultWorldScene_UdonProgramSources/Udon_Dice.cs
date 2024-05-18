
using Newtonsoft.Json.Linq;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using static Udon_Dice;

[UdonBehaviourSyncMode( BehaviourSyncMode.Continuous)]
public class Udon_Dice : Udon_NimbatBase
{
    [UdonSynced]
    public int diceValue;

    //--references
    public GameObject parentGameobject;
    public Udon_KB_PlayerData playerData;

    public Material debugMatWhite;
    public Material debugMatRed;
    public Material debugMatGreen;
    public Material debugMatYellow;

    public MeshRenderer meshRenderer;
    public Rigidbody rbody;

    public int diceState = 0;       //0 = none, 1= grabbed, 2=release, 3=result, 4=postGrab, 5=postRelease, 6= dice Already used
    int diceVelocityState = 0;      //0 = it has not reached minimum velocity, 1= it reached minimum velocity, waiting for it to stop


    Ray diceRay;
    RaycastHit diceRayHit;
    public LayerMask diceRayLayerMask;
    Udon_DiceFace diceFaceHit;
    public VRCPickup vrcPickup;
    public Collider diceCollider;

    Udon_KB_Column kbColumn;

    public int columnId;
    public int slotId;

    public void RequestLocalAuthority()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
    }

    private void Start()
    {
        diceRay = new Ray();
        diceRay.direction = Vector3.up;
        diceRayHit = new RaycastHit();
    }

    public override void OnPickup()
    {
        if(diceState == 0)
        {
            SetDiceState(1);
            diceValue = 0;
        }

        if(diceState == 3 || diceState == 5)
        {
            SetDiceState(4);
        }
    }
    public override void OnDrop()
    {
        if(diceState == 1)
        {
            Vector3 randomRotation = new Vector3(Random.Range(0,360), Random.Range(0, 360), Random.Range(0, 360));
            transform.rotation *= Quaternion.Euler(randomRotation);
            SetDiceState(2);
            diceVelocityState = 0;
        }

        if(diceState == 3 || diceState == 4 ) 
        {
            SetDiceState(5);
        }
    }

    private void OnEnable()
    {
        if (isMine)
        {
            diceValue = 0;
            diceCollider.enabled = true; 
            SetDiceState(0);

        }
        else
        {
            diceCollider.enabled = false;
        }

        RequestSerialization();
    }

    private void Update()
    {
        //--if i dont own the dice, do nothing
        if (!isMine)
            return;

        if(diceState == 2)
        {            
            switch (diceVelocityState)
            {
                case 0:
                    if (rbody.velocity.magnitude > 1f)
                        diceVelocityState = 1;
                    break;
                case 1:
                    if (rbody.velocity.magnitude < .01f)
                    {
                        playerData.SetLocalState(2);
                        SetDiceState(3);
                    }
                    break;
            }
        }        

        //--check for when the slot we were placed to become null to turn ourselves off
        if(diceState == 6)
        {
            switch(slotId)
            {
                case 0:
                    if (!playerData.columns[columnId].IsSlotUsed(0))
                    {
                        Debug.Log("my slot dissapeared");
                        parentGameobject.SetActive(false);
                    }
                    break;
                case 1:
                    if (!playerData.columns[columnId].IsSlotUsed(1))
                    {
                        Debug.Log("my slot dissapeared");
                        parentGameobject.SetActive(false);
                    }
                    break;
                case 2:
                    if (!playerData.columns[columnId].IsSlotUsed(2))
                    {
                        Debug.Log("my slot dissapeared");
                        parentGameobject.SetActive(false);
                    }
                    break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (diceState <= 4)
        {
            Debug.Log("dice not ready to be placed");
            return;
        }

        if(diceState == 6)
        {
            Debug.Log("dice aalready in score");
            return;
        }
           
        kbColumn = other.GetComponent<Udon_KB_Column>();

        columnId = kbColumn.columnId;
        slotId = kbColumn.GetAvailableSlot();

        Debug.Log("column id " + columnId.ToString() + " || slot id " + slotId.ToString()); 
        kbColumn.PlaceInSlot(this,slotId);   

        SetDiceState(6);

    }

    void GetDiceValue()
    {
        diceRay.origin = transform.position;
        if(Physics.Raycast(diceRay,out diceRayHit, .5f, diceRayLayerMask))
        {
            diceFaceHit = (Udon_DiceFace) diceRayHit.collider.GetComponent<Udon_DiceFace>();
            diceValue = diceFaceHit.faceValue;
        }
    }

    void SetDiceState(int newState)
    {
        diceState = newState;

        switch (newState)
        {
            case 0:                
                meshRenderer.material = debugMatWhite;
                break;
            case 1:                
                meshRenderer.material = debugMatGreen;
                break;
            case 2:                
                meshRenderer.material = debugMatRed;
                break;
            case 3:                
                meshRenderer.material = debugMatYellow;                
                GetDiceValue();
                break;
            case 6:

                diceCollider.enabled = false;
                rbody.velocity = Vector3.zero;
                rbody.angularVelocity = Vector3.zero;

                break;
        }
    }
}