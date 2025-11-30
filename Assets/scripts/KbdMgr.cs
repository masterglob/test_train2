
using UnityEngine;
using UnityEngine.InputSystem;

public class KbdMgr : MonoBehaviour
{
    public LocomotiveMover loco;
    public IntersectionsMgr swMgr;

    [SerializeField] private float power = 1.0f;

    private void FixedUpdate()
    {
        if (loco == null) return;
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
        if (keyboard.zKey.isPressed) // Q on AZERTY!
        {
            // TODO direct switch!
        }
        if (keyboard.dKey.isPressed)
        {
            // TODO alternate switch!
        }
    }
}
