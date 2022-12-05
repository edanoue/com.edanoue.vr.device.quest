#nullable enable

using System;
using Edanoue.VR.Device.Core;
using Edanoue.VR.Device.Quest.Internal;

namespace Edanoue.VR.Device.Quest
{
    /// <summary>
    ///     Oculus Quest Touch controller (OQ, OQ2 con)
    /// </summary>
    public class OvrControllerQuestTouch : IController, ISupportedVelocity, IVibration,
        IUpdatable
    {
        private readonly ControllerDomain _controllerDomain;
        private readonly ControllerInputData _controllerInputData;
        private OVRPlugin.PoseStatef _cachedPoseState;
        private Action<float, float>? _changedStickDelegate;
        private Action? _establishedConnectionDelegate;

        private bool _isConnected;

        private Action? _lostConnectionDelegate;
        private Action<bool>? _pressedPrimaryDelegate;
        private Action<bool>? _pressedSecondaryDelegate;
        private Action<bool>? _pressedStickDelegate;
        private Action<bool>? _touchedPrimaryDelegate;
        private Action<bool>? _touchedSecondaryDelegate;
        private Action<bool>? _touchedStickDelegate;

        internal OvrControllerQuestTouch(ControllerDomain controllerDomain)
        {
            _controllerDomain = controllerDomain;
            _controllerInputData = new ControllerInputData();
        }

        private OVRInput.Controller _ovrControllerMask
        {
            get
            {
                return _controllerDomain switch
                {
                    ControllerDomain.Left => OVRInput.Controller.LTouch,
                    ControllerDomain.Right => OVRInput.Controller.RTouch,
                    _ => OVRInput.Controller.None
                };
            }
        }

        ControllerDomain IController.Domain => _controllerDomain;
        bool IController.IsPressedPrimary => _controllerInputData.IsPressedPrimary;
        bool IController.IsTouchedPrimary => _controllerInputData.IsTouchedPrimary;

        event Action<bool>? IController.PressedPrimary
        {
            add => _pressedPrimaryDelegate += value;
            remove => _pressedPrimaryDelegate -= value;
        }

        event Action<bool>? IController.TouchedPrimary
        {
            add => _touchedPrimaryDelegate += value;
            remove => _touchedPrimaryDelegate -= value;
        }

        bool IController.IsPressedSecondary => _controllerInputData.IsPressedSecondary;
        bool IController.IsTouchedSecondary => _controllerInputData.IsTouchedSecondary;

        event Action<bool>? IController.PressedSecondary
        {
            add => _pressedSecondaryDelegate += value;
            remove => _pressedSecondaryDelegate -= value;
        }

        event Action<bool>? IController.TouchedSecondary
        {
            add => _touchedSecondaryDelegate += value;
            remove => _touchedSecondaryDelegate -= value;
        }

        bool IController.IsPressedStick => _controllerInputData.IsPressedStick;
        bool IController.IsTouchedStick => _controllerInputData.IsTouchedStick;

        (float X, float Y) IController.Stick => _controllerInputData.Stick;

        event Action<bool>? IController.PressedStick
        {
            add => _pressedStickDelegate += value;
            remove => _pressedPrimaryDelegate -= value;
        }

        event Action<bool>? IController.TouchedStick
        {
            add => _touchedStickDelegate += value;
            remove => _touchedStickDelegate -= value;
        }

        event Action<float, float>? IController.ChangedStick
        {
            add => _changedStickDelegate += value;
            remove => _changedStickDelegate -= value;
        }

        bool IController.IsTouchedThumbRest => _controllerInputData.IsTouchedThumbRest;

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
                var p = _cachedPoseState.Pose.ToOVRPose().position;
                return (p.x, p.y, p.z);
            }
        }

        (float W, float X, float Y, float Z) ITracker.Rotation
        {
            get
            {
                var o = _cachedPoseState.Pose.ToOVRPose().orientation;
                return (o.w, o.x, o.y, o.z);
            }
        }

        /*
        // 南: MQ2 の Touch Controller は取得できないっぽいです
        (float X, float Y, float Z) ISupportedAcceleration.LinearAcceleration
        {
            get
            {
                var p = _cachedPoseState.Acceleration.FromFlippedZVector3f();
                return (p.x, p.y, p.z);
            }
        }

        (float X, float Y, float Z) ISupportedAcceleration.AngularAcceleration
        {
            get
            {
                var p = _cachedPoseState.AngularAcceleration.FromFlippedZVector3f();
                return (p.x, p.y, p.z);
            }
        }
        */

        (float X, float Y, float Z) ISupportedVelocity.LinearVelocity
        {
            get
            {
                var p = _cachedPoseState.Velocity.FromFlippedZVector3f();
                return (p.x, p.y, p.z);
            }
        }

        (float X, float Y, float Z) ISupportedVelocity.AngularVelocity
        {
            get
            {
                var p = _cachedPoseState.AngularVelocity.FromFlippedZVector3f();
                return (p.x, p.y, p.z);
            }
        }

        /*
        // 南: MQ2 の Touch Controller は取得できない (常に 0) っぽいです
        // https://answers.unity.com/questions/1669595/get-oculus-riftrifts-controllers-battery-level.html
        float ISupport.Battery
        {
            get
            {
                // Range: [0, 100]
                byte nativeRemain= OVRInput.GetControllerBatteryPercentRemaining(_ovrControllerMask);
                // to [0.0, 1.0]
                float a = nativeRemain;
                return a / 100.0f;
            }
        }
        */

        void IUpdatable.Update(float deltaTime)
        {
            // --------------------------------------
            // Connection check
            // --------------------------------------
            var tmpBool = false;
            tmpBool = OVRInput.GetControllerPositionTracked(_ovrControllerMask);
            if (tmpBool != _isConnected)
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
            // Cache PoseStatef for position, rotation and velocity.
            // Use OVRP method
            // Ref: OVRInput.GetLocalControllerPosition
            // --------------------------------------
            _cachedPoseState = _controllerDomain switch
            {
                // version >= OVRP_1_12_0
                ControllerDomain.Left => OvrpApi.ovrp_GetNodePoseState(OVRPlugin.Step.Render, OVRPlugin.Node.HandLeft),
                ControllerDomain.Right =>
                    OvrpApi.ovrp_GetNodePoseState(OVRPlugin.Step.Render, OVRPlugin.Node.HandRight),
                _ => _cachedPoseState
            };

            // --------------------------------------
            // Cache buttons
            // --------------------------------------
            tmpBool = OVRInput.Get(OVRInput.Button.One, _ovrControllerMask);
            if (_controllerInputData.IsPressedPrimary != tmpBool)
            {
                _controllerInputData.IsPressedPrimary = tmpBool;
                _pressedPrimaryDelegate?.Invoke(tmpBool);
            }

            tmpBool = OVRInput.Get(OVRInput.Touch.One, _ovrControllerMask);
            if (_controllerInputData.IsTouchedPrimary != tmpBool)
            {
                _controllerInputData.IsTouchedPrimary = tmpBool;
                _touchedPrimaryDelegate?.Invoke(tmpBool);
            }

            tmpBool = OVRInput.Get(OVRInput.Button.Two, _ovrControllerMask);
            if (_controllerInputData.IsPressedSecondary != tmpBool)
            {
                _controllerInputData.IsPressedSecondary = tmpBool;
                _pressedSecondaryDelegate?.Invoke(tmpBool);
            }

            tmpBool = OVRInput.Get(OVRInput.Touch.Two, _ovrControllerMask);
            if (_controllerInputData.IsTouchedSecondary != tmpBool)
            {
                _controllerInputData.IsTouchedSecondary = tmpBool;
                _touchedSecondaryDelegate?.Invoke(tmpBool);
            }

            tmpBool = OVRInput.Get(OVRInput.Button.PrimaryThumbstick, _ovrControllerMask);
            if (_controllerInputData.IsPressedStick != tmpBool)
            {
                _controllerInputData.IsPressedStick = tmpBool;
                _pressedStickDelegate?.Invoke(tmpBool);
            }

            tmpBool = OVRInput.Get(OVRInput.Touch.PrimaryThumbstick, _ovrControllerMask);
            if (_controllerInputData.IsTouchedStick != tmpBool)
            {
                _controllerInputData.IsTouchedStick = tmpBool;
                _touchedStickDelegate?.Invoke(tmpBool);
            }

            var tmpVec2 = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, _ovrControllerMask);
            if (_controllerInputData.StickX != tmpVec2.x || _controllerInputData.StickY != tmpVec2.y)
            {
                _controllerInputData.StickX = tmpVec2.x;
                _controllerInputData.StickY = tmpVec2.y;
                _changedStickDelegate?.Invoke(_controllerInputData.StickX, _controllerInputData.StickY);
            }

            tmpBool = OVRInput.Get(OVRInput.Touch.PrimaryThumbRest, _ovrControllerMask);
            if (_controllerInputData.IsTouchedThumbRest != tmpBool) _controllerInputData.IsTouchedThumbRest = tmpBool;
        }

        private class ControllerInputData
        {
            public float Grip;
            public bool IsPressedPrimary;
            public bool IsPressedSecondary;
            public bool IsPressedStick;
            public bool IsTouchedGrip;
            public bool IsTouchedPrimary;
            public bool IsTouchedSecondary;
            public bool IsTouchedStick;
            public bool IsTouchedThumbRest;
            public bool IsTouchedTrigger;
            public float StickX;
            public float StickY;
            public float Trigger;

            internal (float X, float Y) Stick => (StickX, StickY);
        }
    }
}