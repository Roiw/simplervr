using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SimplerVR.Core.Controller.Button;
using static SimplerVR.Core.Controller.Hint.HintManager;

namespace SimplerVR.Core.Controller.Hint
{
    public static class HintConvert
    {
        /// <summary>
        /// Converts a ButtonName to a String
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isRight"></param>
        /// <returns></returns>
        public static string ConvertNames(Button.ButtonName name, bool isRight)
        {
            if (isRight)
            {
                if (name == Button.ButtonName.ApplicationMenu)
                    return "RightOptions";
                if (name == Button.ButtonName.Grip)
                    return "RightGrip";
                if (name == Button.ButtonName.Touchpad)
                    return "RightTouchpad";
                if (name == Button.ButtonName.Trigger)
                    return "RightTrigger";
            }
            else
            {
                if (name == Button.ButtonName.ApplicationMenu)
                    return "LeftOptions";
                if (name == Button.ButtonName.Grip)
                    return "LeftGrip";
                if (name == Button.ButtonName.Touchpad)
                    return "LeftTouchpad";
                if (name == Button.ButtonName.Trigger)
                    return "LeftTrigger";
            }
            
            return "";
        }

        /// <summary>
        /// Converts a String to a ButtonName
        /// </summary>
        /// <param name="button"></param>
        /// <returns>A ButtonName</returns>
        public static Button.ButtonName ConvertToButtonName(string button)
        {
            if (button == "RightOptions")
                return Button.ButtonName.ApplicationMenu;
            if (button == "RightGrip")
                return Button.ButtonName.Grip;
            if (button == "RightTouchpad")
                return Button.ButtonName.Touchpad;
            if (button == "RightTrigger")
                return Button.ButtonName.Trigger;
            if (button == "LeftOptions")
                return Button.ButtonName.ApplicationMenu;
            if (button == "LeftGrip")
                return Button.ButtonName.Grip;
            if (button == "LeftTouchpad")
                return Button.ButtonName.Touchpad;
            if (button == "LeftTrigger")
                return Button.ButtonName.Trigger;

            return Button.ButtonName.Trigger;
        }

        /// <summary>
        /// Converts a ButtonName to a ButtonID
        /// </summary>
        /// <param name="name">A ButtonName</param>
        /// <param name="isRight"></param>
        /// <returns>A ButtonID, undefined if can't find it.</returns>
        public static HintManager.ButtonID ConverNames(Button.ButtonName name, bool isRight)
        {
            if (isRight)
            {
                if (name == Button.ButtonName.ApplicationMenu)
                    return HintManager.ButtonID.RightOptions;
                if (name == Button.ButtonName.Grip)
                    return HintManager.ButtonID.RightGrip;
                if (name == Button.ButtonName.Touchpad)
                    return HintManager.ButtonID.RightTouchpad;
                if (name == Button.ButtonName.Trigger)
                    return HintManager.ButtonID.RightTrigger;
            }
            else
            {
                if (name == Button.ButtonName.ApplicationMenu)
                    return HintManager.ButtonID.LeftOptions;
                if (name == Button.ButtonName.Grip)
                    return HintManager.ButtonID.LeftGrip;
                if (name == Button.ButtonName.Touchpad)
                    return HintManager.ButtonID.LeftTouchpad;
                if (name == Button.ButtonName.Trigger)
                    return HintManager.ButtonID.LeftTrigger;
            }

            return HintManager.ButtonID.Undefined;
        }

        /// <summary>
        /// Converts a string to a ButtonID
        /// </summary>
        /// <param name="button">An input string.</param>
        /// <returns>A ButtonID, undefined if can't find it.</returns>
        public static HintManager.ButtonID StringToButtonID(string button)
        {
            if (button == "RightOptions")
                return HintManager.ButtonID.RightOptions;
            if (button == "RightGrip")
                return HintManager.ButtonID.RightGrip;
            if (button == "RightTouchpad")
                return HintManager.ButtonID.RightTouchpad;
            if (button == "RightTrigger")
                return HintManager.ButtonID.RightTrigger;
            if (button == "LeftOptions")
                return HintManager.ButtonID.LeftOptions;
            if (button == "LeftGrip")
                return HintManager.ButtonID.LeftGrip;
            if (button == "LeftTouchpad")
                return HintManager.ButtonID.LeftTouchpad;
            if (button == "LeftTrigger")
                return HintManager.ButtonID.LeftTrigger;

            return HintManager.ButtonID.Undefined;
        }
    }
}
