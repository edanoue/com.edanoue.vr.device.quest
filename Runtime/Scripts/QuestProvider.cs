#nullable enable
using System;
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
            // TODO: 使ってないです
            var deviceModel = SystemInfo.deviceModel;

            // OVRP の関数から現在接続されている Headset を取得
            var headsetType = OVRPlugin.GetSystemHeadsetType();
            
            // None の場合
            // OVR が利用可能な Headset が見つからなかった場合
            // 南: 特にまだ決めていませんが めちゃくちゃエラーなのでここで止めます
            if (headsetType == OVRPlugin.SystemHeadset.None)
            {
                Debug.LogAssertion("Can not find available headset with OVR.");
                throw new ApplicationException("Can not find available headset with OVR.");
            }

            // Standalone の Meta Quest 2 の場合
            if (headsetType == OVRPlugin.SystemHeadset.Oculus_Quest_2)
            {
                _headset = new OvrHeadsetQuest2();
                _leftController = new OvrControllerQuestTouch(ControllerDomain.Left);
                _rightController = new OvrControllerQuestTouch(ControllerDomain.Right);
            }

            // Oculus Link 経由の Meta Quest 2
            // 南: 本来はビルド対象ではないですが, 開発中に使用することがあるので
            else if (headsetType == OVRPlugin.SystemHeadset.Oculus_Link_Quest_2)
            {
                // 南: 問題があれば専用のクラス用意してください
                _headset = new OvrHeadsetQuest2();
                _leftController = new OvrControllerQuestTouch(ControllerDomain.Left);
                _rightController = new OvrControllerQuestTouch(ControllerDomain.Right);
            }

            else
            {
                // 特定クラスが見つからなかった場合のフォールバック
                // 南: Meta Quest 2 用のものを使用するようにしています
                Debug.LogWarning($"Device '{headsetType}' is not implemented. Use default Quest2 setup.");
                _headset = new OvrHeadsetQuest2();
                _leftController = new OvrControllerQuestTouch(ControllerDomain.Left);
                _rightController = new OvrControllerQuestTouch(ControllerDomain.Right);
            }
        }

        IHeadset IProvider.Headset => _headset;
        IController IProvider.LeftController => _leftController;
        IController IProvider.RightController => _rightController;
        
        float[] IProvider.AvailableRefreshRates => OVRManager.display.displayFrequenciesAvailable;

        float IProvider.RefreshRate
        {
            get => OVRPlugin.systemDisplayFrequency;
            set => OVRPlugin.systemDisplayFrequency = value;
        }
    }
}