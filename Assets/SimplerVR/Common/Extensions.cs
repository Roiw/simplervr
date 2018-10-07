using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SimplerVR.Common
{
    public static class Extensions
    {
        // Increase array size
        public static T[] ExpandArray<T>(this Array array,T[] originalArray, int amountToExpand)
        {
            // Create a new array of type T with the new combined size.
            T[] returnArray = new T[originalArray.Length + amountToExpand];
            for (int i = 0; i < originalArray.Length; i++)
            {
                returnArray[i] = originalArray[i];
            }
            return returnArray;
        }

        // Decrease array size
        public static T[] DescreaseArray<T>(this Array array, T[] originalArray, int amountToDecrease)
        {
            // Create a new array of type T with the new combined size.
            T[] returnArray = new T[originalArray.Length - amountToDecrease];
            for (int i = 0; i < returnArray.Length; i++)
            {
                returnArray[i] = originalArray[i];
            }
            return returnArray;
        }
    }
}
