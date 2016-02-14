namespace Extensions
{
    public interface IExtension
    {
        string Name { get; }
        void Initialise();
        void Destruct();
    }
}
