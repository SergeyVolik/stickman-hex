namespace Prototype
{
    public interface ISaveable<T>
    {
        public T Save();
        public void Load(T data);
    }
}