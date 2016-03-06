using System;
using System.Windows.Forms;
using Core;
using System.IO;

namespace FiiAuth
{
    public partial class LoginWindow : Form
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            // Get contents of the boxes
            string userName = userNameBox.Text;
            string password = passwordBox.Text;
            // Show progress bar and disable the form
            statusBox.Text = "Logging in...";
            Enabled = false;
            // Do the login attempt
            var result = Program.login(userName, password);

            // Update status
            switch (result)
            {
                case 4:
                    statusBox.Text = "Incorrect password.";
                    break;

                case 3:
                    statusBox.Text = "Your account is restricted.";
                    break;

                case 2:
                    statusBox.Text = "User doesn't exist.";
                    break;

                case 1:
                    // On success close this form
                    statusBox.Text = "Login successful!";

                    // Attempt to save credentials
                    Config.Write("Auth", "arg1", Program.userId.ToString());
                    Config.Write("Auth", "arg2", Program.sessionId);

                    // Create a close interval timer
                    Timer timer = new Timer();
                    timer.Interval = 1000;
                    timer.Tick += new EventHandler(destroy);
                    timer.Start();
                    return;

                case 0:
                    statusBox.Text = "Empty result?";
                    break;

                default:
                    statusBox.Text = "Something happened.";
                    break;
            }

            Enabled = true;
        }

        void destroy(object sender, EventArgs e)
        {
            Close();
        }
    }
}
