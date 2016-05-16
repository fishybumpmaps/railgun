namespace Extensions
{
    public interface IExtension
    {
        string Name { get; }
        void Initialise();
        void Destruct();
        void Handle(string[] data);
    }
}
