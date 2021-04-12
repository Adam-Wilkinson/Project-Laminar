using System.ComponentModel;

namespace OpenFlow_Core.Primitives
{
    public class Point : IPoint
    {
        private double _x;
        private double _y;

        public double X
        {
            get => _x;
            set
            {
                if (_x != value)
                {
                    _x = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(X)));
                }
            }
        }

        public double Y
        {
            get => _y;
            set
            {
                if (_y != value)
                {
                    _y = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Y)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
