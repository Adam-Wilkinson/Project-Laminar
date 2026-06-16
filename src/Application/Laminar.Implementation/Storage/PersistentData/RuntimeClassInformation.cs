using System.Linq.Expressions;

namespace Laminar.Implementation.Storage.PersistentData;

public class RuntimeClassInformation
{
    private static readonly Dictionary<Type, RuntimeClassInformation> WritableInfo = [];

    private readonly List<Constructor> _constructors = [];
    
    public static RuntimeClassInformation Get(Type type)
    {
        if (WritableInfo.TryGetValue(type, out var info))
        {
            return info;
        }

        var newInfo = new RuntimeClassInformation(type);
        WritableInfo.Add(type, newInfo);
        return newInfo;
    }

    private RuntimeClassInformation(Type type)
    {
        foreach (var constructor in type.GetConstructors())
        {
            bool constructorValid = true;
            List<string> parameterNames = [];
            List<Type> parameterTypes = [];
            List<ParameterExpression> parameterExpressions = [];
            var parameterInfos = constructor.GetParameters();
            foreach (var parameterInfo in parameterInfos)
            {
                if (parameterInfo.Name is null)
                {
                    constructorValid = false;
                    break;
                }
                
                parameterNames.Add(parameterInfo.Name);
                parameterTypes.Add(parameterInfo.ParameterType);
                parameterExpressions.Add(Expression.Parameter(parameterInfo.ParameterType, parameterInfo.Name));
            }

            if (!constructorValid)
            {
                continue;
            }
            
            NewExpression constructorExpression = Expression.New(constructor, parameterExpressions);
            var objectTarget = Expression.Convert(constructorExpression, typeof(object));
            Expression<Func<object[], object>> lambdaExpression = Expression.Lambda<Func<object[], object>>(objectTarget);
            Func<object[], object> createHeadersFunc = lambdaExpression.Compile();
            _constructors.Add(new Constructor(parameterNames.ToArray(), parameterTypes.ToArray(), createHeadersFunc));
        }
    }
    
    public IEnumerable<Constructor> Constructors() => _constructors;
    
    public record Constructor(string[] ParameterNames, Type[] ParameterTypes, Func<object[], object> Function);
}