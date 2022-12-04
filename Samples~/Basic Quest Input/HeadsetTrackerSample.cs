#nullable enable

using Edanoue.VR.Device.Core;
using Edanoue.VR.Device.Quest;
using UnityEngine;

public class HeadsetTrackerSample : MonoBehaviour
{
    private IHeadset? _headset;

    private void Start()
    {
        IProvider provider = new QuestProvider();
        _headset = provider.Headset;
    }

    // Update is called once per frame
    private void Update()
    {
        if (_headset == null) return;

        if (_headset is IUpdatable u) u.Update(Time.deltaTime);

        // Update Transforms
        var pos = _headset.Position;
        var rot = _headset.Rotation;
        transform.localPosition = new Vector3(pos.X, pos.Y, pos.Z);
        transform.localRotation = new Quaternion(rot.X, rot.Y, rot.Z, rot.W);
    }
}