using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimplerVR.Core.Controller;
using static SimplerVR.Core.Controller.Button;

namespace SimplerVR.PlatformInterfaces.VIVE
{
    public static class VIVEButtons
    {
        /// <summary>
        /// The buttons available on a VIVE Controller.
        /// </summary>
        public static Button.ButtonName[] buttons = 
        {
                Button.ButtonName.ApplicationMenu,
                Button.ButtonName.Grip,
                Button.ButtonName.Touchpad,
                Button.ButtonName.Trigger
        };
    }
}
