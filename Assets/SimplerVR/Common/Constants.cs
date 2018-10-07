using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimplerVR.Common
{
    /// <summary>
    /// <para>All constants are set here.</para>
    /// <para>Initialization variables are set here too for the Core Settings.</para>
    /// </summary>
    public static class Constants
    {
        public static class API
        {
            /// <summary>
            /// API Name.
            /// </summary>
            public const string APIName = "SimplerVR";

            /// <summary>
            /// Name of the root of the API game object.
            /// </summary>
            public const string RootObjectName = "[SimplerVR]";

            /// <summary>
            /// Location of the root of the project.
            /// </summary>
            public const string ProjectRoot = "Assets/SimplerVR";

            /// <summary>
            /// Name of the Features game object.
            /// </summary>
            public const string RootFeatureObjectName = "Features";

            /// <summary>
            /// Name of the Platforms game object.
            /// </summary>
            public const string RootPlatformObjectName = "Platforms";

            public const string InteractionTagName = "VR_Interaction";

            public static class Core
            {
                /// <summary>
                /// Name of the Core game object.
                /// </summary>
                public const string RootObjectName = "Core";

                /// <summary>
                /// Where to store the Data file for the CoreSettings
                /// </summary>
                public const string DataLocation = ProjectRoot + "/Core/Resources/Core/Data/CoreSettings.asset";

                /// <summary>
                /// Where the configuration file for the core is on the resources folder. Used on execution time.
                /// </summary>
                public const string ResourceLocation = "Core/Data/CoreSettings";

                public const bool UseDefaultHints = false;

                #region Laser Settings Variables
                public const float LaserLenght = 4;

                public const bool LaserOnLeftHand = false;

                public const bool UseLaserInteraction = true;

                public const float LaserCollisionScale = 0.01f;

                public const string LaserCollisionSpriteLocation = "Core/LaserCollisionCircle";

                public const string LaserMaterialLocation = "Core/Laser";

                public const string LaserCollisionMaterialLocation = "Core/LaserCollisionMaterial";
                #endregion
            }       
        }      
    }
}
