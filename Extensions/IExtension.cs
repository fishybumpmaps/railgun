namespace Extensions
{
    public interface IExtension
    {
        // Gets called upon loading the extension, hooks should be registered from here
        void Init();

        // Gets called upon unloading the extension, the extension should close stuff like filestreams here
        void Kill();

        // Returns the name of the extension
        string Name();
    }
}
