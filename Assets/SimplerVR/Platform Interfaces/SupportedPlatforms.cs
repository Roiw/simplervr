using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimplerVR.Core.Controller;
using static SimplerVR.Core.Controller.Button;

namespace SimplerVR.PlatformInterfaces
{
    /// <summary>
    /// Add specific information for the supported platforms.
    /// </summary>
    [Serializable]
    public static class SupportedPlatforms
    {
        /// <summary>
        /// A list of all supported platforms by the CoreSettings.
        /// </summary>
        public enum Platforms { VIVE };

        /// <summary>
        /// Return the available buttons for an specific platform.
        /// </summary>
        /// <param name="platforms">The platform</param>
        /// <returns>available button list.</returns>
        public static Button.ButtonName[] GetButtons(Platforms platforms)
        {
            if (platforms == Platforms.VIVE)
            {
                return new Button.ButtonName[]
                {
                    Button.ButtonName.ApplicationMenu,
                    Button.ButtonName.Grip,
                    Button.ButtonName.Touchpad,
                    Button.ButtonName.Trigger
                };
            }
            else
                return null;
        }

        // <summary>
        /// Return the available buttons for an specific platform.
        /// </summary>
        /// <param name="platforms">The platform</param>
        /// <returns>available button list.</returns>
        public static Button.ButtonActions[] GetButtonActions(Platforms platforms)
        {
            if (platforms == Platforms.VIVE)
            {
                return new Button.ButtonActions[]
                {
                    Button.ButtonActions.PressUp,
                    Button.ButtonActions.HoldDown,
                };
            }
            else
                return null;
        }

        public static string[] GetPositions(Platforms platforms)
        {
            if (platforms == Platforms.VIVE)
            {
                return new string[] { "Left", "Right" };
            }
            else
                return null;
        }

    }
}
