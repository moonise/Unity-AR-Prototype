using ZXing;
using ZXing.Common;
using UnityEngine;
using Unity.Collections;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;
using BitmapFormat = ZXing.RGBLuminanceSource.BitmapFormat;
[RequireComponent(typeof(ARCameraManager))]
public class QRScanner : MonoBehaviour
{
   public event Action<string> OnQrFound;

    ARCameraManager camManager;
    IBarcodeReaderGeneric reader;
    bool isScanning = true;

    void Awake()
    {
        camManager = GetComponent<ARCameraManager>();

        reader = new BarcodeReaderGeneric
        {
            AutoRotate = true,
            Options = new DecodingOptions
            {
                TryHarder = true,
                PossibleFormats = new[] { BarcodeFormat.QR_CODE }
            }
        };
    }

    void OnEnable()  => camManager.frameReceived += OnFrame;
    void OnDisable() => camManager.frameReceived -= OnFrame;

    void OnFrame(ARCameraFrameEventArgs _)
    {
        if (!isScanning) return;
        if (!camManager.TryAcquireLatestCpuImage(out var image)) return;

        using (image)
        {
            var conv = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, image.width, image.height),
                outputDimensions = new Vector2Int(image.width / 2, image.height / 2), // smaller = faster
                outputFormat = TextureFormat.RGBA32,
                transformation = XRCpuImage.Transformation.MirrorX
            };

            int size = image.GetConvertedDataSize(conv);
            var buffer = new NativeArray<byte>(size, Allocator.Temp);
            image.Convert(conv, buffer);

            // ZXing expects raw bytes + width + height + pixel format
            var res = reader.Decode(
                buffer.ToArray(),
                conv.outputDimensions.x,
                conv.outputDimensions.y,
                BitmapFormat.RGBA32
            );

            buffer.Dispose();

            if (res != null && !string.IsNullOrEmpty(res.Text))
            {
                isScanning = false; // pause after first hit
                Debug.Log("QR: " + res.Text);
                OnQrFound?.Invoke(res.Text);
            }
        }
    }

    public void ResumeScanning() => isScanning = true;
}