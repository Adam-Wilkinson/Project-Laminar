using Laminar.Domain.DataManagement;

namespace Laminar.Contracts.UserData;

public interface IPersistentDataValue
{
    public Type ValueType { get; }

    public object Value { get; set; }
    
    public object SerializedValue { get; set; }

    public Type SerializedType { get; }

    public void ResetToDefault();
    
    
}