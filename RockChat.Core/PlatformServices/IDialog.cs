using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RockChat.Core.PlatformServices
{
    public interface IDialog
    {
        Task<string?> ShowInput(InputDialogData data);
    }

    public class InputDialogData
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? InputDefaultValue { get; set; }
        public string ConfirmText { get; set; } = "Confirm";
        public string CancelText { get; set; } = "Cancel";
    }
}
