#nullable enable
using System.Runtime.InteropServices;
using Edanoue.VR.Device.Core;

namespace Edanoue.VR.Device.Quest
{
    internal static class OVRPluginAPI
    {
        private const string pluginName = "OVRPlugin";

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern OVRPlugin.PoseStatef ovrp_GetNodePoseState(OVRPlugin.Step stepId, OVRPlugin.Node nodeId);
    }

    public class QuestOVRHeadset : IHeadset, IUpdatable
    {
        private OVRPose _pose;
        bool ITracker.IsConnected => true;

        (float X, float Y, float Z) ITracker.Position
        {
            get
            {
                var p = _pose.position;
                return (p.x, p.y, p.z);
            }
        }

        (float W, float X, float Y, float Z) ITracker.Rotation
        {
            get
            {
                var o = _pose.orientation;
                return (o.w, o.x, o.y, o.z);
            }
        }

        string IHeadset.ProductName => "Quest";

        void IUpdatable.Update(float deltaTime)
        {
            // Use OVRP method
            // Ref: OVRCommon.GetNodeStatePropertyVector3 (NodeStatePropertyType: Position)
            // ref: https://scrapbox.io/edanoue/Oculus_Integration_における_XrApi_と_XrDevice_の違い
            if (OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus)
            {
                var ovrNodeId = OVRPlugin.Node.EyeCenter;
                // version >= OVRP_1_12_0
                _pose = OVRPluginAPI.ovrp_GetNodePoseState(OVRPlugin.Step.Render, ovrNodeId).Pose.ToOVRPose();
            }
        }
    }
}