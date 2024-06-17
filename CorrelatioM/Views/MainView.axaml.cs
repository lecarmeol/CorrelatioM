using Avalonia.Controls;
using Avalonia.Interactivity;

namespace CorrelatioM.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    public void PaneHandler(object sender, RoutedEventArgs e)
    {
        splitView.IsPaneOpen = !splitView.IsPaneOpen;
        if (splitView.IsPaneOpen) 
        {
            ((Button)sender).Content = "<";
            tbConfig.IsVisible = true;
            block1.IsVisible = true;
            block2.IsVisible = true;
        }
        else 
        {
            ((Button)sender).Content = ">";
            tbConfig.IsVisible = false;
            block1.IsVisible = false;
            block2.IsVisible = false;
        }
    }
}
