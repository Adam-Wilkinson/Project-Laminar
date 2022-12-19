namespace Laminar.Avalonia.Utils.CloningSystem;

public interface IObjectClonerBase<in T> : IObjectCloner
{
    public object Clone(T toClone);
}
