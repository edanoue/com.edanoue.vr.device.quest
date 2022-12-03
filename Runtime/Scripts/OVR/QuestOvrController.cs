#nullable enable

using System;
using Edanoue.VR.Device.Core;
using Edanoue.VR.Device.Quest.Internal;
using UnityEngine;

namespace Edanoue.VR.Device.Quest
{
    public abstract class QuestOvrControllerBase : IController,IHasVelocity, IUpdatable
    {
        private readonly OVRInput.Controller _controller;

        private readonly ControllerDomain _controllerDomain;
        private Action<float, float, float>? _changedPositionDelegate;
        private Action<float, float, float, float>? _changedRotationDelegate;
        private Action<float, float>? _changedStickDelegate;

        private Action? _establishedConnectionDelegate;


        private bool _isConnected;

        private bool _isPressedPrimary;
        private bool _isPressedStick;
        private bool _isTouchedPrimary;
        private bool _isTouchedStick;
        private Action? _lostConnectionDelegate;

        private OVRPlugin.PoseStatef _cachedPoseState;

        private Action<bool>? _pressedPrimaryDelegate;
        private Action<bool>? _pressedStickDelegate;
        private float _stickX;
        private float _stickY;

        private Action<bool>? _touchedPrimaryDelegate;
        private Action<bool>? _touchedStickDelegate;

        internal QuestOvrControllerBase(OVRInput.Controller controller, ControllerDomain controllerDomain)
        {
            _controller = controller;
            _controllerDomain = controllerDomain;
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

        event Action<float, float, float>? ITracker.ChangedPosition
        {
            add => _changedPositionDelegate += value;
            remove => _changedPositionDelegate -= value;
        }

        (float W, float X, float Y, float Z) ITracker.Rotation
        {
            get
            {
                var o = _cachedPoseState.Pose.ToOVRPose().orientation;
                return (o.w, o.x, o.y, o.z);
            }
        }

        event Action<float, float, float, float>? ITracker.ChangedRotation
        {
            add => _changedRotationDelegate += value;
            remove => _changedRotationDelegate -= value;
        }
        
        (float X, float Y, float Z) IHasVelocity.LinearVelocity
        {
            get
            {
                var p = _cachedPoseState.Velocity.FromFlippedZVector3f();
                return (p.x, p.y, p.z);
            }
        }
        
        (float X, float Y, float Z) IHasVelocity.LinearAcceleration
        {
            get
            {
                var p = _cachedPoseState.Acceleration.FromFlippedZVector3f();
                return (p.x, p.y, p.z);
            }
        }
        
        (float X, float Y, float Z) IHasVelocity.AngularVelocity
        {
            get
            {
                var p = _cachedPoseState.AngularVelocity.FromFlippedZVector3f();
                return (p.x, p.y, p.z);
            }
        }
        
        (float X, float Y, float Z) IHasVelocity.AngularAcceleration
        {
            get
            {
                var p = _cachedPoseState.AngularAcceleration.FromFlippedZVector3f();
                return (p.x, p.y, p.z);
            }
        }

        void IUpdatable.Update(float deltaTime)
        {
            // --------------------------------------
            // Connection check
            // --------------------------------------
            var tmpBool = false;
            tmpBool = OVRInput.IsControllerConnected(_controller);
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
                ControllerDomain.Right => OvrpApi.ovrp_GetNodePoseState(OVRPlugin.Step.Render, OVRPlugin.Node.HandRight),
                _ => _cachedPoseState
            };

            // --------------------------------------
            // Cache buttons
            // --------------------------------------
            tmpBool = OVRInput.Get(OVRInput.Button.One, _controller);
            if (_isPressedPrimary != tmpBool)
            {
                _isPressedPrimary = tmpBool;
                _pressedPrimaryDelegate?.Invoke(tmpBool);
            }

            tmpBool = OVRInput.Get(OVRInput.Touch.One, _controller);
            if (_isTouchedPrimary != tmpBool)
            {
                _isTouchedPrimary = tmpBool;
                _touchedPrimaryDelegate?.Invoke(tmpBool);
            }

            tmpBool = OVRInput.Get(OVRInput.Button.PrimaryThumbstick, _controller);
            if (_isPressedStick != tmpBool)
            {
                _isPressedStick = tmpBool;
                _pressedStickDelegate?.Invoke(tmpBool);
            }

            tmpBool = OVRInput.Get(OVRInput.Touch.PrimaryThumbstick, _controller);
            if (_isTouchedStick != tmpBool)
            {
                _isTouchedStick = tmpBool;
                _touchedStickDelegate?.Invoke(tmpBool);
            }

            var tmpVec2 = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, _controller);
            if (_stickX != tmpVec2.x || _stickY != tmpVec2.y)
            {
                _stickX = tmpVec2.x;
                _stickY = tmpVec2.y;
                _changedStickDelegate?.Invoke(_stickX, _stickY);
            }
        }
    }

    public class QuestOvrControllerLeft : QuestOvrControllerBase
    {
        public QuestOvrControllerLeft() : base(OVRInput.Controller.LTouch, ControllerDomain.Left)
        {
        }
    }

    public class QuestOvrControllerRight : QuestOvrControllerBase
    {
        public QuestOvrControllerRight() : base(OVRInput.Controller.RTouch, ControllerDomain.Right)
        {
        }
    }
}