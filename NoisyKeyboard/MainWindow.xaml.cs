// File: MainWindow.xaml.cs
// Author: Prashanth
// This program is distributed in the hope that it will be useful,but WITHOUT ANY WARRANTY.
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;



namespace NoisyKeyboard
{


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool forceExit = false;
        Mutex _appMutex;

        public MainWindow()
        {
            InitializeComponent();
            bool aIsNewInstance = false;
            _appMutex = new Mutex(true, "inc.pacific.noisykeyboard", out aIsNewInstance);
            if (!aIsNewInstance)
            {
                forceExit = true;
                MessageBox.Show("Already an instance is running...");
                App.Current.Shutdown();
            }
            var soundDirectories = Directory.EnumerateDirectories($"{Directory.GetCurrentDirectory()}\\data\\sounds");
            int selectedIndex = 0;
            var config = Config.GetConfig();
            foreach (var folder in soundDirectories)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                var contentText = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(folder.Split('\\').Last().ToLower());

                comboBoxItem.Content = contentText;
                comboBoxItem.Tag = folder;
                comboboxSounds.Items.Add(comboBoxItem);
                if (config.soundAtStart == contentText)
                {
                    comboboxSounds.SelectedIndex = selectedIndex;
                }
                selectedIndex++;
            }
            sliderVolume.Value = config.volumeAtStart;

            radioBtnSoundNone.IsChecked = true;

            if (config.actionAtStart == (string)radioBtnSoundOnKeyUp.Content)
            {
                radioBtnSoundOnKeyUp.IsChecked = true;
            }
            if (config.actionAtStart == (string)radioBtnSoundOnKeyDown.Content)
            {
                radioBtnSoundOnKeyDown.IsChecked = true;
            }

            this.Left = System.Windows.SystemParameters.PrimaryScreenWidth - this.Width;
            this.Top = System.Windows.SystemParameters.PrimaryScreenHeight - this.Height - 30;

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

        private void btnCheckEnableSound_Unchecked(object sender, RoutedEventArgs e)
        {
            KeyboardListener.StopNoise();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            forceExit = true;
            this.Close();
        }

        private void sliderVolume_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            KeyboardListener.PlaySound(null);
        }

        private void radioBtnSoundOnKeyUp_Checked(object sender, RoutedEventArgs e)
        {
            KeyboardListener.StartNoise(KeyboardListener.KEY_STATE.ON_KEY_UP);
        }

        private void radioBtnSoundOnKeyDown_Checked(object sender, RoutedEventArgs e)
        {
            KeyboardListener.StartNoise(KeyboardListener.KEY_STATE.ON_KEY_DOWN);
        }

        private void radioBtnSoundNone_Checked(object sender, RoutedEventArgs e)
        {
            KeyboardListener.StopNoise();
        }

        private void btnSaveAsDefault_Click(object sender, RoutedEventArgs e)
        {
            var config = Config.GetConfig();
            config.volumeAtStart = sliderVolume.Value;
            config.soundAtStart = comboboxSounds.Text;
            if (radioBtnSoundNone.IsChecked.GetValueOrDefault(false))
            {
                config.actionAtStart = (string)radioBtnSoundNone.Content;

            }
            if (radioBtnSoundOnKeyUp.IsChecked.GetValueOrDefault(false))
            {
                config.actionAtStart = (string)radioBtnSoundOnKeyUp.Content;

            }
            if (radioBtnSoundOnKeyDown.IsChecked.GetValueOrDefault(false))
            {
                config.actionAtStart = (string)radioBtnSoundOnKeyDown.Content;

            }
            Config.SaveConfig();
            MessageBox.Show("Settings saved.");
        }
    }
}
