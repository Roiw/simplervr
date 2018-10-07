using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimplerVR.Core.Controller;

namespace SimplerVR.Features
{
    /// <summary>
    /// All features that use controllers must implement this interface.
    /// </summary>
    public interface IControllerFeature
    {
        /// <summary>
        ///  Register the buttons on the platform and on this feature serializeble registries.
        /// </summary>
        /// <param name="userSelectedButtons">The name of the selected buttons.</param>
        /// <param name="userSelectedActions">The actions of each button.</param>
        /// <param name="userSelectionIsRight">If each button is on the right hand side.</param>
        /// <param name="overrideInteraction">If each button should override interaction.</param>
        void RegisterActionButtons(Button.ButtonName[] controllerButtons, Button.ButtonActions[] buttonActions, bool[] isRight, bool[] overrideInteraction);

        /// <summary>
        /// Clear all the feature internal registries (those registries are used for serialization).
        /// </summary>
        void ClearButtonRegistries();

        /// <summary>
        /// Returns the list of button registry. This is used to deserialize button settings.
        /// </summary>
        /// <returns>A list of ButtonRegistry.</returns>
        List<ButtonRegistry> GetAllButtonRegistries();

        /// <summary>
        /// Returns the right method from a feature given a ButtonRegistry.
        /// </summary>
        /// <param name="buttonRegistry">A button registry.</param>
        /// <returns>A method</returns>
        Action GetButtonMethod(ButtonRegistry buttonRegistry);
    }
}
