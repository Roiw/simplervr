using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimplerVR.Core.Controller;

namespace SimplerVR.Core.Interaction
{
    public interface IActiveInteraction
    {
        /// <summary>
        /// Register all the buttons on the control set.
        /// </summary>
        /// <param name="userSelectedButtons">The ButtonNames</param>
        /// <param name="userSelectedActions">The ButtonActions</param>
        void RegisterButtons(Button.ButtonName[] userSelectedButtons, Button.ButtonActions[] userSelectedActions, bool[] positions);

        /// <summary>
        /// Unregister buttons from the ControlSet.
        /// </summary>
        void UnregisterButtons();

        /// <summary>
        /// Given a ButtonName and a ButtonAction return the list of actions to perform.
        /// </summary>
        /// <param name="name">The ButtonName for to return the selected actions.</param>
        /// <param name="action">The ButtonAction for to return the selected actions.</param>
        /// <returns>A list of actions.</returns>
        List<Action> GetButtonMethods(Button.ButtonName name, Button.ButtonActions action);
    }
}
