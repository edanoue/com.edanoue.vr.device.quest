// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using System.Runtime.CompilerServices;
using Edanoue.VR.Device.Core;
using Edanoue.VR.Device.Quest.Internal;
using UnityEngine;

namespace Edanoue.VR.Device.Quest
{
    internal sealed class OvrTouchController
    {
        private readonly uint                       _controllerMask;
        public           OVRPlugin.ControllerState5 StateRef;

        public OvrTouchController(ControllerDomain domain)
        {
            if (domain == ControllerDomain.Left)
            {
                _controllerMask = (uint)OVRPlugin.Controller.LTouch;
            }
            else
            {
                _controllerMask = (uint)OVRPlugin.Controller.RTouch;
            }
        }

        public void Update()
        {
            OvrpApi.ovrp_GetControllerState5(_controllerMask, ref StateRef);
        }
    }

    /// <summary>
    /// Oculus Quest Touch controller (OQ, OQ2 con)
    /// </summary>
    public class OvrControllerQuestTouch :
        IController,
        ISupportedVelocity,
        ISupportedBodyVibration
    {
        private const float _INPUT_TOLERANCE          = 0.0001f;
        private const float _AXIS_DEAD_ZONE_THRESHOLD = 0.2f;

        private readonly OvrTouchController   _controller;
        private readonly ControllerDomain     _controllerDomain;
        private readonly ControllerInputData  _inputCache;
        protected        OVRPlugin.PoseStatef _cachedPoseState;
        private          Action?              _establishedConnectionDelegate;

        private bool _isConnected;

        private Action? _lostConnectionDelegate;

        internal OvrControllerQuestTouch(ControllerDomain controllerDomain)
        {
            _controllerDomain = controllerDomain;
            _inputCache = new ControllerInputData();
            _controller = new OvrTouchController(controllerDomain);
        }

        protected OVRInput.Controller _ovrControllerMask
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

        public virtual ControllerDeviceType DeviceType => ControllerDeviceType.META_QUEST_TOUCH;
        
        ControllerDomain IController.Domain => _controllerDomain;
        bool IController.IsPressedPrimary => _inputCache.IsPressedPrimary;
        bool IController.IsTouchedPrimary => _inputCache.IsTouchedPrimary;

        event Action<bool>? IController.PressedPrimary
        {
            add => _inputCache.PressedPrimary += value;
            remove => _inputCache.PressedPrimary -= value;
        }

        event Action<bool>? IController.TouchedPrimary
        {
            add => _inputCache.TouchedPrimary += value;
            remove => _inputCache.TouchedPrimary -= value;
        }

        bool IController.IsPressedSecondary => _inputCache.IsPressedSecondary;
        bool IController.IsTouchedSecondary => _inputCache.IsTouchedSecondary;

        event Action<bool>? IController.PressedSecondary
        {
            add => _inputCache.PressedSecondary += value;
            remove => _inputCache.PressedSecondary -= value;
        }

        event Action<bool>? IController.TouchedSecondary
        {
            add => _inputCache.TouchedSecondary += value;
            remove => _inputCache.TouchedSecondary -= value;
        }

        bool IController.IsTouchedTrigger => _inputCache.IsTouchedTrigger;
        float IController.Trigger => _inputCache.Trigger;

        event Action<bool>? IController.TouchedTrigger
        {
            add => _inputCache.TouchedTrigger += value;
            remove => _inputCache.TouchedTrigger -= value;
        }

        event Action<float>? IController.ChangedTrigger
        {
            add => _inputCache.ChangedTrigger += value;
            remove => _inputCache.ChangedTrigger -= value;
        }

        /// <summary>
        /// Quest Touch Controller always return false.
        /// </summary>
        bool IController.IsTouchedGrip => _inputCache.IsTouchedGrip;

        float IController.Grip => _inputCache.Grip;

        event Action<bool>? IController.TouchedGrip
        {
            add => _inputCache.TouchedGrip += value;
            remove => _inputCache.TouchedGrip -= value;
        }

        event Action<float>? IController.ChangedGrip
        {
            add => _inputCache.ChangedGrip += value;
            remove => _inputCache.ChangedGrip -= value;
        }

        bool IController.IsPressedStick => _inputCache.IsPressedStick;
        bool IController.IsTouchedStick => _inputCache.IsTouchedStick;

        (float X, float Y) IController.Stick => _inputCache.Stick;

        event Action<bool>? IController.PressedStick
        {
            add => _inputCache.PressedStick += value;
            remove => _inputCache.PressedStick -= value;
        }

        event Action<bool>? IController.TouchedStick
        {
            add => _inputCache.TouchedStick += value;
            remove => _inputCache.TouchedStick -= value;
        }

        event Action<float, float>? IController.ChangedStick
        {
            add => _inputCache.ChangedStick += value;
            remove => _inputCache.ChangedStick -= value;
        }

        bool IController.IsTouchedThumbRest => _inputCache.IsTouchedThumbRest;

        event Action<bool> IController.TouchedThumbRest
        {
            add => _inputCache.TouchedThumbRest += value;
            remove => _inputCache.TouchedThumbRest -= value;
        }

        /// <summary>
        /// Home の設定で左右を入れ替えていても常に左手側の方の Start が反応します (2022-12 時点)
        /// 右手の方は System に予約されているので取得できません
        /// </summary>
        bool IController.IsPressedStart => _inputCache.IsPressedStart;

        event Action<bool> IController.PressedStart
        {
            add => _inputCache.PressedStart += value;
            remove => _inputCache.PressedStart -= value;
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

        (float W, float X, float Y, float Z) ITracker.Rotation
        {
            get
            {
                var o = _cachedPoseState.Pose.ToOVRPose().orientation;
                return (o.w, o.x, o.y, o.z);
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
        void ISupportedBodyVibration.SetVibration(float frequency, float amplitude)
        {
            if (_controllerDomain == ControllerDomain.Left)
            {
                OvrpApi.ovrp_SetControllerLocalizedVibration(
                    OVRPlugin.Controller.LTouch,
                    OVRPlugin.HapticsLocation.Hand,
                    frequency,
                    amplitude
                );
            }
            else
            {
                OvrpApi.ovrp_SetControllerLocalizedVibration(
                    OVRPlugin.Controller.RTouch,
                    OVRPlugin.HapticsLocation.Hand,
                    frequency,
                    amplitude
                );
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

        internal void Update()
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
            // Use OVRP native api method
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
            // Cache buttons state
            // Use OVRP native api method
            // --------------------------------------
            _controller.Update();
            ref var state = ref _controller.StateRef;
            ref var rawButtons = ref state.Buttons;

            if (_controllerDomain == ControllerDomain.Left)
            {
                // primary (A or X) pressed
                _inputCache.IsPressedPrimary = (rawButtons & (uint)OVRInput.RawButton.X) != 0;

                // secondary button (B or Y) pressed
                _inputCache.IsPressedSecondary = (rawButtons & (uint)OVRInput.RawButton.Y) != 0;

                // thumbstick pressed
                _inputCache.IsPressedStick = (rawButtons & (uint)OVRInput.RawButton.LThumbstick) != 0;

                // start button touched
                _inputCache.IsPressedStart = (rawButtons & (uint)OVRInput.RawButton.Start) != 0;
            }
            else
            {
                // primary (A or X) pressed
                _inputCache.IsPressedPrimary = (rawButtons & (uint)OVRInput.RawButton.A) != 0;

                // secondary button (B or Y) pressed
                _inputCache.IsPressedSecondary = (rawButtons & (uint)OVRInput.RawButton.B) != 0;

                // thumbstick pressed
                _inputCache.IsPressedStick = (rawButtons & (uint)OVRInput.RawButton.RThumbstick) != 0;

                // start button touched
                _inputCache.IsPressedStart = (rawButtons & (uint)OVRInput.RawButton.Start) != 0;
            }

            ref var rawTouches = ref state.Touches;
            if (_controllerDomain == ControllerDomain.Left)
            {
                // primary (A or X) touched
                _inputCache.IsTouchedPrimary = (rawTouches & (uint)OVRInput.RawTouch.X) != 0;

                // secondary button (B or Y) touched
                _inputCache.IsTouchedSecondary = (rawTouches & (uint)OVRInput.RawTouch.Y) != 0;

                // trigger touched
                _inputCache.IsTouchedTrigger = (rawTouches & (uint)OVRInput.RawTouch.LIndexTrigger) != 0;

                // thumbstick touched
                _inputCache.IsTouchedStick = (rawTouches & (uint)OVRInput.RawTouch.LThumbstick) != 0;

                // thumb rest touched
                _inputCache.IsTouchedThumbRest = (rawTouches & (uint)OVRInput.RawTouch.LThumbRest) != 0;
            }
            else
            {
                // primary (A or X) touched
                _inputCache.IsTouchedPrimary = (rawTouches & (uint)OVRInput.RawTouch.A) != 0;

                // secondary button (B or Y) touched
                _inputCache.IsTouchedSecondary = (rawTouches & (uint)OVRInput.RawTouch.B) != 0;

                // trigger touched
                _inputCache.IsTouchedTrigger = (rawTouches & (uint)OVRInput.RawTouch.RIndexTrigger) != 0;

                // thumbstick touched
                _inputCache.IsTouchedStick = (rawTouches & (uint)OVRInput.RawTouch.RThumbstick) != 0;

                // thumb rest touched
                _inputCache.IsTouchedThumbRest = (rawTouches & (uint)OVRInput.RawTouch.RThumbRest) != 0;
            }

            if (_controllerDomain == ControllerDomain.Left)
            {
                // trigger value
                {
                    var axis = CalculateDeadZone(state.LIndexTrigger, _AXIS_DEAD_ZONE_THRESHOLD);
                    _inputCache.Trigger = CalculateAbsMax(0f, axis);
                    // Debug.Log($"Trigger:{_inputCache.Trigger}");
                }

                // grip value
                {
                    var axis = CalculateDeadZone(state.LHandTrigger, _AXIS_DEAD_ZONE_THRESHOLD);
                    _inputCache.Grip = CalculateAbsMax(0f, axis);
                }

                // thumbstick 2D value
                {
                    var tmpVec2 = state.LThumbstick;
                    var axis = new Vector2(tmpVec2.x, tmpVec2.y);
                    axis = CalculateDeadZone(axis, _AXIS_DEAD_ZONE_THRESHOLD);
                    axis = CalculateAbsMax(Vector2.zero, axis);
                    _inputCache.Stick = (axis.x, axis.y);
                }
            }
            else
            {
                // trigger value
                {
                    var axis = CalculateDeadZone(state.RIndexTrigger, _AXIS_DEAD_ZONE_THRESHOLD);
                    _inputCache.Trigger = CalculateAbsMax(0f, axis);
                }

                // grip value
                {
                    var axis = CalculateDeadZone(state.RHandTrigger, _AXIS_DEAD_ZONE_THRESHOLD);
                    _inputCache.Grip = CalculateAbsMax(0f, axis);
                }

                // thumbstick 2D value
                {
                    var tmpVec2 = state.RThumbstick;
                    var axis = new Vector2(tmpVec2.x, tmpVec2.y);
                    axis = CalculateDeadZone(axis, _AXIS_DEAD_ZONE_THRESHOLD);
                    axis = CalculateAbsMax(Vector2.zero, axis);
                    _inputCache.Stick = (axis.x, axis.y);
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CalculateDeadZone(float a, float deadZone)
        {
            var mag = a >= 0 ? a : -a;

            if (mag <= deadZone)
            {
                return 0.0f;
            }

            a *= (mag - deadZone) / (1.0f - deadZone);

            if (a * a > 1.0f)
            {
                return a >= 0 ? 1.0f : -1.0f;
            }

            return a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2 CalculateDeadZone(Vector2 a, float deadZone)
        {
            if (a.sqrMagnitude <= deadZone * deadZone)
            {
                return Vector2.zero;
            }

            a *= (a.magnitude - deadZone) / (1.0f - deadZone);

            return a.sqrMagnitude > 1.0f ? a.normalized : a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CalculateAbsMax(float a, float b)
        {
            var absA = a >= 0 ? a : -a;
            var absB = b >= 0 ? b : -b;
            return absA >= absB ? a : b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2 CalculateAbsMax(Vector2 a, Vector2 b)
        {
            var absA = a.sqrMagnitude;
            var absB = b.sqrMagnitude;
            return absA >= absB ? a : b;
        }

        /// <summary>
        /// </summary>
        private sealed class ControllerInputData
        {
            private  float                 _grip;
            private  bool                  _isPressedPrimary;
            private  bool                  _isPressedSecondary;
            private  bool                  _isPressedStart;
            private  bool                  _isPressedStick;
            private  bool                  _isTouchedGrip;
            private  bool                  _isTouchedPrimary;
            private  bool                  _isTouchedSecondary;
            private  bool                  _isTouchedStick;
            private  bool                  _isTouchedThumbRest;
            private  bool                  _isTouchedTrigger;
            private  float                 _stickX;
            private  float                 _stickY;
            private  float                 _trigger;
            internal Action<float>?        ChangedGrip;
            internal Action<float, float>? ChangedStick;
            internal Action<float>?        ChangedTrigger;

            internal Action<bool>? PressedPrimary;
            internal Action<bool>? PressedSecondary;
            internal Action<bool>? PressedStart;
            internal Action<bool>? PressedStick;
            internal Action<bool>? TouchedGrip;
            internal Action<bool>? TouchedPrimary;
            internal Action<bool>? TouchedSecondary;
            internal Action<bool>? TouchedStick;
            internal Action<bool>? TouchedThumbRest;
            internal Action<bool>? TouchedTrigger;

            internal bool IsPressedPrimary
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _isPressedPrimary;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    if (value ^ _isPressedPrimary)
                    {
                        _isPressedPrimary = value;
                        PressedPrimary?.Invoke(value);
                    }
                }
            }

            internal bool IsTouchedPrimary
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _isTouchedPrimary;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    if (value ^ _isTouchedPrimary)
                    {
                        _isTouchedPrimary = value;
                        TouchedPrimary?.Invoke(value);
                    }
                }
            }

            internal bool IsPressedSecondary
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _isPressedSecondary;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    if (value ^ _isPressedSecondary)
                    {
                        _isPressedSecondary = value;
                        PressedSecondary?.Invoke(value);
                    }
                }
            }

            internal bool IsTouchedSecondary
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _isTouchedSecondary;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    if (value ^ _isTouchedSecondary)
                    {
                        _isTouchedSecondary = value;
                        TouchedSecondary?.Invoke(value);
                    }
                }
            }

            internal bool IsTouchedTrigger
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _isTouchedTrigger;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    if (value ^ _isTouchedTrigger)
                    {
                        _isTouchedTrigger = value;
                        TouchedTrigger?.Invoke(value);
                    }
                }
            }

            internal float Trigger
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _trigger;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    if (Math.Abs(value - _trigger) < _INPUT_TOLERANCE)
                    {
                        return;
                    }

                    _trigger = value;
                    ChangedTrigger?.Invoke(value);
                }
            }

