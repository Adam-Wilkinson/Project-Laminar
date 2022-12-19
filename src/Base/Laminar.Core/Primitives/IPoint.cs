using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Primitives
{
    public interface IPoint : INotifyPropertyChanged
    {
        double X { get; set; }

        double Y { get; set; }
    }
}
