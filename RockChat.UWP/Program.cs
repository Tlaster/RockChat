using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;

namespace RockChat.UWP
{
    public static class Program
    {
        static void Main(string[] args)
        {
            var activatedArgs = AppInstance.GetActivatedEventArgs();
            if (AppInstance.RecommendedInstance != null)
            {
                AppInstance.RecommendedInstance.RedirectActivationTo();
            }
            else
            {
                var key = Guid.NewGuid().ToString();
                var instance = AppInstance.FindOrRegisterInstanceForKey(key);
                if (instance.IsCurrentInstance)
                {
                    Windows.UI.Xaml.Application.Start(p => new App());
                }
                else
                {
                    instance.RedirectActivationTo();
                }
            }
        }
    }
}
