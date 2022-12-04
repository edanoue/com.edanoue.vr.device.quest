#nullable enable
using Edanoue.VR.Device.Core;
using UnityEngine;

namespace Edanoue.VR.Device.Quest
{
    /// <summary>
    ///     Quest device provider class.
    ///     Currently use OVR (OVRP) Plugin.
    /// </summary>
    public class QuestProvider : IProvider
    {
        private readonly OvrHeadsetQuest2 _headset;
        private readonly OvrControllerQuestTouch _leftController;
        private readonly OvrControllerQuestTouch _rightController;

        public QuestProvider()
        {
            // 南: OVR (OVRP) を利用した実装 のもので初期化しています

            // HMD のデバイス情報を取得する
            // SystemInfo.deviceModel
            // Meta Quest 2 => "Oculus Quest"
            var deviceModel = SystemInfo.deviceModel;

            // SystemInfo.deviceName
            // Meta Quest 2 => "Meta Quest 2"
            var deviceName = SystemInfo.deviceName;

            // Meta Quest 2 の場合
            if (deviceName == "Meta Quest 2")
            {
                _headset = new OvrHeadsetQuest2();
                _leftController = new OvrControllerQuestTouch(ControllerDomain.Left);
                _rightController = new OvrControllerQuestTouch(ControllerDomain.Right);
            }

            // 特定クラスが見つからなかった場合のフォールバック
            // 南: Meta Quest 2 用のものを使用するようにしています
            _headset = new OvrHeadsetQuest2();
            _leftController = new OvrControllerQuestTouch(ControllerDomain.Left);
            _rightController = new OvrControllerQuestTouch(ControllerDomain.Right);
        }

        IHeadset IProvider.Headset => _headset;
        IController IProvider.LeftController => _leftController;
        IController IProvider.RightController => _rightController;
    }
}