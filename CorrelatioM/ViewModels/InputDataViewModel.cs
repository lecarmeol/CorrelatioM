using CommunityToolkit.Mvvm.Input;
using CorrelatioM.Core;
using RICPFitter;
using RICPFitter.Collections;
using RICPFitter.Data;
using RICPFitter.Functions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrelatioM.ViewModels
{
    public class InputDataViewModel : ViewModelBase
    {
        private readonly IFitBox1D fitBox;
        private readonly IFunctionCollection functionCollection;
        private string? selectedFunctionName;
        private IFittable? selectedFunction;
        private ObservableCollection<FuncParameter>? funcParameters;
        private string functionInformation = "Information: --";

        public InputDataViewModel(IFitBox1D fitBox, IFunctionCollection functionCollection)
        {
            this.fitBox = fitBox;
            this.functionCollection = functionCollection;
            if (functionCollection != null)
            {
                AllFunctionsName = new ObservableCollection<string>(functionCollection.Functions.ConvertAll(f => f.Name));
                SelectedFunctionName = functionCollection.Functions[0].Name;
            }
            ImportCommand = new RelayCommand(Import);
            GenerateCommand = new RelayCommand(GenerateInputData);
        }

        public IFittable? SelectedFunction 
        { 
            get => selectedFunction; 
            set
            {
                SetProperty(ref selectedFunction, value);
                if (selectedFunction != null)
                {
                    FuncParameters = new ObservableCollection<FuncParameter>(selectedFunction.Parameters);
                    FunctionInformation = $"Information: {selectedFunction.Description} ({selectedFunction.Category})";
                }
            }
        }

        public string ColumnSeparator { get; set; } = ",";

        public bool WithHeader { get; set; } = true;

        public string? OptionalTitle { get; set; }

        public IRelayCommand ImportCommand { get; }

        public Interaction<string, string[]?> SelectFilesInteraction { get; } = new Interaction<string, string[]?>();

        private async void Import()
        {
            try
            {
                var files = await SelectFilesInteraction.HandleAsync("Select your data");

                if (files != null && files.Length > 0)
                {
                    using var reader = new StreamReader(files[0]);
                    List<string> colX = [];
                    List<string> colY = [];
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (line != null)
                        {
                            var values = line.Split(ColumnSeparator);

                            colX.Add(values[0]);
                            colY.Add(values[1]);
                        }
                    }
                    IDataSet inputData = new DataSet();
                    if (OptionalTitle != null) inputData.Title = OptionalTitle;
                    if (WithHeader)
                    {
                        inputData.XLabel = colX[0];
                        inputData.YLabel = colY[0];
                        colX.RemoveAt(0);
                        colY.RemoveAt(0);
                    }
                    inputData.XData = [.. colX.ConvertAll( x =>
                Double.Parse(x.Replace(',','.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture))];
                    inputData.YData = [.. colY.ConvertAll(y =>
                Double.Parse(y.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture))];
                    fitBox.InputData = inputData;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public string FunctionInformation 
        { 
            get => functionInformation; 
            set => SetProperty(ref functionInformation, value);
        }

        public ObservableCollection<FuncParameter>? FuncParameters 
        { 
            get => funcParameters; 
            private set
            {
                SetProperty(ref funcParameters, value);
            } 
        }

        public ObservableCollection<string>? AllFunctionsName { get; set; }

        public string? SelectedFunctionName 
        { 
            get => selectedFunctionName; 
            set
            {
                SetProperty(ref selectedFunctionName, value);
                if (selectedFunctionName != null && functionCollection != null)
                {
                    SelectedFunction = functionCollection.FindByName(selectedFunctionName);
                }
            }
        }

        public double MinX { get; set; } = -10;

        public double MaxX { get; set; } = 10;

        public int NbOfPoints { get; set; } = 201;

        public bool WithRandomness { get; set; } = false;

        public double RandomnessStrength { get; set; } = 10;

        public IRelayCommand GenerateCommand { get; set; }

        private async void GenerateInputData()
        {
            await Task.Run(() =>
            {
                try
                {
                    if (SelectedFunction != null)
                    {
                        SelectedFunction.Randomness = WithRandomness;
                        SelectedFunction.RandomnessStrength = RandomnessStrength;
                        (double[] x, double[] y) = SelectedFunction.GenerateData(MinX, MaxX, NbOfPoints);
                        fitBox.InputData = new DataSet(x, y, SelectedFunction.Name, "x (a.u.)", "y (a.u.)");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }
    }
}
