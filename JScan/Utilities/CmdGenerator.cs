namespace JScan.Utilities
{
    public class CmdGenerator
    {
        public static string ProcKill(string processName) => string.Format(getCmd("taskkill /im {0} /f"), processName);

        public static string WhoAmI() => string.Format(getCmd("whoami.exe"));

        public static string AllowTaskman(bool allow)
        {
            if (!allow)
                return baseBuild("BlockTaskMan");
            else
                return baseBuild("AllowTaskMan");
        }

        public static string getCmd(string args) => string.Format(@"startapp>{0}>""""", args);

        private static string baseBuild(string command, string add = "") => string.Format("{0}>{1}{2}{1}", command, "\"", add);

        public static string GetWMISignal(string Description, string WMIClass, string WMIProp, string WMIFilter)
                => string.Format("GetWMI>{0}>{1}>{2}>{3}", Description, WMIClass, WMIProp, WMIFilter);

        //return string.Format(@"startapp>taskkill /im {0} /f>""""", processName);
        //BlockTaskMan>""
        //AllowTaskMan>""
    }
}