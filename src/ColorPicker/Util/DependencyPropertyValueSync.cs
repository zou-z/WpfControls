using System.Windows;

namespace ColorPicker.Util
{
    internal class DependencyPropertyValueSync<T>
    {
        public DependencyPropertyValueSync(
            DependencyObject dependencyObject,
            DependencyProperty dependencyProperty,
            DependencyPropertyValueAggregator<T>[] aggregators,
            T? defaultValue)
        {
            this.aggregators = aggregators;

            Array.ForEach(aggregators, t =>
            {
                t.SetAggregateValue(defaultValue, true);
                t.AggregateValueChanged += HandleAggregateValueChanged;
            });

            proxy = new DependencyPropertyValueProxy(
                dependencyObject,
                dependencyProperty,
                HandleDependencyPropertyValueChanged
            );
        }

        private void HandleDependencyPropertyValueChanged(
            DependencyPropertyValueProxy sender,
            object? dependencyPropertyValue)
        {
            Array.ForEach(aggregators,
                t => t.SetAggregateValue(dependencyPropertyValue is T value ? value : default)
            );
        }

        private void HandleAggregateValueChanged(DependencyPropertyValueAggregator<T> sender)
        {
            var aggregateValue = sender.GetAggregateValue();

            foreach (var aggregator in aggregators)
            {
                if (!aggregator.Equals(sender))
                {
                    aggregator.SetAggregateValue(aggregateValue);
                }
            }

            proxy.Value = aggregateValue;
        }

        private readonly DependencyPropertyValueProxy proxy;
        private readonly DependencyPropertyValueAggregator<T>[] aggregators;
    }
}
