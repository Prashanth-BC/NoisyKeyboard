// File: KeyboardListener.cs
// Author: Prashanth
// This program is distributed in the hope that it will be useful,but WITHOUT ANY WARRANTY.
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.ComponentModel;

namespace NoisyKeyboard
{
    class KeyboardListener : IDisposable
    {
        public enum KEY_STATE {
            ON_KEY_UP,
            ON_KEY_DOWN
        }
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
        LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private bool disposedValue;
        private static Queue<MediaPlayer> _mediaPlayers = new Queue<MediaPlayer>();
        private static double _volume = 10;
        private static string _soundFolder;
        private static object _synchObject = new object();
        private static int _onKeyUpOrDown = 0x0101;
        private static HashSet<int> _ignoreKeys = new HashSet<int>();

        private static TimeSpan _timeSpanBegin = new TimeSpan(0);
        
        public static void SetVolume(double volume)
        {
            if(volume < 10)
            { volume = 10; }
            _volume = volume;
            lock (_synchObject)
            {
                foreach(var mediaplayer in _mediaPlayers)
                {
                    mediaplayer.Volume = _volume / 100;
                }
            }
        }
        
        public static void SetMusicFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                _soundFolder = folderPath;
                SetSound();
            }

        }

        private delegate IntPtr LowLevelKeyboardProc(
        int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        

        static KeyboardListener()
        {
            
            Init();
        }

        private static void Init()
        {
            var config = Config.GetConfig();
            foreach (var kv in config.ignoreKeys)
            {
                _ignoreKeys.Add(kv.Value);
            }
            
            //Voulume and Sound should be initialized from the config in the MainWindow class.

        }

        private static void SetSound()
        {
            if (String.IsNullOrEmpty(_soundFolder))
            {
                _soundFolder = $"{Directory.GetCurrentDirectory()}\\data\\sounds\\mechanical2";
            }
            lock (_synchObject)
            {

                string uriString = $"{_soundFolder}\\1.wav";
                _mediaPlayers.Clear();
                for (int i = 0; i < 20; i++)
                {
                    var mediaPlayer = new MediaPlayer();

                    
                    mediaPlayer.Open(new Uri(uriString));
                    mediaPlayer.Play();
                    _mediaPlayers.Enqueue(mediaPlayer);
                }
            }
        }

        private static IntPtr HookCallback(
        int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)_onKeyUpOrDown)
            {
                var vkCode = Marshal.ReadInt32(lParam);
                
                if (!_ignoreKeys.Contains(vkCode))
                {
                    
                    //Call the music play here
                    ThreadPool.QueueUserWorkItem(PlaySound);
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
        public static void PlaySound(object state)
        {
            lock (_synchObject)
            {
                var mediaPlayer = _mediaPlayers.Dequeue();
                mediaPlayer.Dispatcher.Invoke(() => {
                    //mediaPlayer.Stop();
                    mediaPlayer.Position = _timeSpanBegin;
                    //mediaPlayer.Play();
                    _mediaPlayers.Enqueue(mediaPlayer);
                });
            }
           
            
            

        }
        public static void StartNoise(KEY_STATE keyState)
        {
            StopNoise();
            if(keyState == KEY_STATE.ON_KEY_DOWN)
            {                
                _onKeyUpOrDown = WM_KEYDOWN;
            }
            else
            {

                _onKeyUpOrDown = WM_KEYUP;
            }
            if (_hookID == IntPtr.Zero)
            {
                _hookID = SetHook(_proc);
            }
            
           

        }
        public static void StopNoise()
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
            }
            _hookID = IntPtr.Zero;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    KeyboardListener.StopNoise();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~KeyboardListener()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
