
using UnityEngine;
using UnityEngine.InputSystem;

public class KbdMgr : MonoBehaviour
{
    public LocomotiveMover loco1;
    public LocomotiveMover loco2;
    public IntersectionsMgr interMgr = null;

    [SerializeField] private float power = 1.0f;

    void Start()
    {
    }

    private void FixedUpdate()
    {
        var keyboard = Keyboard.current;

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
}
