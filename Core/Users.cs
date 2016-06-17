using System.Collections.Generic;
using System.Linq;

namespace Railgun
{
    public static class Users
    {
        private static Dictionary<int, User> UserStorage = new Dictionary<int, User>();

        public static User Add(User user)
        {
            User existing = Get(user.id);

            if (existing != null)
            {
                return existing;
            }

            UserStorage.Add(user.id, user);

            return user;
        }

        public static User Remove(User user)
        {
            UserStorage.Remove(user.id);

            return user;
        }

        public static User Get(int id)
        {
            User user = null;

            if (UserStorage.ContainsKey(id))
            {
                user = UserStorage[id];
            }

            return user;
        }

        public static User Get(string name)
        {
            User user = null;

            try
            {
                user = UserStorage.Single(get => get.Value.Username == name).Value;
            }
            catch { }

            return user;
        }

        public static User Update(int id, User user)
        {
            if (!UserStorage.ContainsKey(id))
            {
                return null;
            }

            UserStorage[id] = user;

            return user;
        }

        public static void Clear()
        {
            UserStorage.Clear();
        }
    }
}
