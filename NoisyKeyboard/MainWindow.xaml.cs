using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace NoisyKeyboard
{
    

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool forceExit = false;
        
        public MainWindow()
        {
            InitializeComponent();
            var soundDirectories = Directory.EnumerateDirectories($"{Directory.GetCurrentDirectory()}\\data\\sounds");
            foreach(var folder in soundDirectories)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                var contentText = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(folder.Split('\\').Last().ToLower());
                
                comboBoxItem.Content = contentText;
                comboBoxItem.Tag = folder;
                comboboxSounds.Items.Add(comboBoxItem);
            }
            this.Left = System.Windows.SystemParameters.PrimaryScreenWidth - this.Width;
            this.Top = System.Windows.SystemParameters.PrimaryScreenHeight - this.Height - 30;
            comboboxSounds.SelectedIndex = 0;
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            if (!forceExit)
            {
                e.Cancel = true;
                this.WindowState = WindowState.Minimized;
            }
        }
        private void sliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            KeyboardListener.SetVolume(e.NewValue);
        }

        private void comboboxSounds_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            KeyboardListener.SetMusicFolder((String)((ComboBoxItem)comboboxSounds.SelectedItem).Tag);
        }

        private void btnCheckEnableSound_Checked(object sender, RoutedEventArgs e)
        {
            KeyboardListener.StartHook();
        }

        private void btnCheckEnableSound_Unchecked(object sender, RoutedEventArgs e)
        {
            KeyboardListener.StopHook();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            forceExit = true;
            this.Close();
        }

        private void sliderVolume_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            KeyboardListener.PlaySound();
        }
    }
}
