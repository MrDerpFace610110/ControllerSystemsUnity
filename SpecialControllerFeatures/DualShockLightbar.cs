using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

public static class DualShockLightbar
{
    public static void ChangeLighbarColor(Gamepad gamepad, Color color)
    {
        DualShockGamepad dsGamepad = gamepad as DualShockGamepad;
        if(dsGamepad == null) return;

        dsGamepad.SetLightBarColor(color);
    }
}