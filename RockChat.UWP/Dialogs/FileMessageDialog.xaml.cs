using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RockChat.UWP.Dialogs
{
    public sealed partial class FileMessageDialog : ContentDialog
    {
        public class FileUploadData : INotifyPropertyChanged
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public StorageFile File { get; set; }
            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public FileUploadData Data { get; }
        public FileMessageDialog(FileUploadData data)
        {
            Data = data;
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            
        }

    }
}
