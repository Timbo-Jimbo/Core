using UnityEngine;

namespace TimboJimbo.Core
{
    public enum ColorInterpolationMode
    {
        [InspectorName("RGB")]
        RGB,
        [InspectorName("HSV")]
        HSV,
        [InspectorName("OkLab")]
        OkLab,
        [InspectorName("OkLCh")]
        OkLCh,
    }
}