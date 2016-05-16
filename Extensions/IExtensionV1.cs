namespace Extensions
{
    public interface IExtensionV1
    {
        string Name { get; }
        void Initialise();
        void Destruct();
        void Handle(string[] data);
    }
}
