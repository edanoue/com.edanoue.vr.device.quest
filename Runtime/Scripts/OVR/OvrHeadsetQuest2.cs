// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using Edanoue.VR.Device.Core;
using Edanoue.VR.Device.Quest.Internal;
using UnityEngine.Device;

namespace Edanoue.VR.Device.Quest
{
    /// <summary>
    ///     Meta Quest 2 の Headset の実装
    /// </summary>
    public class OvrHeadsetQuest2 : IHeadset, ISupportedBattery, IUpdatable
    {
        private Action? _establishedConnectionDelegate;
        private bool    _isConnected;

        private bool    _isMounted;
        private Action? _lostConnectionDelegate;
        private Action? _mountedDelegate;
        private OVRPose _pose;
        private Action? _unmountedDelegate;
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

        (float W, float X, float Y, float Z) ITracker.Rotation
        {
            get
            {
                var o = _pose.orientation;
                return (o.w, o.x, o.y, o.z);
            }
        }

        bool IHeadset.IsMounted => _isMounted;

        event Action? IHeadset.Mounted
        {
            add => _mountedDelegate += value;
            remove => _mountedDelegate -= value;
        }

        event Action? IHeadset.Unmounted
        {
            add => _unmountedDelegate += value;
            remove => _unmountedDelegate -= value;
        }

        float ISupportedBattery.Battery =>
            // Use Unity methods (range: [0, 1])
            SystemInfo.batteryLevel;

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

            // Headset mounted check
            tmpBool = OVRPlugin.userPresent;
            if (_isMounted != tmpBool)
            {
                _isMounted = tmpBool;
                if (_isMounted)
                {
                    _mountedDelegate?.Invoke();
                }
                else
                {
                    _unmountedDelegate?.Invoke();
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