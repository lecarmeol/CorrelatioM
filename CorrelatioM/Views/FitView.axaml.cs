using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CorrelatioM.ViewModels;
using System;
using System.Threading.Tasks;

namespace CorrelatioM.Views
{
    public partial class FitView : UserControl
    {
        public FitView()
        {
            InitializeComponent();
        }

        // Stores a reference to the disposable in order to clean it up if needed
        private IDisposable? _selectFilesInteractionDisposable;

        protected override void OnDataContextChanged(EventArgs e)
        {
            // Dispose any old handler
            _selectFilesInteractionDisposable?.Dispose();

            if (DataContext is FitViewModel vm)
            {
                // register the interaction handler
                _selectFilesInteractionDisposable =
                    vm.SaveFileInteraction.RegisterHandler(InteractionHandler);
            }

            base.OnDataContextChanged(e);
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
