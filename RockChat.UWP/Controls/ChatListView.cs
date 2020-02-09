using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using RockChat.Core.Collection.Data;

namespace RockChat.UWP.Controls
{
    public interface IItemsSourceSelector
    {
        object ItemsSource { get; set; }
    }

    public interface IWithExtraDataSelector
    {
        object ExtraData { get; set; }
    }

    /// <summary>
    ///     This ListView is tailored to a Chat experience where the focus is on the last item in the list
    ///     and as the user scrolls up the older messages are incrementally loaded.  We're performing our
    ///     own logic to trigger loading more data.
    ///     //
    ///     Note: This is just delay loading the data, but isn't true data virtualization.  A user that
    ///     scrolls all the way to the beginning of the list will cause all the data to be loaded.
    /// </summary>
    public class ChatListView : ListView
    {
        public static readonly DependencyProperty ExtraDataProperty = DependencyProperty.Register(
            "ExtraData", typeof(object), typeof(ChatListView), new PropertyMetadata(default, PropertyChangedCallback));

        private double _averageContainerHeight;
        private uint _itemsSeen;
        private bool _processingScrollOffsets;
        private bool _processingScrollOffsetsDeferred;


        public ChatListView()
        {
            DefaultStyleKey = typeof(ListView);
            // We'll manually trigger the loading of data incrementally and buffer for 2 pages worth of data
            IncrementalLoadingTrigger = IncrementalLoadingTrigger.None;

            // Since we'll have variable sized items we compute a running average of height to help estimate
            // how much data to request for incremental loading
            ContainerContentChanging += UpdateRunningAverageContainerHeight;
            RegisterPropertyChangedCallback(ItemsSourceProperty, OnItemsSourceChanged);
        }

        public object ExtraData
        {
            get => GetValue(ExtraDataProperty);
            set => SetValue(ExtraDataProperty, value);
        }

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ChatListView view)
            {
                if (e.Property == ExtraDataProperty)
                {
                    view.OnExtraDataChanged(e.OldValue, e.NewValue);
                }
            }
        }

        private void OnExtraDataChanged(object oldValue, object newValue)
        {
            if (ItemTemplateSelector is IWithExtraDataSelector selector)
            {
                selector.ExtraData = newValue;
            }
        }

        private void OnItemsSourceChanged(DependencyObject sender, DependencyProperty dp)
        {
            _processingScrollOffsets = false;
            _processingScrollOffsetsDeferred = false;
            if (ItemTemplateSelector is IItemsSourceSelector itemsSourceSelector)
            {
                itemsSourceSelector.ItemsSource = ItemsSource;
            }
        }

        protected override void OnApplyTemplate()
        {
            var scrollViewer = GetTemplateChild("ScrollViewer") as ScrollViewer;

            if (scrollViewer != null)
            {
                scrollViewer.ViewChanged += (s, a) =>
                {
                    // Check if we should load more data when the scroll position changes.
                    // We only get this once the content/panel is large enough to be scrollable.
                    StartProcessingDataVirtualizationScrollOffsets(ActualHeight);
                };
            }

            base.OnApplyTemplate();
        }

        // We use ArrangeOverride to trigger incrementally loading data (if needed) when the panel is too small to be scrollable.
        protected override Size ArrangeOverride(Size finalSize)
        {
            // Allow the panel to arrange first
            var result = base.ArrangeOverride(finalSize);

            StartProcessingDataVirtualizationScrollOffsets(finalSize.Height);

            return result;
        }

        private async void StartProcessingDataVirtualizationScrollOffsets(double actualHeight)
        {
            // Avoid re-entrancy. If we are already processing, then defer this request.
            if (_processingScrollOffsets)
            {
                _processingScrollOffsetsDeferred = true;
                return;
            }

            _processingScrollOffsets = true;

            do
            {
                _processingScrollOffsetsDeferred = false;
                await ProcessDataVirtualizationScrollOffsetsAsync(actualHeight);

                // If a request to process scroll offsets occurred while we were processing
                // the previous request, then process the deferred request now.
            } while (_processingScrollOffsetsDeferred);

            // We have finished. Allow new requests to be processed.
            _processingScrollOffsets = false;
        }

        private async Task ProcessDataVirtualizationScrollOffsetsAsync(double actualHeight)
        {
            var panel = ItemsPanelRoot as ItemsStackPanel;
            if (panel != null)
            {
                if (panel.FirstVisibleIndex != -1 && panel.FirstVisibleIndex * _averageContainerHeight <
                    actualHeight * IncrementalLoadingThreshold ||
                    Items.Count == 0)
                {
                    var virtualizingDataSource = ItemsSource as ISupportIncrementalLoading;
                    if (virtualizingDataSource != null)
                    {
                        if (virtualizingDataSource.HasMoreItems)
                        {
                            uint itemsToLoad;
                            if (_averageContainerHeight == 0.0)
                            {
                                // We don't have any items yet. Load the first one so we can get an
                                // estimate of the height of one item, and then we can load the rest.
                                itemsToLoad = 1;
                            }
                            else
                            {
                                var avgItemsPerPage = actualHeight / _averageContainerHeight;
                                // We know there's data to be loaded so load at least one item
                                itemsToLoad = Math.Max((uint) (DataFetchSize * avgItemsPerPage), 1);
                            }

                            if (!virtualizingDataSource.IsLoading)
                            {
                                await virtualizingDataSource.LoadMoreItemsAsync(itemsToLoad);
                            }
                        }
                    }
                }
            }
        }

        private void UpdateRunningAverageContainerHeight(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.ItemContainer != null && !args.InRecycleQueue)
            {
                switch (args.Phase)
                {
                    case 0:
                        // use the size of the very first placeholder as a starting point until
                        // we've seen the first item
                        if (_averageContainerHeight == 0)
                        {
                            _averageContainerHeight = args.ItemContainer.DesiredSize.Height;
                        }

                        args.RegisterUpdateCallback(1, UpdateRunningAverageContainerHeight);
                        args.Handled = true;
                        break;

                    case 1:
                        // set the content
                        args.ItemContainer.Content = args.Item;
                        args.RegisterUpdateCallback(2, UpdateRunningAverageContainerHeight);
                        args.Handled = true;
                        break;

                    case 2:
                        // refine the estimate based on the item's DesiredSize
                        _averageContainerHeight =
                            (_averageContainerHeight * _itemsSeen + args.ItemContainer.DesiredSize.Height) /
                            ++_itemsSeen;
                        args.Handled = true;
                        break;
                }
            }
        }
    }
}