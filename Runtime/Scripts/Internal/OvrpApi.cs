// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System.Runtime.InteropServices;

namespace Edanoue.VR.Device.Quest.Internal
{
    /// <summary>
    ///     OVRP (OVRPlugin) C# API class
    /// </summary>
    internal static class OvrpApi
    {
        private const string PluginName = "OVRPlugin";

        /// <summary>
        /// </summary>
        /// <param name="stepId"></param>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        [DllImport(PluginName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern OVRPlugin.PoseStatef ovrp_GetNodePoseState(OVRPlugin.Step stepId, OVRPlugin.Node nodeId);
    }
}