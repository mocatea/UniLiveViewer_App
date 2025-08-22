using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using VContainer;

namespace UniLiveViewer.Player
{
    public static class GraphicsExtensions
    {
        public static MSAASamples ToMSAALevelFromSlider(this int value)
        {
            if (value == 0) return MSAASamples.None;
            if (value == 1) return MSAASamples.MSAA2x;
            if (value == 2) return MSAASamples.MSAA4x;
            if (value == 3) return MSAASamples.MSAA8x;
            return MSAASamples.None;
        }

        public static int ToMSAASliderValue(this MSAASamples value)
        {
            if (value == MSAASamples.None) return 0;
            if (value == MSAASamples.MSAA2x) return 1;
            if (value == MSAASamples.MSAA4x) return 2;
            if (value == MSAASamples.MSAA8x) return 3;
            return 0;
        }

        public static string AsString(this AntialiasingMode mode)
        {
            if (mode == AntialiasingMode.None) return "None";
            if (mode == AntialiasingMode.FastApproximateAntialiasing) return "FXAA";
            if (mode == AntialiasingMode.SubpixelMorphologicalAntiAliasing) return "SMAA";
            return "";
        }

        public static string AsString(this MSAASamples value)
        {
            if (value == MSAASamples.None) return "None";
            if (value == MSAASamples.MSAA2x) return "x2";
            if (value == MSAASamples.MSAA4x) return "x4";
            if (value == MSAASamples.MSAA8x) return "x8";
            return "";
        }
    }
}