#nullable enable
namespace Edanoue.VR.Device.Quest
{
    internal enum HeadsetType
    {
        Unknown = 0,
        // Oculus Quest
        Quest = 1,
        // Oculus Quest 2
        Quest_2 = 2,
        // Meta Quest Pro
        Quest_Pro = 3
    }

    internal enum ControllerType
    {
        Unknown = 0,
        // Oculus Quest, Oculus Quest 2 controller
        Quest_Touch = 1,
        // Meta Quest Pro controller
        Quest_Touch_Pro = 2
    }
}