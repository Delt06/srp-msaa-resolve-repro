using System;
using System.Diagnostics.CodeAnalysis;

[Serializable]
public struct CustomSrpSettings
{
    public MsaaSamples MsaaSamples;
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum MsaaSamples
{
    x1 = 1,
    x2 = 2,
    x4 = 4,
    x8 = 8,
}