#nullable enable
using System;
using Edanoue.VR.Device.Core;
using Edanoue.VR.Device.Quest.Internal;

namespace Edanoue.VR.Device.Quest
{
    public class QuestOVRHeadset : IHeadset, IUpdatable
    {
        private Action<float, float, float>? _changedPositionDelegate;
        private Action<float, float, float, float>? _changedRotationDelegate;

        private Action? _establishedConnectionDelegate;
        private bool _isConnected;
        private Action? _lostConnectionDelegate;
        private OVRPose _pose;
        bool ITracker.IsConnected => _isConnected;

        event Action? ITracker.EstablishedConnection
        {
            add => _establishedConnectionDelegate += value;
            remove => _establishedConnectionDelegate -= value;
        }

        event Action? ITracker.LostConnection
        {
            add => _lostConnectionDelegate += value;
            remove => _lostConnectionDelegate -= value;
        }

        (float X, float Y, float Z) ITracker.Position
        {
            get
            {
                var p = _pose.position;
                return (p.x, p.y, p.z);
            }
        }

        event Action<float, float, float>? ITracker.ChangedPosition
        {
            add => _changedPositionDelegate += value;
            remove => _changedPositionDelegate -= value;
        }

        (float W, float X, float Y, float Z) ITracker.Rotation
        {
            get
            {
                var o = _pose.orientation;
                return (o.w, o.x, o.y, o.z);
            }
        }

        event Action<float, float, float, float>? ITracker.ChangedRotation
        {
            add => _changedRotationDelegate += value;
            remove => _changedRotationDelegate -= value;
        }

        void IUpdatable.Update(float deltaTime)
        {
            var tmpBool = false;

            // -----------------
            // Connection check
            // -----------------
            tmpBool = OVRPlugin.hmdPresent;
            if (_isConnected != tmpBool)
            {
                _isConnected = tmpBool;
                if (_isConnected)
                {
                    _establishedConnectionDelegate?.Invoke();
                }
                else
                {
                    _lostConnectionDelegate?.Invoke();
                    return;
                }
            }

            // --------------------------------------
            // Cache pose (position and rotation)
            // Use OVRP method
            // Ref: OVRCommon.GetNodeStatePropertyVector3 (NodeStatePropertyType: Position)
            // Ref: https://scrapbox.io/edanoue/Oculus_Integration_における_XrApi_と_XrDevice_の違い
            // --------------------------------------
            if (OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus)
            {
                var ovrNodeId = OVRPlugin.Node.EyeCenter;
                // version >= OVRP_1_12_0
                _pose = OvrpApi.ovrp_GetNodePoseState(OVRPlugin.Step.Render, ovrNodeId).Pose.ToOVRPose();
            }
        }
    }
}