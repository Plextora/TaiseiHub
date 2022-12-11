// Copyright Plextora 2022
// This file is licensed under GPL-3.0-or-later

using System;
using System.Diagnostics;
using System.Threading;
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
        DispatcherTimer _giveInfHealth = new DispatcherTimer();
        private DispatcherTimer _giveInfSC = new DispatcherTimer(); // SC = Spell Cards
        static Mem _memoryManager = new Mem();

        public MainWindow()
        {
            InitializeComponent();

            _checkGameState.Tick += _checkGameState_Tick;
            _checkGameState.Interval = new TimeSpan(0, 0, 1);

            _giveInfHealth.Tick += _giveInfHealth_Tick;
            _giveInfHealth.Interval = new TimeSpan(0, 0, (int)0.5);
            _giveInfSC.Tick += _giveInfSC_Tick;
            _giveInfSC.Interval = new TimeSpan(0, 0, (int)0.5);
            _checkGameState.Start();

            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

            if (!_memoryManager.OpenProcess("taisei")) SetStatusLabel(GameStatusLabel, "Game Status: Closed!", "#DC143C");
        }

        #region Cheats

        private void InfiniteHealthButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (IsTaiseiRunning())
            {
                if (_giveInfHealth.IsEnabled)
                {
                    InfiniteHealthButton.Content = "Infinite Health (Off)";
                    SetStatusLabel(CheatStatusLabel, "Cheat Status: No cheats running", "#ffdab9");
                    _giveInfHealth.Stop();
                    _memoryManager.WriteMemory("taisei.exe+98F548", "int", "2");
                    return;
                }

                SetStatusLabel(CheatStatusLabel, "Cheat Status: Running infinite health!", "#c7fcc7");
                InfiniteHealthButton.Content = "Infinite Health (On)";

                _giveInfHealth.Start();
            }
        }

        private void InfiniteSpellCardButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (IsTaiseiRunning())
            {
                if (_giveInfSC.IsEnabled)
                {
                    InfiniteSpellCardButton.Content = "Infinite Spell Cards (Off)";
                    SetStatusLabel(CheatStatusLabel, "Cheat Status: No cheats running", "#ffdab9");
                    _giveInfSC.Stop();
                    _memoryManager.WriteMemory("taisei.exe+98F54C", "int", "3");
                    return;
                }

                SetStatusLabel(CheatStatusLabel, "Cheat Status: Running infinite spell cards!", "#c7fcc7");
                InfiniteSpellCardButton.FontSize = 14;
                InfiniteSpellCardButton.Content = "Infinite Spell Cards (On)";

                _giveInfSC.Start();
            }
        }

        private void _giveInfHealth_Tick(object sender, EventArgs e)
        {
            if (!_memoryManager.WriteMemory("taisei.exe+98F548", "int", "8"))
            {
                SetStatusLabel(CheatStatusLabel, "Cheat status: Failed to write memory.", "#DC143C");
                InfiniteHealthButton.Content = "Infinite Health (Off)";
            }
        }

        private void _giveInfSC_Tick(object sender, EventArgs e)
        {
            if (!_memoryManager.WriteMemory("taisei.exe+98F54C", "int", "8"))
            {
                SetStatusLabel(CheatStatusLabel, "Cheat status: Failed to write memory.", "#DC143C");
                InfiniteSpellCardButton.Content = "Infinite Spell Cards (Off)";
            }
        }

        #endregion

        private void _checkGameState_Tick(object sender, EventArgs e)
        {
            int pausedState = _memoryManager.ReadInt("taisei.exe+977B50");
            int pausedStateExtended = _memoryManager.ReadInt("taisei.exe+98E3D8");

            if (pausedState == 0) SetStatusLabel(GameStatusLabel, "Game Status: Paused!", "#F0E68C");
            else if (pausedStateExtended != 0) SetStatusLabel(GameStatusLabel, "Game Status: Playing!", "#98FB98");
            else SetStatusLabel(GameStatusLabel, "Game Status: In Menu!", "#F0E68C");

            if (!IsTaiseiRunning()) SetStatusLabel(GameStatusLabel, "Game Status: Closed!", "#DC143C");
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

        private void SetStatusLabel(System.Windows.Controls.Label statusLabel, string statusText, string colorHexCode)
        {
            statusLabel.Foreground = new BrushConverter().ConvertFromString($"{colorHexCode}") as Brush;
            statusLabel.Content = $"{statusText}";
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void WindowToolbar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();

        private void OnProcessExit(object sender, EventArgs e) => _memoryManager.CloseProcess();
    }
}
