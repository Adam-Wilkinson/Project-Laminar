using OpenFlow_PluginFramework.Primitives.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals
{
    public static class VisualComponentExtensions
    {
        public static T WithFlowInput<T>(this T component, bool HasFlowInput = true) where T : IVisualNodeComponent
        {
            component.SetFlowInput(HasFlowInput);
            return component;
        }

        public static T WithFlowOutput<T>(this T component, bool HasFlowOutput = true) where T : IVisualNodeComponent
        {
            component.SetFlowOutput(HasFlowOutput);
            return component;
        }

        public static TComponent WithValue<TComponent, TValue>(this TComponent nodeField, string valueKey, TValue defaultValue, bool isUserEditable = false) where TComponent : INodeField
        {
            nodeField.AddValue(valueKey, Constructor.LaminarValue(FindValueManager(defaultValue), isUserEditable));
            return nodeField;
        }

        public static TComponent WithInput<TComponent, TValue>(this TComponent nodeField, TValue defaultValue) where TComponent : INodeField
            => nodeField.WithValue(INodeField.InputKey, defaultValue, true);

        public static TComponent WithOutput<TComponent, TValue>(this TComponent nodeField, TValue defaultValue) where TComponent : INodeField
            => nodeField.WithValue(INodeField.OutputKey, defaultValue, false);


        private static ITypeDefinitionProvider FindValueManager<TValue>(TValue defaultValue)
        {
            if (defaultValue is ITypeDefinitionProvider typeDefinitionProvider)
            {
                return typeDefinitionProvider;
            }

            if (defaultValue is ITypeDefinition typeDefinition)
            {
                return Constructor.ManualTypeDefinitionManager().WithAcceptedDefinition(typeDefinition);
            }

            return Constructor.ManualTypeDefinitionManager().WithAcceptedDefinition(defaultValue);
        }
    }
}
