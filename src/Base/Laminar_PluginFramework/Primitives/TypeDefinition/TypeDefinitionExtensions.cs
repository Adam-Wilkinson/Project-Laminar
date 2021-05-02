using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_PluginFramework.Primitives.TypeDefinition
{
    public static class TypeDefinitionExtensions
    {
        public static IManualTypeDefinitionProvider WithAcceptedDefinition(this IManualTypeDefinitionProvider manualTypeDefinitionManager, ITypeDefinition typeDefinition)
        {
            manualTypeDefinitionManager.RegisterTypeDefinition(typeDefinition);
            return manualTypeDefinitionManager;
        }

        public static IManualTypeDefinitionProvider WithAcceptedDefinition<T>(this IManualTypeDefinitionProvider manualTypeDefinitionManager, T defaultValue, string editorName = null, string displayName = null)
            => manualTypeDefinitionManager.WithAcceptedDefinition(Constructor.TypeDefinition(defaultValue, editorName, displayName));

        public static IManualTypeDefinitionProvider WithAcceptedDefinition<TValue>(this IManualTypeDefinitionProvider manualTypeDefinitionManager, ITypeDefinitionConstructor<TValue> typeDefinitionConstructor)
            => manualTypeDefinitionManager.WithAcceptedDefinition(typeDefinitionConstructor.Construct());

        public static ITypeDefinitionConstructor<T> WithProperty<T>(this ITypeDefinitionConstructor<T> typeDefinitionConstructor, string propertyName, object propertyValue)
        {
            typeDefinitionConstructor.AddProperty(propertyName, propertyValue);

            return typeDefinitionConstructor;
        }

        public static ITypeDefinitionConstructor<T> WithConstraint<T>(this ITypeDefinitionConstructor<T> typeDefinitionConstructor, Func<T, T> constraint)
        {
            IValueConstraint<T> valueConstraint = Constructor.ValueConstraint(constraint);

            typeDefinitionConstructor.AddConstraint(valueConstraint);

            return typeDefinitionConstructor;
        }

        public static ITypeDefinitionConstructor<T> WithConstraint<T>(this ITypeDefinitionConstructor<T> typeDefinitionConstructor, string constraintName, object constraintValue, Func<T, T> constraint)
            => typeDefinitionConstructor.WithProperty(constraintName, constraintValue).WithConstraint(constraint);

        public static ITypeDefinitionConstructor<double> WithUnits(this ITypeDefinitionConstructor<double> cons, string units) => cons.WithProperty("Units", units);
    }
}
