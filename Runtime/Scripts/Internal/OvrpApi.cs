// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Edanoue.VR.Device.Quest.Internal
{
    /// <summary>
    /// OVRP (OVRPlugin) C# API class
    /// </summary>
    internal static class OvrpApi
    {
        private const string _PLUGIN_NAME = "OVRPlugin";


        #region OVRP 1.29.0

        /// <summary>
        /// </summary>
        /// <param name="stepId"></param>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        [DllImport(_PLUGIN_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern OVRPlugin.PoseStatef ovrp_GetNodePoseState(
            OVRPlugin.Step stepId,
            OVRPlugin.Node nodeId
        );

        #endregion

        internal enum Bool
        {
            False = 0,
            True
        }

        internal enum Result
        {
            /// Success
            Success = 0,
            Success_EventUnavailable = 1,
            Success_Pending          = 2,

            /// Failure
            Failure = -1000,
            Failure_InvalidParameter          = -1001,
            Failure_NotInitialized            = -1002,
            Failure_InvalidOperation          = -1003,
            Failure_Unsupported               = -1004,
            Failure_NotYetImplemented         = -1005,
            Failure_OperationFailed           = -1006,
            Failure_InsufficientSize          = -1007,
            Failure_DataIsInvalid             = -1008,
            Failure_DeprecatedOperation       = -1009,
            Failure_ErrorLimitReached         = -1010,
            Failure_ErrorInitializationFailed = -1011,

            /// Space error cases
            Failure_SpaceCloudStorageDisabled = -2000,
            Failure_SpaceMappingInsufficient  = -2001,
            Failure_SpaceLocalizationFailed   = -2002,
            Failure_SpaceNetworkTimeout       = -2003,
            Failure_SpaceNetworkRequestFailed = -2004
        }

        #region OVRP 1.21.0

        /// <summary>
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        [DllImport(_PLUGIN_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern Result ovrp_GetTiledMultiResLevel(out OVRPlugin.FoveatedRenderingLevel level);

        /// <summary>
        /// タイルベース (Fixed) な Forveated Rendering を設定する
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        [DllImport(_PLUGIN_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern Result ovrp_SetTiledMultiResLevel(OVRPlugin.FoveatedRenderingLevel level);

        #endregion

        #region OVRP 1.46.0

        [DllImport(_PLUGIN_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern Result ovrp_GetTiledMultiResDynamic(out Bool isDynamic);

        [DllImport(_PLUGIN_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern Result ovrp_SetTiledMultiResDynamic(Bool isDynamic);

        #endregion

        #region OVRP 1.78.0

        /// <summary>
        /// </summary>
        /// <param name="controllerMask"></param>
        /// <param name="controllerState"></param>
        /// <returns></returns>
        [DllImport(_PLUGIN_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern Result ovrp_GetControllerState5(
            uint controllerMask,
            ref OVRPlugin.ControllerState5 controllerState
        );

        [DllImport(_PLUGIN_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern Result ovrp_SetControllerLocalizedVibration(
            OVRPlugin.Controller controllerMask,
            OVRPlugin.HapticsLocation hapticsLocationMask,
            float frequency,
            float amplitude
        );

        #endregion
    }
}