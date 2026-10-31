namespace KeywordDriven.Config
{
    public static class DriverSetting
    {
        public static string DriverType { get; set; } = "local";
        public static bool Headless { get; set; } = false;

        public static int DefaultTimeoutSeconds { get; set; } = 50;
        public static int PageLoadTimeoutSeconds { get; set; } = 60;
    }
}
