using System.ComponentModel;
using System.Windows;

namespace ColorPicker.Util
{
    internal class DependencyPropertyValueProxy
    {
        public DependencyPropertyValueProxy(
             DependencyObject dependencyObject,
             DependencyProperty dependencyProperty,
             Action<DependencyPropertyValueProxy, object?> valueChangedCallback)
        {
            this.valueChangedCallback = valueChangedCallback;
            DependencyObject = dependencyObject;
            DependencyProperty = dependencyProperty;
            descriptor = DependencyPropertyDescriptor.FromProperty(dependencyProperty, dependencyObject.GetType());
            AddValueChanged();
        }

        public DependencyObject DependencyObject { get; }

        public DependencyProperty DependencyProperty { get; }

        public object? Value
        {
            get => DependencyObject.GetValue(DependencyProperty);
            set
            {
                RemoveValueChanged();
                DependencyObject.SetCurrentValue(DependencyProperty, value);
                AddValueChanged();
            }
        }

        private void OnPropertyValueChanged(object? sender, EventArgs e)
        {
            valueChangedCallback?.Invoke(this, Value);
        }

        private void AddValueChanged()
        {
            descriptor.AddValueChanged(DependencyObject, OnPropertyValueChanged);
        }

        private void RemoveValueChanged()
        {
            descriptor.RemoveValueChanged(DependencyObject, OnPropertyValueChanged);
        }

        private readonly DependencyPropertyDescriptor descriptor;
        private readonly Action<DependencyPropertyValueProxy, object?> valueChangedCallback;
    }
}
