// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using Edanoue.VR.Device.Core;
using Edanoue.VR.Device.Quest.Internal;

namespace Edanoue.VR.Device.Quest
{
    /// <summary>
    ///     Oculus Quest Touch controller (OQ, OQ2 con)
    /// </summary>
    public class OvrControllerQuestTouch :
        IController,
        ISupportedVelocity,
        ISupportedBodyVibration,
        IUpdatable
    {
        private const    float                InputTolerance = 0.0001f;
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
        ///     Quest Touch Controller always return false.
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
            const OVRInput.HapticsLocation location = OVRInput.HapticsLocation.Hand;
            OVRInput.SetControllerLocalizedVibration(location, frequency, amplitude, _ovrControllerMask);
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
            // primary (A or X) pressed
            _inputCache.IsPressedPrimary = OVRInput.Get(OVRInput.Button.One, _ovrControllerMask);

            // primary (A or X) touched
            _inputCache.IsTouchedPrimary = OVRInput.Get(OVRInput.Touch.One, _ovrControllerMask);

            // secondary button (B or Y) pressed
            _inputCache.IsPressedSecondary = OVRInput.Get(OVRInput.Button.Two, _ovrControllerMask);

            // secondary button (B or Y) touched
            _inputCache.IsTouchedSecondary = OVRInput.Get(OVRInput.Touch.Two, _ovrControllerMask);

            // trigger touched
            _inputCache.IsTouchedTrigger = OVRInput.Get(OVRInput.Touch.PrimaryIndexTrigger, _ovrControllerMask);

            // trigger value
            _inputCache.Trigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, _ovrControllerMask);

            // Note: Oculus Quest Touch controller can't detect grip touch

            // grip value
            _inputCache.Grip = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, _ovrControllerMask);

            // thumbstick pressed
            _inputCache.IsPressedStick = OVRInput.Get(OVRInput.Button.PrimaryThumbstick, _ovrControllerMask);

            // thumbstick touched
            _inputCache.IsTouchedStick = OVRInput.Get(OVRInput.Touch.PrimaryThumbstick, _ovrControllerMask);

            // thumbstick 2D value
            var tmpVec2 = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, _ovrControllerMask);
            _inputCache.Stick = (tmpVec2.x, tmpVec2.y);

            // thumb rest touched
            _inputCache.IsTouchedThumbRest = OVRInput.Get(OVRInput.Touch.PrimaryThumbRest, _ovrControllerMask);
        }


        /// <summary>
        /// </summary>
        private class ControllerInputData
        {
            private  float                 _grip;
            private  bool                  _isPressedPrimary;
            private  bool                  _isPressedSecondary;
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
            internal Action<bool>? PressedStick;
            internal Action<bool>? TouchedGrip;
            internal Action<bool>? TouchedPrimary;
            internal Action<bool>? TouchedSecondary;
            internal Action<bool>? TouchedStick;
            internal Action<bool>? TouchedThumbRest;
            internal Action<bool>? TouchedTrigger;

            internal bool IsPressedPrimary
            {
                get => _isPressedPrimary;
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
                get => _isTouchedPrimary;
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
                get => _isPressedSecondary;
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
                get => _isTouchedSecondary;
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
                get => _isTouchedTrigger;
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
                get => _trigger;
                set
                {
                    if (!(Math.Abs(value - _trigger) > InputTolerance))
                    {
                        return;
                    }

                    _trigger = value;
                    ChangedTrigger?.Invoke(value);
                }
            }

            internal bool IsTouchedGrip
            {
                get => _isTouchedGrip;
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
                get => _grip;
                set
                {
                    if (!(Math.Abs(value - _grip) > InputTolerance))
                    {
                        return;
                    }

                    _grip = value;
                    ChangedGrip?.Invoke(value);
                }
            }

            internal bool IsPressedStick
            {
                get => _isPressedStick;
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
                get => _isTouchedStick;
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
                get => (_stickX, _stickY);
                set
                {
                    if (Math.Abs(value.X - _stickX) > InputTolerance || Math.Abs(value.Y - _stickY) > InputTolerance)
                    {
                        _stickX = value.X;
                        _stickY = value.Y;
                        ChangedStick?.Invoke(value.X, value.Y);
                    }
                }
            }

            internal bool IsTouchedThumbRest
            {
                get => _isTouchedThumbRest;
                set
                {
                    if (value ^ _isTouchedThumbRest)
                    {
                        _isTouchedThumbRest = value;
                        TouchedThumbRest?.Invoke(value);
                    }
                }
            }
        }
    }
}