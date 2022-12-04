#nullable enable

using System;
using Edanoue.VR.Device.Core;
using Edanoue.VR.Device.Quest.Internal;

namespace Edanoue.VR.Device.Quest
{
    /// <summary>
    /// Oculus Quest Touch controller (OQ, OQ2 con)
    /// </summary>
    public class OvrControllerQuestTouch : IController, ISupportedVelocity, ISupportedAcceleration, IVibration,
        IUpdatable
    {
        private readonly ControllerDomain _controllerDomain;

        private OVRPlugin.PoseStatef _cachedPoseState;
        private Action<float, float>? _changedStickDelegate;

        private Action? _establishedConnectionDelegate;


        private bool _isConnected;

        private bool _isPressedPrimary;
        private bool _isPressedStick;
        private bool _isTouchedPrimary;
        private bool _isTouchedStick;

        private bool _isTouchedThumbRest;
        private Action? _lostConnectionDelegate;

        private Action<bool>? _pressedPrimaryDelegate;
        private Action<bool>? _pressedStickDelegate;
        private float _stickX;
        private float _stickY;

        private Action<bool>? _touchedPrimaryDelegate;
        private Action<bool>? _touchedStickDelegate;

        internal OvrControllerQuestTouch(ControllerDomain controllerDomain)
        {
            _controllerDomain = controllerDomain;
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
        bool IController.IsPressedPrimary => _isPressedPrimary;
        bool IController.IsTouchedPrimary => _isTouchedPrimary;

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

        bool IController.IsPressedStick => _isPressedStick;
        bool IController.IsTouchedStick => _isTouchedStick;

        (float X, float Y) IController.Stick => (_stickX, _stickY);


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

        bool IController.IsTouchedThumbRest => _isTouchedThumbRest;

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

        // 南: MQ2 の Touch Controller は取得できない (常に 0) っぽいです
        // https://answers.unity.com/questions/1669595/get-oculus-riftrifts-controllers-battery-level.html
        /*
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
            tmpBool = OVRInput.IsControllerConnected(_ovrControllerMask);
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
            if (_isPressedPrimary != tmpBool)
            {
                _isPressedPrimary = tmpBool;
                _pressedPrimaryDelegate?.Invoke(tmpBool);
            }

            tmpBool = OVRInput.Get(OVRInput.Touch.One, _ovrControllerMask);
            if (_isTouchedPrimary != tmpBool)
            {
                _isTouchedPrimary = tmpBool;
                _touchedPrimaryDelegate?.Invoke(tmpBool);
            }

            tmpBool = OVRInput.Get(OVRInput.Button.PrimaryThumbstick, _ovrControllerMask);
            if (_isPressedStick != tmpBool)
            {
                _isPressedStick = tmpBool;
                _pressedStickDelegate?.Invoke(tmpBool);
            }

            tmpBool = OVRInput.Get(OVRInput.Touch.PrimaryThumbstick, _ovrControllerMask);
            if (_isTouchedStick != tmpBool)
            {
                _isTouchedStick = tmpBool;
                _touchedStickDelegate?.Invoke(tmpBool);
            }

            var tmpVec2 = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, _ovrControllerMask);
            if (_stickX != tmpVec2.x || _stickY != tmpVec2.y)
            {
                _stickX = tmpVec2.x;
                _stickY = tmpVec2.y;
                _changedStickDelegate?.Invoke(_stickX, _stickY);
            }

            tmpBool = OVRInput.Get(OVRInput.Touch.PrimaryThumbRest, _ovrControllerMask);
            if (_isTouchedThumbRest != tmpBool) _isTouchedThumbRest = tmpBool;
        }
    }
}