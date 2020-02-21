using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace RockChat.UWP.Common
{
    class OptionalDataTemplateSelector : DataTemplateSelector
    {
        private readonly DataTemplate _emptyTemplate = new DataTemplate();
        public Type VisibleType { get; set; }
        public DataTemplate DataTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item?.GetType() == VisibleType)
            {
                return DataTemplate;
            }

            return _emptyTemplate;
        }
    }
}