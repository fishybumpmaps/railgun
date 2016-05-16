namespace Protocol
{
    public interface IProtocol
    {
        // Returns the connection state
        int State();

        // Sends the authentication command to the server
        void Authenticate(string[] args);

        // Sends a message to the server
        void SendMessage(string text);

        // Closes the connection
        void Close();
    }
}
