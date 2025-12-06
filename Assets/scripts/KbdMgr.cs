
using UnityEngine;
using UnityEngine.InputSystem;

public class KbdMgr : MonoBehaviour
{
    public LocomotiveMover loco1;
    public LocomotiveMover loco2;
    public IntersectionsMgr interMgr = null;

    public Camera camera1; 
    public Camera camera2;
    private bool isCamera1 = true;
    private bool isCPressed = false;

    [SerializeField] private float power = 1.0f;

    void Start()
    {

        if (camera1 != null && camera2 != null)
        { 
            camera1.gameObject.SetActive(true);
            camera2.gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        var keyboard = Keyboard.current;

        if (camera1 != null && camera2 != null)
        {
            if (keyboard.cKey.isPressed && !isCPressed)
            {
                SwitchCamera();
            }
            isCPressed = keyboard.cKey.isPressed;
        }

        if (loco1 != null)
        {
            if (keyboard.wKey.isPressed) // Z on AZERTY
            {
                loco1.Throttle(power);
            }

            if (keyboard.sKey.isPressed)
            {
                loco1.Throttle(-power);
            }
            // Freinage rapide!
            if (keyboard.spaceKey.isPressed)
            {
                loco1.Brake();
            }
        }

        if (loco2 != null)
        {
            if (keyboard.upArrowKey.isPressed)
            {
                loco2.Throttle(power);
            }
            if (keyboard.downArrowKey.isPressed)
            {
                loco2.Brake();
            }
        }

        // Change directions

        if (interMgr != null)
        {
            if (keyboard.aKey.isPressed) // Q on AZERTY!
            {
                if (interMgr != null)
                    interMgr.SetGlobalDirect(true);
            }
            if (keyboard.dKey.isPressed)
            {
                if (interMgr != null)
                    interMgr.SetGlobalDirect(false);
            }

            // ACK msg
            if (keyboard.qKey.isPressed) // A on AZERTY!
            {
                if (interMgr != null)
                    interMgr.ResetError();
            }
        }
    }

    private void SwitchCamera()
    {
        isCamera1 = !isCamera1;
        // Bascule entre les deux caméras
        if (!isCamera1)
        {
            camera1.gameObject.SetActive(false);
            camera2.gameObject.SetActive(true);
        }
        else
        {
            camera1.gameObject.SetActive(true);
            camera2.gameObject.SetActive(false);
        }
    }
}
