using FbF;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    [Serializable]
    public class DamageFallOff
    {
        public float damageMinDist = 10;
        public float damageMaxDist = 3;
        public float minDist = 2;
        public float maxDist = 8;

        public int CalculateDamage(float distanceToTarget)
        {
            float clampedDist = Mathf.Clamp(distanceToTarget, minDist, maxDist);
            return Mathf.RoundToInt(Mathf.Lerp(damageMinDist, damageMaxDist, (clampedDist - minDist) / (maxDist - minDist)));
        }

        public void RecordProperties(EventData eventData)
        {
            eventData.AddProperty("Damage Min Distance", damageMinDist);
            eventData.AddProperty("Damage Min Distance", damageMinDist);
            eventData.AddProperty("Min Distance", minDist);
            eventData.AddProperty("Max Distance", maxDist);
        }
    }
}