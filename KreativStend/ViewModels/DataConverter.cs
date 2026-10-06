namespace KreativStend.ViewModels
{
    public static class DataConverter
    {
        public static bool ToBool(this ushort source) =>
            source != 0;

        public static bool[] ToBoolArray(this ushort[] source) =>
            source.Select(q => q.ToBool()).ToArray();
    }
}
