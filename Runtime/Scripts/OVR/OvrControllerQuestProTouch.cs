// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using Edanoue.VR.Device.Core;

namespace Edanoue.VR.Device.Quest
{
    /// <summary>
    /// Meta Quest Touch Pro controller
    /// Touch Controller に追加で 対応している機能
    /// - Trigger Button の振動 (ISupportedTriggerVibration)
    /// - Thumb Rest の振動 (ISupportedThumbRestVibration)
    /// </summary>
    public class OvrControllerQuestProTouch :
        OvrControllerQuestTouch,
        ISupportedThumbRestVibration,
        ISupportedTriggerVibration
    {
        internal OvrControllerQuestProTouch(ControllerDomain controllerDomain) : base(controllerDomain)
        {
        }
        
        public override ControllerDeviceType DeviceType => ControllerDeviceType.META_QUEST_PRO_TOUCH;

        void ISupportedThumbRestVibration.SetVibration(float frequency, float amplitude)
        {
            const OVRInput.HapticsLocation location = OVRInput.HapticsLocation.Thumb;
            OVRInput.SetControllerLocalizedVibration(location, frequency, amplitude, _ovrControllerMask);
        }

        void ISupportedTriggerVibration.SetVibration(float frequency, float amplitude)
        {
            const OVRInput.HapticsLocation location = OVRInput.HapticsLocation.Index;
            OVRInput.SetControllerLocalizedVibration(location, frequency, amplitude, _ovrControllerMask);
        }

        /*
        // (南) 2022-11 現在 QuestProTouch は 加速度が取得できません
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

        /*
        // (南) 2022-11 現在 QuestProTouch は バッテリー残量が 取得できません
        float ISupportedBattery.Battery
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
    }
}