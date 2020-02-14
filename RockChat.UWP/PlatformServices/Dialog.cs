using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using RockChat.Core.PlatformServices;
using RockChat.UWP.Dialogs;

namespace RockChat.UWP.PlatformServices
{
    class Dialog : IDialog
    {
        public async Task<string?> ShowInput(InputDialogData data)
        {
            var dialog = new TextInputDialog(data);
            var showResult = await dialog.ShowAsync();
            if (showResult == ContentDialogResult.Primary)
            {
                return dialog.Data.InputDefaultValue;
            }

            return null;
        }
    }
}
