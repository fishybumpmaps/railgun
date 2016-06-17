using Railgun;
using System;
using System.Net;

namespace FiiAuth
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Write(LogLevels.INFO, "FiiAuth", "Flashii.net Authentication Helper");
            Config.Init();

            bool loginDone = false;

            do {
                string[] auth = StartAuth();
                string[] loginAttempt = Login(auth[0], auth[1]);
                Console.WriteLine(auth[0]);
                Console.WriteLine(auth[1]);

                switch (int.Parse(loginAttempt[2]))
                {
                    case 4:
                        Log.Write(LogLevels.WARNING, "FiiAuth", "Incorrect password.");
                        break;

                    case 3:
                        Log.Write(LogLevels.WARNING, "FiiAuth", "Your account is restricted.");
                        break;

                    case 2:
                        Log.Write(LogLevels.WARNING, "FiiAuth", "User doesn't exist.");
                        break;

                    case 1:
                        Log.Write(LogLevels.INFO, "FiiAuth", "Login successful!");

                        // Attempt to save credentials
                        Config.Write("Auth", "arg1", loginAttempt[0]);
                        Config.Write("Auth", "arg2", loginAttempt[1]);
                        return;

                    case 0:
                        Log.Write(LogLevels.ERROR, "FiiAuth", "Empty result?");
                        break;

                    default:
                        Log.Write(LogLevels.ERROR, "FiiAuth", "Something happened.");
                        break;
                }
            } while (!loginDone);

            Log.Write(LogLevels.INFO, "FiiAuth", "Railgun.exe should now be able to authenticate with Flashii Chat.");
        }

        static string[] StartAuth()
        {
            string[] result = new string[2];

            Console.Write("Username: ");
            result[0] = Console.ReadLine();

            Console.Write("Password: ");
            result[1] = Console.ReadLine();

            return result;
        }

        static string[] Login(string userName, string password)
        {
            // Set parameters
            string postLocation = "//flashii.net/web/login.php";
            string postParams = "u=" + userName + "&p=" + password + "&z=meow";
            string result = "";
            bool success = true;

            // HTTPS attempt
            using (WebClient wc = new WebClient())
            {
                // Set headers
                wc.Headers["User-Agent"] = "Railgun Flashii Authentication Helper";
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
                    wc.Headers["User-Agent"] = "Railgun Flashii Authentication Helper";
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
                Log.Write(LogLevels.ERROR, "FiiAuth", "Check your internet connection, both https and http failed to connect.");
            }

            // Check if we got anything back
            if (result.Length > 0)
            {
                // Split the result
                return result.Split('|');
            }
            else
            {
                string[] end = new string[3];
                end[0] = "0";
                end[1] = "0";
                end[2] = "0";
                return end;
            }
        }
    }
}
