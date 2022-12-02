namespace BeanFramework.core.bean
{
    public interface Component
    {
        string Name { get { return ""; } }

        void Init();

        void Destroy();

        virtual int Order() { return 0; }
    }
}