            internal bool IsTouchedGrip
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _isTouchedGrip;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    if (value ^ _isTouchedGrip)
                    {
                        _isTouchedGrip = value;
                        TouchedGrip?.Invoke(value);
                    }
                }
            }

            internal float Grip
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _grip;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    if (!(Math.Abs(value - _grip) > _INPUT_TOLERANCE))
                    {
                        return;
                    }

                    _grip = value;
                    ChangedGrip?.Invoke(value);
                }
            }

            internal bool IsPressedStick
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _isPressedStick;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    if (value ^ _isPressedStick)
                    {
                        _isPressedStick = value;
                        PressedStick?.Invoke(value);
                    }
                }
            }

            internal bool IsTouchedStick
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _isTouchedStick;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    if (value ^ _isTouchedStick)
                    {
                        _isTouchedStick = value;
                        TouchedStick?.Invoke(value);
                    }
                }
            }

            internal (float X, float Y) Stick
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => (_stickX, _stickY);
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    if (Math.Abs(value.X - _stickX) > _INPUT_TOLERANCE ||
                        Math.Abs(value.Y - _stickY) > _INPUT_TOLERANCE)
                    {
                        _stickX = value.X;
                        _stickY = value.Y;
                        ChangedStick?.Invoke(value.X, value.Y);
                    }
                }
            }

            internal bool IsTouchedThumbRest
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _isTouchedThumbRest;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    if (value ^ _isTouchedThumbRest)
                    {
                        _isTouchedThumbRest = value;
                        TouchedThumbRest?.Invoke(value);
                    }
                }
            }

            internal bool IsPressedStart
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _isPressedStart;
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    if (value ^ _isPressedStart)
                    {
                        _isPressedStart = value;
                        PressedStart?.Invoke(value);
                    }
                }
            }
        }
    }
}