using System;
using System.Net;
using System.Windows.Forms;

namespace FiiAuth
{
    static class Program
    {
        // Login data
        public static int userId = 0;
        public static string sessionId = "";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginWindow());
        }

        public static int login(string userName, string password)
        {
            // Set parameters
            string postLocation = "//flashii.net/spookyshit/login.php";
            string postParams = "u=" + userName + "&p=" + password + "&z=meow";
            string result = "";
            bool success = true;

            // HTTPS attempt
            using (WebClient wc = new WebClient())
            {
                // Set headers
                wc.Headers["User-Agent"] = "Flashii Chat Windows";
                wc.Headers["Content-Type"] = "application/x-www-form-urlencoded";
                // Make request
                try
                {
                    result = wc.UploadString("https:" + postLocation, postParams);
                }
                catch { success = false; }
            }

            // Attempt to do it again over HTTP if it failed the first time
            if (!success)
            {
                using (WebClient wc = new WebClient())
                {
                    // Set headers
                    wc.Headers["User-Agent"] = "Flashii Chat Windows (Unsecure fallback)";
                    wc.Headers["Content-Type"] = "application/x-www-form-urlencoded";
                    // Make request
                    try
                    {
                        result = wc.UploadString("http:" + postLocation, postParams);
                        success = true;
                    }
                    catch { success = false; }
                }
            }

            // If success is still false display an error and return false
            if (!success)
            {
                MessageBox.Show("Check your internet connection, both https and http failed to connect.");
            }

            // Check if we got anything back
            if (result.Length > 0)
            {
                // Split the result
                string[] res = result.Split('|');
                
                // Assign the variables
                userId = int.Parse(res[0]);
                sessionId = res[1];
                return int.Parse(res[2]);
            }
            else
            {
                return 0;
            }
        }
    }
}
