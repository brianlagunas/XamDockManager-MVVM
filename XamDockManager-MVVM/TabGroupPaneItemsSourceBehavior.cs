using Infragistics.Windows.DockManager;
using Infragistics.Windows.DockManager.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interactivity;

namespace XamDockManager_MVVM
{
    public class TabGroupPaneItemsSourceBehavior : Behavior<TabGroupPane>
    {
        public static readonly DependencyProperty HeaderMemberPathProperty = DependencyProperty.Register("HeaderMemberPath", typeof(string), typeof(TabGroupPaneItemsSourceBehavior));
        public string HeaderMemberPath
        {
            get { return (string)GetValue(HeaderMemberPathProperty); }
            set { SetValue(HeaderMemberPathProperty, value); }
        }

        public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(TabGroupPaneItemsSourceBehavior), new PropertyMetadata(null));
        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IList), typeof(TabGroupPaneItemsSourceBehavior), new PropertyMetadata(null, new PropertyChangedCallback(OnItemsSourcePropertyChanged)));
        public IList ItemsSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TabGroupPaneItemsSourceBehavior behavior = d as TabGroupPaneItemsSourceBehavior;
            if (behavior != null)
                behavior.OnItemsSourcePropertyChanged((IList)e.OldValue, (IList)e.NewValue);
        }

        void OnItemsSourcePropertyChanged(IList oldValue, IList newValue)
        {
            AssociatedObject.Items.Clear();

            if (oldValue != null)
            {
                var oldCollectionChanged = oldValue as INotifyCollectionChanged;
                if (oldCollectionChanged != null)
                    oldCollectionChanged.CollectionChanged -= CollectionChanged_CollectionChanged;
            }

            if (newValue != null)
            {
                var collectionChanged = newValue as INotifyCollectionChanged;
                if (collectionChanged != null)
                    collectionChanged.CollectionChanged += CollectionChanged_CollectionChanged;

                foreach (var item in newValue)
                {
                    ContentPane contentPane = PrepareContainerForItem(item);
                    AssociatedObject.Items.Add(contentPane);
                }
            }
        }

        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(TabGroupPaneItemsSourceBehavior), new PropertyMetadata(null));
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        void CollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                IEnumerable<ContentPane> contentPanes = XamDockManager.GetDockManager(AssociatedObject).GetPanes(PaneNavigationOrder.VisibleOrder);
                foreach (ContentPane contentPane in contentPanes)
                {
                    var dc = contentPane.DataContext;
                    if (dc != null && e.OldItems.Contains(dc))
                    {
                        contentPane.ExecuteCommand(ContentPaneCommands.Close);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    ContentPane contentPane = PrepareContainerForItem(item);
                    if (contentPane != null)
                        AssociatedObject.Items.Add(contentPane);
                }
            }
        }

        protected ContentPane PrepareContainerForItem(object item)
        {
            ContentPane container = new ContentPane();

            container.Content = item;
            container.DataContext = item;

            if (HeaderTemplate != null)
                container.HeaderTemplate = HeaderTemplate;

            if (ItemTemplate != null)
                container.ContentTemplate = ItemTemplate;

            container.CloseAction = PaneCloseAction.RemovePane;
            container.Closed += Container_Closed;

            CreateBindings(item, container);

            return container;
        }

        private void Container_Closed(object sender, PaneClosedEventArgs e)
        {
            ContentPane contentPane = sender as ContentPane;
            if (contentPane != null)
            {
                contentPane.Closed -= Container_Closed; //no memory leaks

                var item = contentPane.DataContext;

                if (ItemsSource != null && ItemsSource.Contains(item))
                    ItemsSource.Remove(item);

                RemoveBindings(contentPane);
            }
        }

        private void CreateBindings(object item, ContentPane contentPane)
        {
            if (!String.IsNullOrWhiteSpace(HeaderMemberPath))
            {
                Binding headerBinding = new Binding(HeaderMemberPath);
                headerBinding.Source = item;
                contentPane.SetBinding(ContentPane.HeaderProperty, headerBinding);
            }
        }

        private void RemoveBindings(ContentPane contentPane)
        {
            contentPane.ClearValue(ContentPane.HeaderProperty);
        }
    }

}
