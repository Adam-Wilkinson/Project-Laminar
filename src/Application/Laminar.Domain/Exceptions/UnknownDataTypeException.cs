using Laminar.Domain.DataManagement;

namespace Laminar.Domain.Exceptions;

public class UnknownDataTypeException(PersistentDataType dataType) : Exception($"No saving type implemented for persistent data type {dataType}");
