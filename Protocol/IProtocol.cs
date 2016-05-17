namespace Protocol
{
    public interface IProtocol
    {
        // Returns a unique name to identify this protocol by (namely in the config)
        string Name();

        // Returns the connection state
        int State();

        // Open the connection with the server
        void Open();

        // Closes the connection
        void Close();

        // Sends a message to the server
        void SendMessage(string text);
    }
}
