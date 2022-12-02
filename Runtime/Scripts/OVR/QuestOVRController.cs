#nullable enable

using System;
using Edanoue.VR.Device.Core;

namespace Edanoue.VR.Device.Quest
{
    public abstract class QuestOvrControllerBase : IController, IUpdatable
    {
        private Action<float, float>? _changedStickDelegate;
        protected OVRInput.Controller _contoller;

        private bool _isPressedPrimary;
        private bool _isPressedStick;
        private bool _isTouchedPrimary;
        private bool _isTouchedStick;

        private OVRPose _pose;

        private Action<bool>? _pressedPrimaryDelegate;
        private Action<bool>? _pressedStickDelegate;
        private float _stickX;
        private float _stickY;

        private Action<bool>? _touchedPrimaryDelegate;
        private Action<bool>? _touchedStickDelegate;

        protected abstract ControllerDomain _controllerDomain { get; }

        bool ITracker.IsConnected
        {
            get
            {
                return OVRInput.IsControllerConnected(_contoller);
            }
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

        void IUpdatable.Update(float deltaTime)
        {
            // --------------------------------------
            // Cache Possition and Rotations
            // Use OVRP method
            // --------------------------------------
            // Ref: OVRInput.GetLocalControllerPosition
            if (_controllerDomain == ControllerDomain.Left)
                _pose = OVRPluginAPI.ovrp_GetNodePoseState(OVRPlugin.Step.Render, OVRPlugin.Node.HandLeft).Pose
                    .ToOVRPose();
            else
                _pose = OVRPluginAPI.ovrp_GetNodePoseState(OVRPlugin.Step.Render, OVRPlugin.Node.HandRight).Pose
                    .ToOVRPose();

            // --------------------------------------
            // Cache buttons
            // --------------------------------------
            var tmpBool = false;
            tmpBool = OVRInput.Get(OVRInput.Button.One, _contoller);
            if (_isPressedPrimary != tmpBool)
            {
                _isPressedPrimary = tmpBool;
                _pressedPrimaryDelegate?.Invoke(tmpBool);
            }

            tmpBool = OVRInput.Get(OVRInput.Touch.One, _contoller);
            if (_isTouchedPrimary != tmpBool)
            {
                _isTouchedPrimary = tmpBool;
                _touchedPrimaryDelegate?.Invoke(tmpBool);
            }

            tmpBool = OVRInput.Get(OVRInput.Button.PrimaryThumbstick, _contoller);
            if (_isPressedStick != tmpBool)
            {
                _isPressedStick = tmpBool;
                _touchedPrimaryDelegate?.Invoke(tmpBool);
            }

            tmpBool = OVRInput.Get(OVRInput.Touch.PrimaryThumbstick, _contoller);
            if (_isTouchedStick != tmpBool)
            {
                _isTouchedStick = tmpBool;
                _touchedStickDelegate?.Invoke(tmpBool);
            }

            var tmpVec2 = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, _contoller);
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
        public QuestOvrControllerLeft()
        {
            _contoller = OVRInput.Controller.LTouch;
        }

        protected override ControllerDomain _controllerDomain => ControllerDomain.Left;
    }

    public class QuestOvrControllerRight : QuestOvrControllerBase
    {
        public QuestOvrControllerRight()
        {
            _contoller = OVRInput.Controller.RTouch;
        }

        protected override ControllerDomain _controllerDomain => ControllerDomain.Right;
    }
}