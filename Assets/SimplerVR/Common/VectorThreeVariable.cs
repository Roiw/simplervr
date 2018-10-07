using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplerVR.Common
{
    /// <summary>
    /// This is a shareable Vector3 variable.
    /// </summary>
    public class VectorThreeVariable : ScriptableObject
    {
        public float Initial_X;
        public float Initial_Y;
        public float Initial_Z;

        [NonSerialized]
        public float Runtime_X;
        [NonSerialized]
        public float Runtime_Y;
        [NonSerialized]
        public float Runtime_Z;

        public void OnAfterDeserialize()
        {
            Runtime_X = Initial_X;
            Runtime_Y = Initial_Y;
            Runtime_Z = Initial_Z;
        }

        public void SerializeCurrentValues()
        {
            Initial_X = Runtime_X;
            Initial_Y = Runtime_Y;
            Initial_Z = Runtime_Z;
        }
    }
}
