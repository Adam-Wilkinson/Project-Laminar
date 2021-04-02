using OpenFlow_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core.Primitives
{
    /// <summary>
    /// Defines the opacity of a displayed object
    /// </summary>
    public class Opacity : IOpacity
    {
        private readonly List<IOpacity> _opacityFactors = new();
        private double _myValue = 1.0;
        private double _factorValue = 1.0;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The value of the opacity, a double between 0 and 1
        /// </summary>
        public double Value
        {
            get => _myValue * _factorValue;
            set
            {
                _myValue = value;
                ValueChanged();
            }
        }

        /// <summary>
        /// Adds another opacity as a factor to this opacity
        /// </summary>
        /// <param name="factor">The Opacity class that provides the factor</param>
        public void AddOpacityFactor(IOpacity factor)
        {
            factor.PropertyChanged += Factor_PropertyChanged;
            _opacityFactors.Add(factor);
            UpdateFactor();
        }

        /// <summary>
        /// Removes an opacity factor from this opacity
        /// </summary>
        /// <param name="factor">The factor to remove</param>
        public bool RemoveOpacityFactor(IOpacity factor)
        {
            if (_opacityFactors.Remove(factor))
            {
                factor.PropertyChanged -= Factor_PropertyChanged;
                UpdateFactor();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Run when a property of one of the factors is changed
        /// </summary>
        /// <param name="sender">The original opacity</param>
        /// <param name="e">The property that is changed</param>
        private void Factor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateFactor();
        }

        /// <summary>
        /// Updates the <see cref="_factorValue"/> variable to match 
        /// </summary>
        private void UpdateFactor()
        {
            _factorValue = 1.0;
            foreach (IOpacity factor in _opacityFactors)
            {
                _factorValue *= factor.Value;
            }
            ValueChanged();
        }

        /// <summary>
        /// Notify that the <see cref="Value"/> of this Opacity has been changed
        /// </summary>
        private void ValueChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }
    }
}
