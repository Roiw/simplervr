using System.Collections;
using System.Collections.Generic;
using SimplerVR.PlatformInterfaces;
using UnityEngine;

namespace SimplerVR.Core.Controller.Hint
{
    /// <summary>
    /// This class will be responsible for managing hints, displayed on the controller.
    /// </summary>
    public class HintManager : MonoBehaviour
    {
        public static HintManager Instance;

        public enum HandNumber {HandOne, HandTwo}

        // Must contain Left or Right to determine which hand to display the hint.
        public enum ButtonID {Undefined, LeftTrigger, RightTrigger, RightGrip, LeftGrip, RightTouchpad, LeftTouchpad, RightOptions, LeftOptions}

        [Tooltip("The general prefab for all the hints.")]
        public GameObject HintPrefab;

        /* This list is here and not on the hint class, so we don't have several instances of this list instantiated (One for each hint). */
        private List<Hint.HintHook> hintHooks;
        
        /* The list of hints being displayed. */
        private List<HintRankPair> activeHints;

        /* A reference to hand 2. */
        private GameObject leftHand;

        /* A reference to hand 2. */
        private GameObject rightHand;

        /// <summary>
        /// A reference to the settings of the core of the API.
        /// </summary>
        private CoreSettings coreSettingsAsset;
        private CoreSettings coreSettings
        {
            get
            {
                if (coreSettingsAsset != null)
                    return coreSettingsAsset;

                coreSettingsAsset = CoreSettings.LoadCoreSettings();
                return coreSettingsAsset;
            }
        }

        // Relates a hint with a rank so when there are two hints to be displayed on the same button only the
        // one with higher rank will display.
        private class HintRankPair
        {
            public int rank;
            public Hint hint;
            public HintRankPair (Hint h)
            {
                hint = h;
            }
        }

        void Awake()
        {
            // Singleton
            if (Instance != null)
            {
                Destroy(this.gameObject);
                Destroy(this);
                return;
            }

            Instance = this;
            if (activeHints == null)
                activeHints = new List<HintRankPair>();
        }

        void Update()
        {
            if (!coreSettings.CurrentPlatform.GetType().IsSubclassOf(typeof(GenericControllerPlatform)))
                return;

            // Get left and right controller..
            rightHand = ControllerManager.Instance.GetRightController();
            leftHand = ControllerManager.Instance.GetLeftController();

            HintHandCheck();

            if (coreSettings.UseDefaultHints)
            {
                for (int i = 0; i < coreSettings.DefaultHintsContent.Count; i++)
                {
                    // Check if we are not spawning it twice.
                    List<HintRankPair> hintsOnTheSameButton = activeHints.FindAll(h => h.hint.HintButton.Equals(coreSettings.DefaultHintButtons[i]));
                    if (hintsOnTheSameButton.Count == 0 )
                        DisplayHint(coreSettings.DefaultHintsContent[i], HintConvert.StringToButtonID(coreSettings.DefaultHintButtons[i]));
                }
            }
        }

        /// <summary>
        /// Get the list which has the position of all hints in the controller.
        /// </summary>
        /// <returns>A list of hint hook.</returns>
        public List<Hint.HintHook> GetHintHooks()
        {
            return hintHooks;
        }

        /// <summary>
        /// Set the list which has the position of all hints in the controller.
        /// </summary>
        /// <param name="list">A list of hint hook to be set.</param>
        public void SetHintHooks(List<Hint.HintHook> list)
        {
            hintHooks = list;
        }

