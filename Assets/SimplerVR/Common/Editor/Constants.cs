using System;
using System.Collections;
using System.Collections.Generic;
using SimplerVR.Core.Editor;
using SimplerVR.Features.ArcTeleport.Editor;
using UnityEngine;

namespace SimplerVR.Common.Editor
{
    public class Constants : MonoBehaviour
    {
        /// <summary>
        /// The icon to show on the window.
        /// </summary>
        public const string SmashIconFileName = "SmashIcon.png";

        /// <summary>
        /// The delete icon of the project.
        /// </summary>
        public const string DeleteIconFileName = "DeleteIcon.png";

        /// <summary>
        /// The window width.
        /// </summary>
        public const int WindowWidth = 420;

        /// <summary>
        /// The window height.
        /// </summary>
        public const int WindowMaxHeight = 800;

        /// <summary>
        /// The window height.
        /// </summary>
        public const int WindowMinHeight = 100;

        /// <summary>
        /// Name of the GUISkin filename (on the resources folder).
        /// </summary>
        public const string SkinFileName = "Skin";

        /// <summary>
        /// The name of the GUIStyle for displaying version.
        /// </summary>
        public const string VersionGUIStyle = "Version";

        public const string VersionPrefix = " [SimplerVR] v";

        /// <summary>
        /// The name of the GUIStyle for buttons.
        /// </summary>
        public const string BodyGUIStyle = "Body";

        /// <summary>
        /// The name of the GUIStyle for buttons.
        /// </summary>
        public const string ButtonLabelUIStyle = "Button Label";

        /// <summary>
        /// The name of the GUIStyle for buttons.
        /// </summary>
        public const string DeleteButtonIconUIStyle = "Delete Button";

        /// <summary>
        /// A GUIStyle for subtitles.
        /// </summary>
        public const string SubtitleGUIStyle = "SubTitle";

        /// <summary>
        /// A GUIStyle for second subtitles.
        /// </summary>
        public const string Subtitle2GUIStyle = "SubTitle 2";

        /// <summary>
        /// Known window types for this system.
        /// </summary>
        public static Type[] WindowTypes = { typeof(CoreSettingsWindow), typeof(ArcTeleportWindow) };

        public static class Core
        {
            /// <summary>
            /// Name of the Core Header PNG image (on the resources folder).
            /// </summary>
            public const string HeaderTextureFileName = "CoreHeaderBanner";

            /// <summary>
            /// The GUIStyle for the Feature Header.
            /// </summary>
            public const string HeaderGUIStyle = "Core Header";
        }

        public static class Features
        {
            /// <summary>
            /// Name of the Features Header PNG image (on the resources folder).
            /// </summary>
            public const string HeaderTextureFileName = "FeaturesHeaderBanner";

            /// <summary>
            /// The GUIStyle for the Feature Header.
            /// </summary>
            public const string HeaderGUIStyle = "Feature Headers";

            
        }
    }
}

