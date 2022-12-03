#nullable enable
using Edanoue.VR.Device.Core;

namespace Edanoue.VR.Device.Quest
{
    /// <summary>
    ///     Quest device provider class.
    ///     Currently use OVR (OVRP) Plugin.
    /// </summary>
    public class QuestProvider : IProvider
    {
        private readonly QuestOVRHeadset _headset;
        private readonly QuestOvrControllerLeft _leftController;
        private readonly QuestOvrControllerRight _rightController;

        public QuestProvider()
        {
            // 南: OVR (OVRP) を利用した実装 のもので初期化しています
            _headset = new QuestOVRHeadset();
            _leftController = new QuestOvrControllerLeft();
            _rightController = new QuestOvrControllerRight();
        }

        IHeadset IProvider.Headset => _headset;
        IController IProvider.LeftController => _leftController;
        IController IProvider.RightController => _rightController;
    }
}