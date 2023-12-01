using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace HiddenGems.Common
{
    public static class LicenseManager
    {
        static LicenseManager()
        {
        }

        public static bool IsLicenseValid(bool overrideExisting = false)
        {
            //Add your own licensing logic here
            return true;
        }

        public static bool IsCurrentVersion()
        {
            //Add your own logic for checking the version
            return true;
        }

        public static bool ResendActivationCode(string emailAddress)
        {
            //Add your own logic for resending an activation code to the user
            return true;
        }

        public static void PutLicense(string activationKey)
        {
            //Add your own logic for storing your license key locally
        }

        public static bool IsDemoMode()
        {
            //Add your own logic for determining whether the app is in demo mode or not
            //Demo mode has limited capabilities, and is intended for limited live demo purposes on costly hosted equipment
            return false;
        }
    }
}
