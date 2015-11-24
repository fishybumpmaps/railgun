namespace Core
{
    public class User
    {
        public int id { get; set; }
        public string userName { get; set; }
        public string colour { get; set; }
        public string permissions { get; set; }
        public User(int id, string userName, string colour, string permissions)
        {
            this.id = id;
            this.userName = userName;
            this.colour = colour;
            this.permissions = permissions;
        }
    }
}
