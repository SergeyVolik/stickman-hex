namespace Prototype
{
    public static class TextUtils
    {
        public static string IntToText(int numberOfIntems)
        {
            var Thousand = numberOfIntems / 1000f;

            if (Thousand >= 1)
            {
                return $"{Thousand.ToString("0.0")}K";
            }

            return numberOfIntems.ToString();
        }
    }
}
