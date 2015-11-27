using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public static class Users
    {
        public static List<User> Online;

        public static User Add(int id, string userName, string colour, string permissions)
        {
            try {
                return Get(id);
            } catch{}
            User subject = new User(id, userName, colour, permissions);
            Online.Add(subject);
            return subject;
        }

        public static User Remove(int id)
        {
            User subject = Get(id);
            Online.Remove(subject);
            return subject;
        }

        public static User Get(int id)
        {
            User user = Online.Single(get => get.id == id);
            return user;
        }

        public static User Get(string name)
        {
            User user = Online.Single(get => get.userName == name);
            return user;
        }

        public static User Update(int id, string userName, string colour, string permissions)
        {
            User user = Get(id);
            user.userName = userName;
            user.colour = colour;
            user.permissions = permissions;
            return user;
        }

        public static void Init()
        {
            Online = new List<User>();
        }
    }
}
