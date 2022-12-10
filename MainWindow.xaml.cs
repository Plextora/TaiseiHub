// Copyright Plextora 2022
// This file is licensed under GPL-3.0-or-later

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Memory;

namespace TaiseiHub
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer _checkGameState = new DispatcherTimer();
        static Mem _memoryManager = new Mem();

        public MainWindow()
        {
            InitializeComponent();

            _checkGameState.Tick += _checkGameState_Tick;
            _checkGameState.Interval = new TimeSpan(0, 0, 1);
            _checkGameState.Start();

            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

            if (!_memoryManager.OpenProcess("taisei")) SetStatusLabel("Closed!", "#DC143C");
        }

        private void _checkGameState_Tick(object sender, EventArgs e)
        {
            int pausedState = _memoryManager.ReadInt("taisei.exe+98E3D8");

            if (pausedState != 0) SetStatusLabel("Playing!", "#98FB98");
            else SetStatusLabel("Paused/In Menu!", "#F0E68C");

            if (!IsTaiseiRunning()) SetStatusLabel("Closed!", "#DC143C");
            else if (IsTaiseiRunning()) _memoryManager.OpenProcess("taisei");
        }

        public static bool IsTaiseiRunning()
        {
            Process[] name = Process.GetProcessesByName("taisei");

            if (name.Length == 0)
            {
                return false;
            }

            return true;
        }

        private void SetStatusLabel(string status, string colorHexCode)
        {
            StatusLabel.Foreground = new BrushConverter().ConvertFromString($"{colorHexCode}") as Brush;
            StatusLabel.Content = $"Game Status: {status}";
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void WindowToolbar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();

        private void OnProcessExit(object sender, EventArgs e) => _memoryManager.CloseProcess();
    }
}
