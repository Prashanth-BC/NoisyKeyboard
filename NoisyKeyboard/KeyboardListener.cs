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

namespace NoisyKeyboard
{
    class KeyboardListener : IDisposable
    {
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
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private bool disposedValue;
        private static Stack<MediaPlayer> _mediaPlayers = new Stack<MediaPlayer>();
        private static double _volume = 10;
        private static string _soundFolder;
        private static object _synchObject = new object();
        public static void SetVolume(double volume)
        {
            if(volume < 10)
            { volume = 10; }
            _volume = volume;
        }
        public static void SetMusicFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                _soundFolder = folderPath;
                Init();
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
            if (String.IsNullOrEmpty(_soundFolder))
            {
                _soundFolder =  $"{Directory.GetCurrentDirectory()}\\data\\sounds\\mechanical"; 
            }
            lock (_synchObject)
            {
                _mediaPlayers.Clear();
                for (int i = 0; i < 20; i++)
                {
                    var mediaPlayer = new MediaPlayer();
                    Directory.GetCurrentDirectory();
                    mediaPlayer.Open(new Uri($"{_soundFolder}\\1.wav"));
                    _mediaPlayers.Push(mediaPlayer);
                }
            }
            
        }

        private static IntPtr HookCallback(
        int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                //Call the music play here
                new Thread(PlaySound).Start();
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
        public static void PlaySound()
        {
            lock (_synchObject)
            {
                var mediaPlayer = _mediaPlayers.Pop();
                mediaPlayer.Dispatcher.Invoke(() => {
                    mediaPlayer.Stop();
                    mediaPlayer.Volume = _volume / 100.0;
                    mediaPlayer.Play();
                    _mediaPlayers.Push(mediaPlayer);
                });
            }
           
            
            

        }
        public static void StartHook()
        {
            if(_hookID == IntPtr.Zero)
            {
                _hookID = SetHook(_proc);
            }
           

        }
        public static void StopHook()
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
                    KeyboardListener.StopHook();
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
