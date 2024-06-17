using RICPFitter.Collections;
using RICPFitter;
using System;

namespace CorrelatioM.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        string xmlFP = AppDomain.CurrentDomain.BaseDirectory + "ListOfFunctions.xml";
        FitBox = new FitBox1DXml(xmlFP);
        FitBox.ListOfFitFunctions.SortByCategoryAndName();
        FunctionCollectionXml funcCollection = new(xmlFP);
        funcCollection.SortByCategoryAndName();
        MainInputDataViewModel = new InputDataViewModel(FitBox, funcCollection);
        MainGraphViewModel = new GraphViewModel(FitBox);
        MainFitDataViewModel = new FitViewModel(FitBox);
    }

    public IFitBox1D FitBox { get; set; }

    public InputDataViewModel MainInputDataViewModel { get; set; }

    public GraphViewModel MainGraphViewModel { get; set; }

    public FitViewModel MainFitDataViewModel { get; set; }
}
