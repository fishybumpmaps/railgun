namespace Railgun.Protocols
{
    public interface IProtocol
    {
        // Returns the connection state
        int State();
        
        // Sends a message to the server
        void SendMessage(string text);
    }
}
