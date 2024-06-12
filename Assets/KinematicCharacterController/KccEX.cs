using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public static class KccEX
{
    public static void RefreshLayer(this KinematicCharacterMotor motor, int layer)
    {
        if (motor == null)
            return;

        // Build CollidableLayers mask
        motor.CollidableLayers = 0;
        for (int i = 0; i < 32; i++)
        {
            if (!Physics.GetIgnoreLayerCollision(layer, i))
            {
                motor.CollidableLayers |= (1 << i);
            }
        }
    }
}
