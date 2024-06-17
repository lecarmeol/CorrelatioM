using CommunityToolkit.Mvvm.Input;
using CorrelatioM.Core;
using RICPFitter;
using RICPFitter.Functions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelatioM.ViewModels
{
    /// <summary>
    /// Viewmodel of a fitting parameter view
    /// </summary>
    public class FitViewModel : ViewModelBase
    {
        private readonly IFitBox1D fitBox;
        private string? selectedFunctionName;
        private IFittable? selectedFunction;
        private string functionInformation = "Information: --";
        private ObservableCollection<FuncParameter>? initGuesses;
        private string fitInformation = "--";
        private bool fitInfoIsVisible;

        /// <summary>
        /// New viewmodel with dependency
        /// </summary>
        /// <param name="fitBox">model</param>
        public FitViewModel(IFitBox1D fitBox)
        {
            this.fitBox = fitBox;
            if (fitBox.ListOfFitFunctions != null)
            {
                AllFunctionsName = new ObservableCollection<string>(fitBox.ListOfFitFunctions.Functions.ConvertAll(f => f.Name));
                SelectedFunctionName = fitBox.ListOfFitFunctions.Functions[0].Name;
            }
            DoFitCommand = new RelayCommand(DoFit);
            ExportCommand = new RelayCommand(Export);
        }

        /// <summary>
        /// Current function used for fitting
        /// </summary>
        public IFittable? SelectedFunction
        {
            get => selectedFunction;
            set
            {
                SetProperty(ref selectedFunction, value);
                if (selectedFunction != null)
                {
                    fitBox.FitFunction = value;
                    InitialGuesses = new ObservableCollection<FuncParameter>(selectedFunction.GuessParameters);
                    FunctionInformation = $"Information: {selectedFunction.Description} ({selectedFunction.Category})";
                    FitInfoIsVisible = false;
                }
            }
        }

        /// <summary>
        /// Name of all functions available for fitting
        /// </summary>
        public ObservableCollection<string>? AllFunctionsName { get; set; }

        /// <summary>
        /// Current function name
        /// </summary>
        public string? SelectedFunctionName
        {
            get => selectedFunctionName;
            set
            {
                SetProperty(ref selectedFunctionName, value);
                if (selectedFunctionName != null && fitBox.ListOfFitFunctions != null)
                {
                    SelectedFunction = fitBox.ListOfFitFunctions.FindByName(selectedFunctionName);
                }
            }
        }

        /// <summary>
        /// Information about the current function
        /// </summary>
        public string FunctionInformation
        {
            get => functionInformation;
            set => SetProperty(ref functionInformation, value);
        }
        /// <summary>
        /// Initial guess of the fit parameters
        /// </summary>
        public ObservableCollection<FuncParameter>? InitialGuesses
        {
            get => initGuesses;
            private set
            {
                SetProperty(ref initGuesses, value);
            }
        }

        /// <summary>
        /// Fit tolerance value
        /// </summary>
        public double FitTolerance 
        {
            get
            {
                if (SelectedFunction != null)
                {
                    return SelectedFunction.FitTolerance;
                }
                else return 0;
            }
            set
            {
                if (SelectedFunction != null)
                {
                    SelectedFunction.FitTolerance = value;
                    OnPropertyChanged(nameof(FitTolerance));
                }
            }
        }

        /// <summary>
        /// Max number of iteration during fit calculation
        /// </summary>
        public int FitMaxIteration
        {
            get
            {
                if (SelectedFunction != null)
                {
                    return SelectedFunction.FitMaxIteration;
                }
                else return 0;
            }
            set
            {
                if (SelectedFunction != null)
                {
                    SelectedFunction.FitMaxIteration = value;
                    OnPropertyChanged(nameof(FitMaxIteration));
                }
            }
        }

        /// <summary>
        /// Information about fit result
        /// </summary>
        public string FitInformation 
        { 
            get => fitInformation; 
            set => SetProperty(ref fitInformation, value);
        }

        public bool FitInfoIsVisible 
        { 
            get => fitInfoIsVisible; 
            set => SetProperty(ref fitInfoIsVisible, value); 
        }

        /// <summary>
        /// Command to run the fitting calculation
        /// </summary>
        public IRelayCommand DoFitCommand { get; }

        private async void DoFit()
        {
            await Task.Run(() =>
            {
                try
                {
                    if (SelectedFunction != null && fitBox.InputData != null)
                    {
                        double cod = SelectedFunction.DoFit(fitBox.InputData.XData, fitBox.InputData.YData);
                        string result = $"R² = {cod:G4}";
                        foreach (var param in SelectedFunction.Parameters)
                        {
                            result += $"\n{param.Name} = {param.Value:G4}";
                        }
                        FitInformation = result;
                        FitInfoIsVisible = true;
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }

        public string ColumnSeparator { get; set; } = ",";

        public string DecimalSeparator { get; set; } = ".";

        public bool WithHeader { get; set; } = false;

        public bool WithInputData { get; set; } = false;

        public IRelayCommand ExportCommand { get; }

        public Interaction<string, string[]?> SaveFileInteraction { get; } = new Interaction<string, string[]?>();

        private async void Export()
        {
            try
            {
                var files = await SaveFileInteraction.HandleAsync("Save your graph");
                if (files != null && fitBox.InputData != null)
                {
                    using StreamWriter writer = new(files[0]);
                    if (WithHeader)
                    {
                        string header = fitBox.InputData.XLabel + ColumnSeparator;
                        if (WithInputData)
                        {
                            header += fitBox.InputData.YLabel + ColumnSeparator;
                        }
                        header += fitBox.FitFunction?.Name;
                        writer.WriteLine(header);
                    }
                    if (fitBox.FitFunction != null && fitBox.InputData.XData != null && fitBox.InputData.YData != null)
                    {
                        double[] yFit = fitBox.FitFunction.GenerateData(fitBox.InputData.XData);
                        for (int i = 0; i < fitBox.InputData.XData.Length; i++)
                        {
                            string line = fitBox.InputData.XData[i].ToString(CultureInfo.InvariantCulture).Replace(".", DecimalSeparator) + ColumnSeparator;
                            if (WithInputData)
                            {
                                line += fitBox.InputData.YData[i].ToString(CultureInfo.InvariantCulture).Replace(".", DecimalSeparator) + ColumnSeparator;
                            }
                            line += yFit[i].ToString(CultureInfo.InvariantCulture).Replace(".", DecimalSeparator);
                            writer.WriteLine(line);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
