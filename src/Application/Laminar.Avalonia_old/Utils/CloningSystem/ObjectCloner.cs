namespace Laminar.Avalonia.Utils.CloningSystem;

public abstract class ObjectCloner<TClonee, TCloner> : IObjectCloner
    where TCloner : IObjectClonerBase<TClonee>, new()
{
    /*
    static ObjectCloner()
    {
        Cloner.Cloners.Add(typeof(TClonee), new TCloner());
    }
    */

    public abstract object Clone(object toClone);
}
