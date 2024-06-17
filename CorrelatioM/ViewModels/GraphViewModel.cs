using CommunityToolkit.Mvvm.Input;
using CorrelatioM.Core;
using RICPFitter;
using ScottPlot.Avalonia;
using ScottPlot.DataSources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelatioM.ViewModels
{
    public class GraphViewModel : ViewModelBase
    {
        private readonly IFitBox1D mainFitBox;

        // Needed as Scottplot is not MVVM friendly
        private AvaPlot? mainPlot;
        private ScatterSourceDoubleArray? genData, fitData;

        public GraphViewModel(IFitBox1D fitBox)
        {
            mainFitBox = fitBox;
            mainFitBox.InputDataChanged += MainFitBox_InputDataChanged;
            mainFitBox.FitDataChanged += MainFitBox_FitDataChanged;
            ExportAsPngCmd = new RelayCommand(ExportAsPng);
            ExportAsSvgCmd = new RelayCommand(ExportAsSvg);
        }

        private void MainFitBox_FitDataChanged(RICPFitter.Functions.IFittable? function, double[]? x, double[]? y)
        {
            string fitInfo = "";
            if (function is not null)
            {
                fitInfo = function.Description + Environment.NewLine;
                foreach (var param in function.Parameters)
                {
                    fitInfo += $"{param.Name} = {param.Value:G4}" + Environment.NewLine;
                }
                fitInfo += $"R² = {function.CoeffOfDetermination:G4}";
            }
            if (x is not null && y is not null)
            {
                fitData = new ScatterSourceDoubleArray(x, y);
                UpdatePlot(fitInfo);
            }
        }

        private void MainFitBox_InputDataChanged(double[]? x, double[]? y)
        {
            if (x is not null && y is not null)
            {
                genData = new ScatterSourceDoubleArray(x, y);
                fitData = null;
                UpdatePlot();
            }
            if (mainFitBox is not null &&
                mainFitBox.InputData is not null &&
                mainFitBox.InputData.Title is not null &&
                mainFitBox.InputData.XLabel is not null &&
                mainFitBox.InputData.YLabel is not null)
            {
                var input = mainFitBox.InputData;
                UpdateInputInfo(input.Title, input.XLabel, input.YLabel);
            }
        }

        #region NON MVVM AREA
        public void SetAvaPlot(AvaPlot plot)
        {
            mainPlot = plot;
            UpdatePlot();
        }

        private void UpdatePlot(string? fitInfo = null)
        {
            if (mainPlot is not null)
            {
                mainPlot.Plot.Clear();
                if (genData != null)
                {
                    var inputPLot = mainPlot.Plot.Add.Scatter(genData);
                    inputPLot.LegendText = "input data";
                }
                if (fitData != null)
                {

                    var fitPlot = mainPlot.Plot.Add.ScatterLine(fitData);
                    fitPlot.LineWidth = 3;
                    if (fitInfo is not null) fitPlot.LegendText = fitInfo;
                }
                mainPlot.Plot.Axes.AutoScale();
                mainPlot.Refresh();
            }
        }

        private void UpdateInputInfo(string title, string xLabel, string yLabel)
        {
            if (mainPlot is not null)
            {
                mainPlot.Plot.Title(title);
                mainPlot.Plot.XLabel(xLabel);
                mainPlot.Plot.YLabel(yLabel);
            }
        }
        #endregion

        #region MVVM AREA
        public int ExportWidth { get; set; } = 1920;
        public int ExportHeight { get; set; } = 1080;

        public Interaction<string, string[]?> SaveFileInteraction { get; } = new Interaction<string, string[]?>();

        public IRelayCommand ExportAsPngCmd { get; set; }

        public IRelayCommand ExportAsSvgCmd { get; set; }

        private void ExportAsPng()
        {
            Export("png");
        }

        private void ExportAsSvg()
        {
            Export("svg");
        }

        private async void Export(string format)
        {
            var files = await SaveFileInteraction.HandleAsync("Save your graph");
            if (files != null && mainPlot != null)
            {
                var splitted = files[0].Split('.');
                if (splitted.LastOrDefault() != format)
                {
                    files[0] += "." + format;
                }

                switch (format)
                {
                    case "png":
                        mainPlot.Plot.SavePng(files[0], ExportWidth, ExportHeight);
                        break;
                    case "svg":
                        mainPlot.Plot.SaveSvg(files[0], ExportWidth, ExportHeight);
                        break;
                }
            }
        }
        #endregion
    }
}
