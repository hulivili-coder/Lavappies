using System.Windows;

namespace Lavappies;

public partial class SetupWizard : Window
{
    public SetupWizard()
    {
        InitializeComponent();
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        // For now, just close
        this.Close();
    }
}