namespace Core
{
    class Message
    {
        public static string Separator = "\t"; // Seperator used by sock chat
        
        // Message packer
        public static string Pack(int id, string parameters)
        {
            return id.ToString() + Separator + string.Join(Separator, parameters);
        }

        // String array edition
        public static string PackArray(string[] array)
        {
            return string.Join(Separator, array);
        }

        // Message unpacker
        public static string[] Unpack(string message)
        {
            string[] data = message.Split(Separator.ToCharArray());
            return data;
        }
    }
}
