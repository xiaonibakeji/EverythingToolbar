﻿using EverythingToolbar.Helpers;
using EverythingToolbar.Properties;
using NHotkey;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace EverythingToolbar
{
    public partial class SearchWindow : MicaWindow
    {
        //public static Edge taskbarEdge;
        public static double taskbarHeight = 0;
        public static double taskbarWidth = 0;
        public bool IsOpen = false;
        
        public new double Height
        {
            get
            {
                return (double)GetValue(HeightProperty);
            }
            set
            {
                double screenHeight = SystemParameters.PrimaryScreenHeight;
                double newHeight = Math.Max(Math.Min(screenHeight - taskbarHeight, value), 300);
                SetValue(HeightProperty, newHeight);
            }
        }
        
        public new double Width
        {
            get
            {
                return (double)GetValue(WidthProperty);
            }
            set
            {
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double newWidth = Math.Max(Math.Min(screenWidth - taskbarWidth, value), 300);
                SetValue(WidthProperty, newWidth);
            }
        }

        public static readonly SearchWindow Instance = new SearchWindow();

        private SearchWindow()
        {
            InitializeComponent();
            DataContext = EverythingSearch.Instance;

            if (Settings.Default.isUpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.isUpgradeRequired = false;
                Settings.Default.Save();
            }
            Settings.Default.PropertyChanged += (s, e) => Settings.Default.Save();

            LostKeyboardFocus += OnLostKeyboardFocus;
            Activated += OnActivated;
            EventDispatcher.Instance.HideWindow += Hide;
            EventDispatcher.Instance.ShowWindow += Show;

            ResourceManager.Instance.ResourceChanged += (sender, e) => { Resources = e.NewResource; };
            ResourceManager.Instance.AutoApplyTheme();
        }

        private void OnActivated(object sender, EventArgs e)
        {
            SearchBox.Focus();
            NativeMethods.SetForegroundWindow(((HwndSource)PresentationSource.FromVisual(this)).Handle);
            EventDispatcher.Instance.InvokeFocusRequested(sender, e);
        }

        private void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (e.NewFocus == null)  // New focus outside application
            {
                Hide();
            }
        }

        private new void Hide()
        {
            IsOpen = false;
            HistoryManager.Instance.AddToHistory(EverythingSearch.Instance.SearchTerm);
            base.Hide();
        }

        private new void Show()
        {
            IsOpen = true;
            base.Show();

            if (!IsOpen)
                AnimateOpen();
        }

        public void Show(object sender, HotkeyEventArgs e)
        {
            Show();
        }

        private void AnimateOpen()
        {
            //Keyboard.Focus(SearchBox);

            //switch (taskbarEdge)
            //{
            //    case Edge.Top:
            //        Placement = PlacementMode.Bottom;
            //        PopupMarginBorder.Margin = new Thickness(12, 0, 12, 12);
            //        break;
            //    case Edge.Left:
            //        Placement = PlacementMode.Right;
            //        PopupMarginBorder.Margin = new Thickness(0, 12, 12, 12);
            //        break;
            //    case Edge.Right:
            //        Placement = PlacementMode.Left;
            //        PopupMarginBorder.Margin = new Thickness(12, 12, 0, 12);
            //        break;
            //    case Edge.Bottom:
            //        Placement = PlacementMode.Top;
            //        PopupMarginBorder.Margin = new Thickness(12, 12, 12, 0);
            //        break;
            //}

            Height = Settings.Default.popupSize.Height;
            Width = Settings.Default.popupSize.Width;

            QuinticEase ease = new QuinticEase
            {
                EasingMode = EasingMode.EaseOut
            };

            int modifier = 1;
            Duration duration = TimeSpan.FromSeconds(Settings.Default.isAnimationsDisabled ? 0 : 0.4);
            DoubleAnimation outer = new DoubleAnimation(modifier * 150, 0, duration)
            {
                EasingFunction = ease
            };
            DependencyProperty outerProp = TranslateTransform.YProperty;
            translateTransform?.BeginAnimation(outerProp, outer);

            DoubleAnimation opacity = new DoubleAnimation(0, 1, duration)
            {
                EasingFunction = ease
            };
            PopupMarginBorder?.BeginAnimation(OpacityProperty, opacity);

            duration = TimeSpan.FromSeconds(Settings.Default.isAnimationsDisabled ? 0 : 0.8);
            ThicknessAnimation inner = new ThicknessAnimation(new Thickness(0), duration)
            {
                EasingFunction = ease
            };
            //if (taskbarEdge == Edge.Top)
            //    inner.From = new Thickness(0, -50, 0, 50);
            //else if (taskbarEdge == Edge.Right)
            //    inner.From = new Thickness(50, 0, -50, 0);
            //else if (taskbarEdge == Edge.Bottom)
            inner.From = new Thickness(0, 50, 0, -50);
            //else if (taskbarEdge == Edge.Left)
            //    inner.From = new Thickness(-50, 0, 50, 0);
            ContentGrid?.BeginAnimation(MarginProperty, inner);
        }

        private void OnOpened(object sender, EventArgs e)
        {
            AnimateOpen();
        }

        private void OpenSearchInEverything(object sender, RoutedEventArgs e)
        {
            EverythingSearch.Instance.OpenLastSearchInEverything();
        }
    }
}
