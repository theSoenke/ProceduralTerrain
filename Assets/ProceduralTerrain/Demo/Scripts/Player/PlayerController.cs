using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerController : MonoBehaviour
{
    private RigidbodyFirstPersonController walkController;
    private FlyCamera flyController;
    private Rigidbody playerRigidbody;
    private HeadBob headbobController;

    private bool flyMode;
    private bool spawned = true;



    private void Start()
    {
        walkController = GetComponent<RigidbodyFirstPersonController>();
        headbobController = GetComponentInChildren<HeadBob>();
        flyController = GetComponent<FlyCamera>();
        playerRigidbody = GetComponent<Rigidbody>();

        //FreezePlayer();
        SetPlayerMode(false);
    }


    private void LateUpdate()
    {
        if (!spawned)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            flyMode = !flyMode;
            SetPlayerMode(flyMode);
        }

        if (flyMode)
        {
            playerRigidbody.velocity = Vector3.zero;
        }
    }


    public void FreezePlayer()
    {
        spawned = false;
        walkController.enabled = false;
        flyController.enabled = false;
        playerRigidbody.useGravity = false;
    }

    private void SetPlayerMode(bool fly)
    {
        if (fly)
        {
            flyController.enabled = true;
            walkController.enabled = false;
            headbobController.enabled = false;

            playerRigidbody.useGravity = false;
            playerRigidbody.drag = 0;
        }
        else
        {
            walkController.enabled = true;
            flyController.enabled = false;
            headbobController.enabled = true;

            playerRigidbody.useGravity = true;
            playerRigidbody.drag = 1;
        }
    }
}