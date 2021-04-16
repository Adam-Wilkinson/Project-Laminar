namespace Laminar_PluginFramework.Primitives.TypeDefinition
{
    using System;

    /// <summary>
    /// Immmutable type that defines how a value can behave and how to display it
    /// </summary>
    public interface ITypeDefinition
    {
        public object this[string key] { get; }

        /// <summary>
        /// The c# type that this ITypeDefinition allows
        /// </summary>
        public Type ValueType { get; }

        /// <summary>
        /// The default value that this ITypeDefinition assigns
        /// </summary>
        public object DefaultValue { get; }

        /// <summary>
        /// The name of the editor UI the value will be displayed with
        /// </summary>
        public string EditorName { get; }

        /// <summary>
        /// The name of the display UI the value will be displayed with
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Checks that an object is a valid input for this ITypeDefinition
        /// </summary>
        /// <param name="inputValue">Input object to be checked</param>
        /// <param name="outputValue">Output object, with any constraints applied</param>
        /// <returns>True if inputValue is allowed, false if inputValue is not allowed</returns>
        public bool TryConstrainValue(object inputValue, out object outputValue);

        public bool CanAcceptValue(object value);
    }
}
