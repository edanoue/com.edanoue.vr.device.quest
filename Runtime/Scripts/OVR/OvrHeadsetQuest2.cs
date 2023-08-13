// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using Edanoue.VR.Device.Core;
using Edanoue.VR.Device.Quest.Internal;
using UnityEngine.Device;

namespace Edanoue.VR.Device.Quest
{
    internal sealed class QuestHeadsetDisplayColorHandler : HeadsetDisplayColorHandler
    {
        public override void SetColorScale(float x, float y, float z, float w)
        {
            OculusXRPlugin.SetColorScale(x, y, z, w);
        }

        public override void SetColorOffset(float x, float y, float z, float w)
        {
            OculusXRPlugin.SetColorOffset(x, y, z, w);
        }

        public override void ResetColorScale()
        {
            OculusXRPlugin.SetColorScale(1, 1, 1, 1);
        }

        public override void ResetColorOffset()
        {
            OculusXRPlugin.SetColorOffset(0, 0, 0, 0);
        }
    }

    /// <summary>
    /// Meta Quest 2 の Headset の実装
    /// </summary>
    public class OvrHeadsetQuest2 : IHeadset, ISupportedBattery
    {
        private Action? _establishedConnectionDelegate;

        private bool _isConnected;

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

        FoveatedRenderingLevel IHeadset.FoveatedRenderingLevel
        {
            get
            {
                OvrpApi.ovrp_GetTiledMultiResLevel(out var level);
                return (FoveatedRenderingLevel)level;
            }
            set => OvrpApi.ovrp_SetTiledMultiResLevel((OVRPlugin.FoveatedRenderingLevel)value);
        }

        bool IHeadset.UseDynamicFoveatedRendering
        {
            get
            {
                OvrpApi.ovrp_GetTiledMultiResDynamic(out var isDynamic);
                return isDynamic != OvrpApi.Bool.False;
            }
            set => OvrpApi.ovrp_SetTiledMultiResDynamic(value ? OvrpApi.Bool.True : OvrpApi.Bool.False);
        }

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

        public HeadsetDisplayColorHandler DisplayColor { get; } = new QuestHeadsetDisplayColorHandler();

        float ISupportedBattery.Battery =>
            // Use Unity methods (range: [0, 1])
            SystemInfo.batteryLevel;

        internal void Update()
        {
            // -----------------
            // Connection check
            // -----------------
            
            var tmpBool = OVRPlugin.hmdPresent;
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
            // Ref: https://scrapbox.io/edanoue/Oculus_Integration_%E3%81%AB%E3%81%8A%E3%81%91%E3%82%8B_XrApi_%E3%81%A8_XrDevice_%E3%81%AE%E9%81%95%E3%81%84
            // --------------------------------------
            if (OVRPlugin.initialized)
            {
                const OVRPlugin.Node ovrNodeId = OVRPlugin.Node.EyeCenter;
                // version >= OVRP_1_12_0
                _pose = OvrpApi.ovrp_GetNodePoseState(OVRPlugin.Step.Render, ovrNodeId).Pose.ToOVRPose();
            }
        }
    }
}