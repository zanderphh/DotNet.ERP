﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Pharos.Wpf.Controls
{
    public class IconPasswordBox : Control
    {
        static IconPasswordBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconPasswordBox), new FrameworkPropertyMetadata(typeof(IconPasswordBox)));
        }
        public IconPasswordBox()
        {
        }

        void IconTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (DefualtEnter)
            {
                if (e.Key == Key.Enter)
                {
                    this.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
            }
        }

        public override void OnApplyTemplate()
        {
            PasswordBox passwordBox = GetTemplateChild("pwdbox") as PasswordBox;
            if (passwordBox != null)
            {
                passwordBox.PasswordChanged += passwordBox_PasswordChanged;
            }
            this.MouseDown += IconPasswordBox_MouseDown;
            this.GotKeyboardFocus += IconPasswordBox_GotKeyboardFocus;
            this.GotFocus += IconPasswordBox_GotFocus;
            passwordBox.PreviewKeyDown += IconTextBox_PreviewKeyDown;

            base.OnApplyTemplate();

        }

        void IconPasswordBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            PasswordBox passwordBox = GetTemplateChild("pwdbox") as PasswordBox;
            if (passwordBox != null)
                Keyboard.Focus(passwordBox);
        }

        void IconPasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = GetTemplateChild("pwdbox") as PasswordBox;
            if (passwordBox != null)
                Keyboard.Focus(passwordBox);
        }

        void IconPasswordBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PasswordBox passwordBox = GetTemplateChild("pwdbox") as PasswordBox;
            if (passwordBox != null)
                Keyboard.Focus(passwordBox);
        }

        void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var pwdBox = sender as PasswordBox;
            var obj = sender as DependencyObject;
            if (pwdBox != null)
            {
                Password = pwdBox.Password;
            }
        }


        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(IconPasswordBox));
        public static readonly DependencyProperty IconHeightProperty = DependencyProperty.Register("IconHeight", typeof(double), typeof(IconPasswordBox));
        public static readonly DependencyProperty IconWidthProperty = DependencyProperty.Register("IconWidth", typeof(double), typeof(IconPasswordBox));
        public static readonly DependencyProperty IconMarginProperty = DependencyProperty.Register("IconMargin", typeof(Thickness), typeof(IconPasswordBox));
        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register("Password", typeof(string), typeof(IconPasswordBox));
        public static readonly DependencyProperty DefualtEnterProperty = DependencyProperty.Register("DefualtEnter", typeof(bool), typeof(IconPasswordBox), new PropertyMetadata(true));
        public bool DefualtEnter
        {
            get { return (bool)GetValue(DefualtEnterProperty); }

            set { SetValue(DefualtEnterProperty, value); }
        }
        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }

            set { SetValue(PasswordProperty, value); }
        }
        public Thickness IconMargin
        {
            get { return (Thickness)GetValue(IconMarginProperty); }

            set { SetValue(IconMarginProperty, value); }
        }

        public double IconHeight
        {
            get { return (double)GetValue(IconHeightProperty); }

            set { SetValue(IconHeightProperty, value); }
        }
        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }

            set { SetValue(IconWidthProperty, value); }
        }

        public ImageSource Icon
        {

            get { return (ImageSource)GetValue(IconProperty); }

            set { SetValue(IconProperty, value); }

        }
    }
}