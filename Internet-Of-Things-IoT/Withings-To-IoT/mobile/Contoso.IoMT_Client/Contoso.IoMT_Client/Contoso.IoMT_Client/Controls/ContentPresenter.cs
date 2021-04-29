// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Contoso.IoMT_Client.Controls
{
    using Xamarin.Forms;

    public class ContentPresenter : ContentView
    {
        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
            "ItemTemplate",
            typeof(DataTemplate),
            typeof(ContentPresenter),
            null,
            propertyChanged: OnItemTemplateChanged);

        public DataTemplate ItemTemplate
        {
            get
            {
                return (DataTemplate)GetValue(ItemTemplateProperty);
            }

            set
            {
                SetValue(ItemTemplateProperty, value);
            }
        }

        private static void OnItemTemplateChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            var cp = (ContentPresenter)bindable;

            var template = cp.ItemTemplate;
            if (template != null)
            {
                var content = (View)template.CreateContent();
                cp.Content = content;
            }
            else
            {
                cp.Content = null;
            }
        }
    }
}
