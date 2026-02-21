namespace KeywordDriven.Config
{
    public class DriverSetting
    {
        internal static string _devicename;
        internal static string _udid;
        internal static string _platformversion;
        internal static string _apppath;

        internal static string _drivertype = "local";
        internal static double _timeout = 20;
        internal static double _navigationtimeout = 200;
        internal static bool _headless = false;

        public static void WebDriver(string drivertype, double timeout, double navigationtimeout, bool headless)
        {
            _drivertype = drivertype;
            _timeout = timeout;
            _navigationtimeout = navigationtimeout;
            _headless = headless;

        }
        
        public static void AndroidDriver(string devicename, string udid, string platformversion, string apppath)
        {
            _devicename = devicename;
            _udid = udid;
            _platformversion = platformversion;
            _apppath = apppath;
        }
    }
}
