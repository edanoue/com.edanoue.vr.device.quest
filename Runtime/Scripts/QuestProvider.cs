#nullable enable
using Edanoue.VR.Device.Core;

namespace Edanoue.VR.Device.Quest
{
    public class QuestProvider : IProvider
    {
        private readonly QuestOVRHeadset _headset;

        public QuestProvider()
        {
            _headset = new QuestOVRHeadset();
            LeftController = new QuestOvrControllerLeft();
            RightController = new QuestOvrControllerRight();
        }

        public QuestOvrControllerLeft LeftController { get; }
        public QuestOvrControllerRight RightController { get; }


        string IProvider.FamilyName => "Quest";
        string IProvider.ProductName => "Quest";
        string IProvider.Version => "0.1.0";

        // IProvider impls

        #region IProvider impls

        IHeadset IProvider.Headset => _headset;
        IController IProvider.LeftController => LeftController;
        IController IProvider.RightController => RightController;

        #endregion
    }
}