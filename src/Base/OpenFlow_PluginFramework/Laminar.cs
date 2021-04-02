using OpenFlow_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_PluginFramework
{
    public static class Laminar
    {
        private static bool initComplete = false;

        public static IObjectFactory Factory { get; private set; }

        public static bool Init(IObjectFactory factory)
        {
            if (!initComplete)
            {
                Factory = factory;
                initComplete = true;
                return true;
            }

            return false;
        }

        public static T New<T>()
        {
            return Factory.GetImplementation<T>();
        }
    }
}
