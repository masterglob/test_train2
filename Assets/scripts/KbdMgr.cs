
using UnityEngine;
using UnityEngine.InputSystem;

public class KbdMgr : MonoBehaviour
{
    public LocomotiveMover loco;
    public IntersectionsMgr interMgr = null;

    [SerializeField] private float power = 1.0f;

    void Start()
    {
    }

    private void FixedUpdate()
    {
        if (loco == null)
        {
            Debug.LogError("Assign a loco in KbdMgr.");
            return;
        }
        if (interMgr == null)
        {
            Debug.LogError("Assign a interMgr in KbdMgr.");
            return;
        }
        var keyboard = Keyboard.current;
        if (keyboard.wKey.isPressed) // Z on AZERTY
        {
            loco.Throttle(power);
        }

        if (keyboard.sKey.isPressed)
        {
            loco.Throttle(-power);
        }
        // Freinage rapide!
        if (keyboard.spaceKey.isPressed)
        {
            loco.Brake();
        }

        // Change directions
        if (keyboard.aKey.isPressed) // Q on AZERTY!
        {
            // TODO direct switch!
            if (interMgr != null)
                interMgr.SetGlobalDirect(true);
        }
        if (keyboard.dKey.isPressed)
        {
            // TODO alternate switch!
            if (interMgr != null)
                interMgr.SetGlobalDirect(false);
        }
    }
}
