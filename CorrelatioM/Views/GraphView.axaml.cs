using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CorrelatioM.ViewModels;
using ScottPlot.Avalonia;
using ScottPlot.Plottables;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CorrelatioM.Views
{
    public partial class GraphView : UserControl
    {

        private Crosshair? crosshair;

        public GraphView()
        {
            InitializeComponent();

            // Binding
            this.DataContextChanged += GraphView_DataContextChanged;
        }

        private void GraphView_DataContextChanged(object? sender, EventArgs e)
        {
            if (sender is GraphView graphView)
            {
                if (graphView.DataContext is GraphViewModel viewModel)
                {
                    var plot = this.Find<AvaPlot>("mainPlot");
                    if (plot != null)
                    {
                        viewModel?.SetAvaPlot(plot);
                        // Custom context menu
                        plot.Menu.AddSeparator();
                        plot.Menu.Add("Show/Hide legend", plot =>
                        {
                            var legend = plot.Plot.Legend;
                            if (legend.IsVisible) legend.IsVisible = false;
                            else legend.IsVisible = true;
                            plot.Refresh();
                        });
                        plot.Menu.Add("Autoscale", plot =>
                        {
                            plot.Plot.Axes.AutoScale();
                            plot.Refresh();
                        });
                        plot.Menu.Add("Show/Hide export panel", _ =>
                        {
                            gridExport.IsVisible = !gridExport.IsVisible;
                        });

                        // Read value
                        crosshair = plot.Plot.Add.Crosshair(0, 0);
                        crosshair.IsVisible = false;
                    }

                    //_selectFilesInteractionDisposable?.Dispose();
                    viewModel?.SaveFileInteraction.RegisterHandler(InteractionHandler);
                }
            }
        }

        private async Task<string[]?> InteractionHandler(string input)
        {
            // Get a reference to our TopLevel (in our case the parent Window)
            var topLevel = TopLevel.GetTopLevel(this);

            // Try to get the files
            var storageFiles = await topLevel!.StorageProvider.SaveFilePickerAsync(
                            new FilePickerSaveOptions()
                            {
                                Title = input
                            });

            // Transform the files as needed and return them. If no file was selected, null will be returned
            if (storageFiles != null)
            {
                return [storageFiles.Path.AbsolutePath];
            }
            return null;
        }
    }
}
