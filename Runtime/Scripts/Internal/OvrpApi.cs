// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System.Runtime.InteropServices;

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

        #region OVRP 1.21.0

        /// <summary>
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        [DllImport(_PLUGIN_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern OVRPlugin.Result ovrp_GetTiledMultiResLevel(out OVRPlugin.FoveatedRenderingLevel level);

        /// <summary>
        /// タイルベース (Fixed) な Forveated Rendering を設定する
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        [DllImport(_PLUGIN_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern OVRPlugin.Result ovrp_SetTiledMultiResLevel(OVRPlugin.FoveatedRenderingLevel level);

        #endregion

        #region OVRP 1.78.0

        /// <summary>
        /// </summary>
        /// <param name="controllerMask"></param>
        /// <param name="controllerState"></param>
        /// <returns></returns>
        [DllImport(_PLUGIN_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern OVRPlugin.Result ovrp_GetControllerState5(
            uint controllerMask,
            ref OVRPlugin.ControllerState5 controllerState
        );

        [DllImport(_PLUGIN_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern OVRPlugin.Result ovrp_SetControllerLocalizedVibration(
            OVRPlugin.Controller controllerMask,
            OVRPlugin.HapticsLocation hapticsLocationMask,
            float frequency,
            float amplitude
        );

        #endregion
    }
}