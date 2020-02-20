using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using RockChat.Core.i18n;
using RockChat.Core.Models;
using RockChat.Core.PlatformServices;

namespace RockChat.Core
{
    public class RockApp
    {
        public static RockApp Current { get; private set; }
        public IContainer Container { get; private set; }
        public List<InstanceModel> ActiveInstance { get; } = new List<InstanceModel>();


        internal void AddInstance(InstanceModel model)
        {
            var settings = this.Platform<ISettings>();
            var current = settings.Get("instance", new List<InstanceModel>());
            current.Add(model);
            settings.Set("instance", current);
            ActiveInstance.Add(model);
        }

        internal void GetAllInstance()
        {
            ActiveInstance.Clear();
            var settings = this.Platform<ISettings>();
            var current = settings.Get("instance", new List<InstanceModel>());
            if (!current.Any())
            {
                return;
            }

            ActiveInstance.AddRange(current);
        }


        public static void Init(Action<ContainerBuilder> func)
        {
            var builder = new ContainerBuilder();
            func.Invoke(builder);
            Current = new RockApp
            {
                Container = builder.Build()
            };
        }
    }

    internal static class RockAppExtensions
    {
        public static T Platform<T>(this object any)
        {
            return RockApp.Current.Container.Resolve<T>();
        }
    }
}
