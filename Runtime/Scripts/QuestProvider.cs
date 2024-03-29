﻿// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using Edanoue.VR.Device.Core;
using UnityEngine;

namespace Edanoue.VR.Device.Quest
{
    /// <summary>
    /// Quest device provider class.
    /// Currently use OVR (OVRP) Plugin.
    /// </summary>
    public class QuestProvider : IProvider
    {
        private readonly OvrHeadsetQuest2         _headset;
        private readonly OvrControllerQuestTouch  _leftController;
        private readonly EdaLightWeightOvrManager _manager;
        private readonly OvrControllerQuestTouch  _rightController;

        private Action? _applicationFocusAcquiredDelegate;
        private Action? _applicationFocusLostDelegate;
        private bool    _hasApplicationFocus;

        public QuestProvider()
        {
            // 南: OVR (OVRP) を利用した実装 のもので初期化しています

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

            // Manager を作成する
            _manager = new EdaLightWeightOvrManager();

            // Standalone の Meta Quest 2 の場合
            if (headsetType == OVRPlugin.SystemHeadset.Oculus_Quest_2)
            {
                _headset = new OvrHeadsetQuest2();
                SetupQuest2Controller(out _leftController, out _rightController);
            }

            // Standalone の Meta Quest 3 の場合
            else if (headsetType == OVRPlugin.SystemHeadset.Meta_Quest_3)
            {
                // ToDo: (南) 実機無いため, 暫定的に Quest 2 のもので初期化しています
                _headset = new OvrHeadsetQuest2();
                SetupQuest2Controller(out _leftController, out _rightController);
            }

            // Oculus Link 経由の Meta Quest 2
            // 南: 本来はビルド対象ではないですが, 開発中に使用することがあるので
            else if (headsetType == OVRPlugin.SystemHeadset.Oculus_Link_Quest_2)
            {
                // (南) 問題があれば専用のクラス用意してください
                _headset = new OvrHeadsetQuest2();
                // (南) Oculus Link では コントローラーの判定ができないのでとりあえず OQ2 用のものを使用しています
                _leftController = new OvrControllerQuestTouch(ControllerDomain.Left);
                _rightController = new OvrControllerQuestTouch(ControllerDomain.Right);
            }

            else
            {
                // OVR はヘッドセットを認識しているが, こっちで実装クラスが見つからなかった場合のフォールバック
                // リリース後に新しい Headset 出てきたけど実装していないなどでここに来る可能性があります
                // 南: Meta Quest 2 用のものを使用するようにしています
                Debug.LogWarning($"Device '{headsetType}' is not implemented. Use default Quest2 setup.");
                _headset = new OvrHeadsetQuest2();
                SetupQuest2Controller(out _leftController, out _rightController);
            }
        }

        IHeadset IProvider.Headset => _headset;
        IController IProvider.LeftController => _leftController;
        IController IProvider.RightController => _rightController;

        float[] IProvider.AvailableRefreshRates => OVRPlugin.systemDisplayFrequenciesAvailable;

        float IProvider.RefreshRate
        {
            get => OVRPlugin.systemDisplayFrequency;
            set => OVRPlugin.systemDisplayFrequency = value;
        }

        bool IProvider.HasApplicationFocus => _hasApplicationFocus;

        event Action? IProvider.ApplicationFocusAcquired
        {
            add => _applicationFocusAcquiredDelegate += value;
            remove => _applicationFocusAcquiredDelegate -= value;
        }

        event Action? IProvider.ApplicationFocusLost
        {
            add => _applicationFocusLostDelegate += value;
            remove => _applicationFocusLostDelegate -= value;
        }

        void IProvider.Update(float deltaTime)
        {
            // Update manager
            _manager.Update();

            // Headset focus check
            var tmpBool = OVRPlugin.hasInputFocus;
            if (_hasApplicationFocus != tmpBool)
            {
                _hasApplicationFocus = tmpBool;
                if (_hasApplicationFocus)
                {
                    _applicationFocusAcquiredDelegate?.Invoke();
                }
                else
                {
                    _applicationFocusLostDelegate?.Invoke();
                }
            }

            // Update Devices
            _headset.Update();
            _leftController.Update();
            _rightController.Update();
        }

        /// <summary>
        /// Meta Quest 2 を使用している場合のコントローラーのセットアップ
        /// Note: 2022-11 現在 Meta Quest 2 は以下のコントローラーに対応しています
        /// - Oculus Quest Touch Controller
        /// - Meta Quest Pro Touch Controller
        /// Note: 2022-11 現在 InteractionProfile は Standalone じゃないと取得できません
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private static void SetupQuest2Controller(out OvrControllerQuestTouch leftController,
            out OvrControllerQuestTouch rightController)
        {
            // 左手
            {
                var profile = OVRPlugin.GetCurrentInteractionProfile(OVRPlugin.Hand.HandLeft);
                leftController = profile switch
                {
                    // Oculus Quest Touch Controller
                    OVRPlugin.InteractionProfile.Touch => new OvrControllerQuestTouch(ControllerDomain.Left),

                    // Meta Quest Pro Touch Controller
                    OVRPlugin.InteractionProfile.TouchPro => new OvrControllerQuestProTouch(ControllerDomain.Left),

                    // Quest 3 Controller ToDo: (南) 実機で未検証 / 専用のクラス作成していません
                    OVRPlugin.InteractionProfile.TouchPlus => new OvrControllerQuestTouch(ControllerDomain.Left),

                    // Invalid controller
                    OVRPlugin.InteractionProfile.None => throw new NotImplementedException(),
                    _ => throw new NotImplementedException()
                };

                Debug.Log($"[QuestProvider] Detected left controller: {profile}");
            }

            // 右手
            {
                var profile = OVRPlugin.GetCurrentInteractionProfile(OVRPlugin.Hand.HandRight);
                rightController = profile switch
                {
                    // Oculus Quest Touch Controller
                    OVRPlugin.InteractionProfile.Touch => new OvrControllerQuestTouch(ControllerDomain.Right),

                    // Meta Quest Pro Touch Controller
                    OVRPlugin.InteractionProfile.TouchPro => new OvrControllerQuestProTouch(ControllerDomain.Right),

                    // Quest 3 Controller ToDo: (南) 実機で未検証 / 専用のクラス作成していません
                    OVRPlugin.InteractionProfile.TouchPlus => new OvrControllerQuestTouch(ControllerDomain.Right),

                    // Invalid controller
                    OVRPlugin.InteractionProfile.None => throw new NotImplementedException(),
                    _ => throw new NotImplementedException()
                };

                Debug.Log($"[QuestProvider] Detected right controller: {profile}");
            }
        }
    }
}