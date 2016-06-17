namespace Railgun
{
    public class User
    {
        public int id = 0;
        public string Username = "";
        public string Colour = "";

        public int Hierarchy = 0;
        public bool ActiveUser = false;
        public bool CanBan = false;
        public bool CanSilence = false;
        public bool CreateChannel = false;
        public bool PermanentChannel = false;
        public bool ChangeUsername = false;

        public User(int id, string username, string colour, bool active = false)
        {
            this.id = id;
            Username = username;
            Colour = colour;
            ActiveUser = active;
        }

        public void SetPermissions(string rawperms)
        {
            string[] perms = rawperms.Split(' ');

            Hierarchy = int.Parse(perms[0]);
            CanBan = perms[1] == "1";
            CanSilence = perms[1] == "1";
            ChangeUsername = perms[3] == "1";
            CreateChannel = int.Parse(perms[4]) > 0;
            PermanentChannel = int.Parse(perms[4]) > 1;
        }
    }
}
