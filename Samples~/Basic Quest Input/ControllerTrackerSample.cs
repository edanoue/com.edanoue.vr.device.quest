#nullable enable

using Edanoue.VR.Device.Core;
using Edanoue.VR.Device.Quest;
using UnityEngine;
using UnityEngine.UI;

public class ControllerTrackerSample : MonoBehaviour
{
    [SerializeField] private bool isRightController;
    [SerializeField] private Text? debugText;
    private IController? _controller;

    private void Start()
    {
        IProvider provider = new QuestProvider();
        _controller = isRightController ? provider.RightController : provider.LeftController;

        // コールバックを追加するサンプル
        _controller.PressedPrimary += OnPressedPrimary;
    }

    // Update is called once per frame
    private void Update()
    {
        if (_controller == null) return;

        if (_controller is IUpdatable u) u.Update(Time.deltaTime);

        // Update this gameobject transforms
        var pos = _controller.Position;
        var rot = _controller.Rotation;
        transform.localPosition = new Vector3(pos.X, pos.Y, pos.Z);
        transform.localRotation = new Quaternion(rot.X, rot.Y, rot.Z, rot.W);

        // If specified debug gui text component
        if (debugText != null)
        {
            var msg = "";

            var side = isRightController ? "Right" : "Left";
            msg += $"Name: Controller {side}\n";
            msg += $"Connected: {_controller.IsConnected}\n";

            // コントローラーの入力を受け取るサンプル
            {
                // Primary Button
                msg += $"PressedPrimary: {_controller.IsPressedPrimary}\n";
                msg += $"TouchedPrimary: {_controller.IsTouchedPrimary}\n";

                // Touched ThumbRest
                msg += $"TouchedThumbRest: {_controller.IsTouchedThumbRest}\n";

                // Stick
                msg += $"Stick: {_controller.Stick.X:0.00}, {_controller.Stick.Y:0.00}\n";
            }

            // バッテリー情報にアクセス
            if (_controller is ISupportedBattery b) msg += $"Battery: {b.Battery}\n";

            // 速度情報にアクセス
            if (_controller is ISupportedVelocity v)
            {
                msg += $"Linear Vel: {v.LinearVelocity.X:0.00}, {v.LinearVelocity.Y:0.00}, {v.LinearVelocity.Z:0.00}" +
                       "\n";
                msg +=
                    $"Angular Vel: {v.AngularVelocity.X:0.00}, {v.AngularVelocity.Y:0.00}, {v.AngularVelocity.Z:0.00}" +
                    "\n";
            }

            // 加速度情報にアクセス
            if (_controller is ISupportedAcceleration a)
            {
                msg +=
                    $"Linear Acc: {a.LinearAcceleration.X:0.00}, {a.LinearAcceleration.Y:0.00}, {a.LinearAcceleration.Z:0.00}" +
                    "\n";
                msg +=
                    $"Angular Acc: {a.AngularAcceleration.X:0.00}, {a.AngularAcceleration.Y:0.00}, {a.AngularAcceleration.Z:0.00}" +
                    "\n";
            }

            debugText.text = msg;
        }
    }

    private void OnDestroy()
    {
        if (_controller != null) _controller.PressedPrimary -= OnPressedPrimary;
    }

    // Primary Button (A or B) Callback
    private void OnPressedPrimary(bool value)
    {
        var side = isRightController ? "Right" : "Left";
        Debug.Log($"({side}) Primary Pressed: {value}");
    }
}