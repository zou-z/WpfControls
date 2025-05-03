using System.Collections.Immutable;
using System.Windows;

namespace ColorPicker.Util
{
    internal abstract class DependencyPropertyValueAggregator<T>
    {
        public event Action<DependencyPropertyValueAggregator<T>>? AggregateValueChanged;

        public DependencyPropertyValueAggregator(KeyValuePair<DependencyObject, DependencyProperty>[] properties)
        {
            proxies = [.. properties.Select(t => new DependencyPropertyValueProxy(t.Key, t.Value, HandlePropertyValueChanged))];
        }

        public T? GetAggregateValue() => aggregateValue;

        public void SetAggregateValue(T? value, bool isForceUpdateProperties = false)
        {
            var isEqual = EqualityComparer<T>.Default.Equals(aggregateValue, value);
            if (!isEqual)
            {
                aggregateValue = value;
            }

            if (!isEqual || isForceUpdateProperties)
            {
                OnUpdatePropertiesValue();
            }
        }

        protected TPropertyType? GetPropertyValue<TPropertyType>(DependencyObject dependencyObject, DependencyProperty dependencyProperty)
        {
            var proxy = FindPropertyValueProxy(dependencyObject, dependencyProperty);
            return proxy?.Value is TPropertyType value ? value : default;
        }

        protected void SetPropertyValue<TPropertyType>(
            DependencyObject dependencyObject,
            DependencyProperty dependencyProperty,
            TPropertyType? value)
        {
            var proxy = FindPropertyValueProxy(dependencyObject, dependencyProperty);
            if (proxy != null)
            {
                proxy.Value = value;
            }
        }

        protected abstract void OnUpdatePropertiesValue();

        protected abstract void OnUpdateAggregateValue(
            DependencyObject dependencyObject,
            DependencyProperty dependencyProperty,
            object? propertyValue,
            Action<T> updateCallback
        );

        private void HandlePropertyValueChanged(DependencyPropertyValueProxy sender, object? propertyValue)
        {
            OnUpdateAggregateValue(
                sender.DependencyObject,
                sender.DependencyProperty,
                propertyValue,
                newAggregateValue => aggregateValue = newAggregateValue
            );
            AggregateValueChanged?.Invoke(this);
        }

        private DependencyPropertyValueProxy? FindPropertyValueProxy(
            DependencyObject dependencyObject,
            DependencyProperty dependencyProperty)
            => proxies.FirstOrDefault(
                t => t.DependencyObject == dependencyObject &&
                     t.DependencyProperty == dependencyProperty);

        private readonly ImmutableArray<DependencyPropertyValueProxy> proxies;
        private T? aggregateValue = default;
    }
}
