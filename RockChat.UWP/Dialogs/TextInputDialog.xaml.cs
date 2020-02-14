using Windows.UI.Xaml.Controls;
using RockChat.Core.PlatformServices;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RockChat.UWP.Dialogs
{
    public sealed partial class TextInputDialog : ContentDialog
    {
        public TextInputDialog(InputDialogData data)
        {
            Data = data;
            InitializeComponent();
        }

        public InputDialogData Data { get; }
    }
}