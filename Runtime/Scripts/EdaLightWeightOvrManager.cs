// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;
#if UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEditor;
#endif

namespace Edanoue.VR.Device.Quest
{
    using Settings = XRSettings;
    using Node = XRNode;

    /// <summary>
    /// Lightweight OVRManager
    /// </summary>
    internal sealed class EdaLightWeightOvrManager
    {
        /// <summary>
        /// </summary>
        private static bool _managerInitialized;

        internal EdaLightWeightOvrManager()
        {
            InitManager();
        }

        internal void Update()
        {
            //If we're using the XR SDK and the display subsystem is present, and OVRPlugin is initialized, we can init OVRManager
            InitManager();

            if (OVRPlugin.shouldQuit)
            {
                Debug.Log($"[{nameof(EdaLightWeightOvrManager)}] {nameof(OVRPlugin.shouldQuit)} detected");

#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }

#if UNITY_EDITOR
            if (Application.isBatchMode)
            {
                OVRPlugin.UpdateInBatchMode();
            }
#endif
        }


#if UNITY_EDITOR
        [MonoPInvokeCallback(typeof(OVRPlugin.LogCallback2DelegateType))]
        private static void OvrPluginLogCallback(OVRPlugin.LogLevel logLevel, IntPtr message, int size)
        {
            var logString = Marshal.PtrToStringAnsi(message, size);
            if (logLevel <= OVRPlugin.LogLevel.Info)
            {
                Debug.Log("[OVRPlugin] " + logString);
            }
            else
            {
                Debug.LogWarning("[OVRPlugin] " + logString);
            }
        }
#endif

        private void InitManager()
        {
            // Already initialized Manager
            if (_managerInitialized)
            {
                return;
            }

            if (!OVRPlugin.initialized)
            {
                Debug.Log(
                    $"[{nameof(EdaLightWeightOvrManager)}] OVRPlugin not initialized, skip {nameof(InitManager)}().");
                return;
            }

            // Create new marker
            const int markerId = 163069401;
            OVRPlugin.Qpl.MarkerStart(markerId);

            // Add version annotation to the marker
            var version = OVRPlugin.version.ToString();
            OVRPlugin.Qpl.MarkerAnnotation(markerId, "sdk_version", version);

            OVRPlugin.Qpl.CreateMarkerHandle("InitPermissionRequest", out var nameHandle);
            OVRPlugin.Qpl.MarkerPointCached(markerId, nameHandle);

            // end marker
            OVRPlugin.Qpl.MarkerEnd(markerId);

#if UNITY_ANDROID && !UNITY_EDITOR
            bool mediaInitialized = OVRPlugin.Media.Initialize();
            Debug.Log(mediaInitialized ? "OVRPlugin.Media initialized" : "OVRPlugin.Media not initialized");
            if (mediaInitialized)
            {
                var audioConfig = AudioSettings.GetConfiguration();
                if (audioConfig.sampleRate > 0)
                {
                    OVRPlugin.Media.SetMrcAudioSampleRate(audioConfig.sampleRate);
                    Debug.LogFormat("[MRC] SetMrcAudioSampleRate({0})", audioConfig.sampleRate);
                }

                OVRPlugin.Media.SetMrcInputVideoBufferType(OVRPlugin.Media.InputVideoBufferType.TextureHandle);
                Debug.LogFormat("[MRC] Active InputVideoBufferType:{0}", OVRPlugin.Media.GetMrcInputVideoBufferType());

                {
                    OVRPlugin.Media.SetMrcActivationMode(OVRPlugin.Media.MrcActivationMode.Disabled);
                    Debug.LogFormat("[MRC] ActivateMode: Disabled");
                }

                if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan)
                {
                    OVRPlugin.Media.SetAvailableQueueIndexVulkan(1);
                    OVRPlugin.Media.SetMrcFrameImageFlipped(true);
                }
            }
#endif

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            var supportedTypes =
                GraphicsDeviceType.Direct3D11 + ", " +
                GraphicsDeviceType.Direct3D12;

            if (!supportedTypes.Contains(SystemInfo.graphicsDeviceType.ToString()))
            {
                Debug.LogWarning("VR rendering requires one of the following device types: (" + supportedTypes +
                                 "). Your graphics device: " + SystemInfo.graphicsDeviceType);
            }
#endif

#if UNITY_EDITOR
            OVRPlugin.SetLogCallback2(OvrPluginLogCallback);
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
            // Turn off chromatic aberration by default to save texture bandwidth.
            OVRPlugin.chromatic = false;
#endif

#if DEVELOPMENT_BUILD && !UNITY_EDITOR
            OVRPlugin.SetDeveloperMode(OVRPlugin.Bool.True);
#endif

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            // Force Occlusion Mesh on all the time, you can change the value to false if you really need it be off for some reasons,
            // be aware there are performance drops if you don't use occlusionMesh.
            OVRPlugin.occlusionMesh = true;
#endif

            // Set the eye buffer sharpen type at the start
            OVRPlugin.SetEyeBufferSharpenType(OVRPlugin.LayerSharpenType.None);

            // Activate features
            OVRPlugin.position = true;
            OVRPlugin.rotation = true;
            OVRPlugin.useIPDInPositionTracking = true;
            OVRPlugin.SetTrackingOriginType(OVRPlugin.TrackingOrigin.FloorLevel);

            Debug.Log($"[{nameof(EdaLightWeightOvrManager)}] Initialized.");
            _managerInitialized = true;
        }

        ~EdaLightWeightOvrManager()
        {
#if UNITY_EDITOR
            OVRPlugin.SetLogCallback2(null);
#endif
        }
    }
}