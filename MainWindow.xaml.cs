using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Interop;
using System.Text.Json.Serialization;
using IOPath = System.IO.Path;
using DrawingIcon = System.Drawing.Icon;

namespace Lavappies;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public ObservableCollection<AppItem> Apps { get; set; } = new();

    public MainWindow()
    {
        InitializeComponent();
        AppsListView.ItemsSource = Apps;
        LoadApps();
        AutoStartCheckBox.IsChecked = IsAutoStartEnabled();
        LoadLogo();
        ApplyTheme(false); // Start with light mode
    }

    private void LoadApps()
    {
        try
        {
            if (File.Exists("apps.json"))
            {
                var json = File.ReadAllText("apps.json");
                Apps = System.Text.Json.JsonSerializer.Deserialize<ObservableCollection<AppItem>>(json) ?? new();
                foreach (var app in Apps)
                {
                    LoadIcon(app);
                }
                AppsListView.ItemsSource = Apps;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading apps: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SaveApps()
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(Apps);
            File.WriteAllText("apps.json", json);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving apps: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AddApp_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Executables (*.exe)|*.exe|Shortcuts (*.lnk)|*.lnk"
        };
        if (dialog.ShowDialog() == true)
        {
            try
            {
                string path = dialog.FileName;
                string name = IOPath.GetFileNameWithoutExtension(path);
                BitmapSource icon = null;
                // Note: For shortcuts, we use the .lnk directly without resolving to avoid compatibility issues
                if (File.Exists(path))
                {
                    var sysIcon = DrawingIcon.ExtractAssociatedIcon(path);
                    if (sysIcon != null)
                    {
                        icon = Imaging.CreateBitmapSourceFromHIcon(sysIcon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    }
                }
                Apps.Add(new AppItem { Name = name, Path = path, Icon = icon });
                SaveApps();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding app: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void RemoveApp_Click(object sender, RoutedEventArgs e)
    {
        if (AppsListView.SelectedItem is AppItem item)
        {
            Apps.Remove(item);
            SaveApps();
        }
    }

    private void LaunchApp_Click(object sender, RoutedEventArgs e)
    {
        if (AppsListView.SelectedItem is AppItem item)
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = item.Path, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error launching app: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void CreatePortableExe_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // First, publish
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "publish --self-contained -r win-x64 -c Release",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                MessageBox.Show($"Error publishing: {error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Then, try to create installer with Inno Setup
            var isccProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "ISCC.exe",
                Arguments = "setup.iss",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });
            if (isccProcess != null)
            {
                isccProcess.WaitForExit();
                if (isccProcess.ExitCode == 0)
                {
                    MessageBox.Show("Installer created successfully: LavappiesSetup.exe", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var error = isccProcess.StandardError.ReadToEnd();
                    MessageBox.Show($"Error creating installer: {error}. Make sure Inno Setup is installed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Portable EXE created. To create installer, install Inno Setup and run ISCC.exe setup.iss", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AutoStartCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        SetAutoStart(true);
    }

    private void AutoStartCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        SetAutoStart(false);
    }

    private bool IsAutoStartEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false);
        return key?.GetValue("Lavappies") != null;
    }

    private void SetAutoStart(bool enable)
    {
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
        if (enable)
        {
            key.SetValue("Lavappies", $"\"{System.Reflection.Assembly.GetExecutingAssembly().Location}\"");
        }
        else
        {
            key.DeleteValue("Lavappies", false);
        }
    }

    private void LoadIcon(AppItem app)
    {
        if (File.Exists(app.Path))
        {
            var sysIcon = DrawingIcon.ExtractAssociatedIcon(app.Path);
            if (sysIcon != null)
            {
                app.Icon = Imaging.CreateBitmapSourceFromHIcon(sysIcon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
        }
    }

    private void LoadLogo()
    {
        try
        {
            LogoImage.Source = new BitmapImage(new Uri("logo.ico", UriKind.Relative));
        }
        catch
        {
            // Logo not found, use placeholder
        }
    }

    private void DarkModeCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        ApplyTheme(true);
    }

    private void DarkModeCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        ApplyTheme(false);
    }

    private void ApplyTheme(bool dark)
    {
        var background = dark ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 30)) : System.Windows.Media.Brushes.White;
        var foreground = dark ? System.Windows.Media.Brushes.White : System.Windows.Media.Brushes.Black;
        this.Background = background;
        this.Foreground = foreground;
        AppsListView.Background = background;
        AppsListView.Foreground = foreground;
    }
}

public class AppItem
{
    public string? Name { get; set; }
    public string? Path { get; set; }
    [JsonIgnore]
    public BitmapSource? Icon { get; set; }
}