        /// <summary>
        /// Display a hint.
        /// </summary>
        /// <param name="text">Text to be displayed/</param>
        /// <param name="button">A ButtonID to display the hint.</param>
        public void DisplayHint(string text, ButtonID button)
        {
            if (activeHints == null)
                activeHints = new List<HintRankPair>();

            GameObject newHint = GameObject.Instantiate(HintPrefab);
            newHint.name = button.ToString() + " Hint";
            
            Hint hint = newHint.GetComponent<Hint>();
            hint.SetText(text);
            hint.HintButton = button;
            /* Check if the hint hooks was loaded before. If not load it now. */
            if (hintHooks == null)
                hint.LoadHintHooks();

            /* This hint should be on the right hand. */
            if (button.ToString().Contains("Right"))    
                newHint.transform.SetParent(ControllerManager.Instance.GetControllerAttachPosition(true).transform);
            else if (button.ToString().Contains("Left"))
                newHint.transform.SetParent(ControllerManager.Instance.GetControllerAttachPosition(false).transform);
            else
                Debug.LogError("HintManager: Something is wrong, can't figure out which hand to place hint.");
                
            /* Position the hint */
            hint.SetPosition(hintHooks.Find(h => h.Button.Equals(button)).HintPosition);

            /* Draw the line linking hint and controller */
            hint.DrawLine(hintHooks.Find(h => h.Button.Equals(button)).LinePosition);

            List<HintRankPair> hintsOnTheSameButton = activeHints.FindAll(h => h.hint.HintButton.Equals(button));
            HintRankPair rankPair = new HintRankPair(hint);
            
            rankPair.rank = hintsOnTheSameButton.Count + 1;

            if (hintsOnTheSameButton.Count == 0)
            {
                /* Add hint to actives*/
                activeHints.Add(rankPair);
            }
            else
            {
                /* Protect the activeHints from spamming. (If a rogue programmer misuse this method). */
                if (hintsOnTheSameButton.Find(h => h.hint.GetText().Equals(text)) != null)
                {
                    Destroy(newHint);
                    return;
                }
                
                /* Disable the active hint with rank below and add new hint. */
                HintRankPair hintToDisable;
                hintToDisable = hintsOnTheSameButton.Find(h => h.rank == hintsOnTheSameButton.Count);
                hintToDisable.hint.gameObject.SetActive(false);
                activeHints.Add(rankPair);
            }
        }


        /// <summary>
        /// Hide a hint on the right or left hand.
        /// </summary>
        /// <param name="button">The ButtonID of the hint to be disabled.</param>
        public void HideHint(ButtonID button)
        {
            if (activeHints == null)
                return;

            List <HintRankPair> hintsActive = activeHints.FindAll(h => h.hint.HintButton.Equals(button));

            if (hintsActive.Count == 0)
                return;
            else
            {
                // Removes highest rank hint from active list
                HintRankPair hintHighestRank = hintsActive.Find(h => h.rank == hintsActive.Count);
                activeHints.Remove(hintHighestRank);
                GameObject.Destroy(hintHighestRank.hint.gameObject);

                // Displays highest rank hint
                hintsActive.Remove(hintHighestRank);
                hintHighestRank = hintsActive.Find(h => h.rank == hintsActive.Count);
                if (hintHighestRank == null)
                {
                    // No second highest rank, return
                    return;
                }
                else
                {
                    // Sets second highest hint to display
                    hintHighestRank.hint.gameObject.SetActive(true);
                }
            }         
        }

        // Switches all hints from left hand to right hand and vice-versa
        private void HintHandCheck()
        {
            if (activeHints == null)
                return;

           // Finds list of hints for each hand
            List<HintRankPair> leftHandHints = activeHints.FindAll(h => h.hint.HintButton.ToString().Contains("Left"));
            List<HintRankPair> rightHandHints = activeHints.FindAll(h => h.hint.HintButton.ToString().Contains("Right"));

            // Gets parent object
            GameObject rightTip = ControllerManager.Instance.GetControllerAttachPosition(true);
            GameObject leftTip = ControllerManager.Instance.GetControllerAttachPosition(false);

            if (leftHandHints.Count > 0)
            {
                // For each hint in left hand, switch
                foreach (HintRankPair pairy in leftHandHints)
                {
                    Vector3 pos = pairy.hint.transform.localPosition;
                    Quaternion rot = pairy.hint.transform.localRotation;
                    pairy.hint.transform.SetParent(rightTip.transform);
                    pairy.hint.transform.localPosition = pos;
                    pairy.hint.transform.localRotation = rot;
                }
            }
            if (rightHandHints.Count > 0)
            {
                // For each hint in right hand, switch
                foreach (HintRankPair pairy in rightHandHints)
                {
                    Vector3 pos = pairy.hint.transform.localPosition;
                    Quaternion rot = pairy.hint.transform.localRotation;
                    pairy.hint.transform.SetParent(leftTip.transform);
                    pairy.hint.transform.localPosition = pos;
                    pairy.hint.transform.localRotation = rot;
                }
            }
        }

        /// <summary>
        /// Runs whenever a new level is loaded.
        /// </summary>
        void OnLevelWasLoaded()
        {
            if (activeHints != null)
                activeHints.Clear();
        }

    }
}

