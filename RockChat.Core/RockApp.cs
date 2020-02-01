using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using RockChat.Core.Models;
using RockChat.Core.PlatformServices;

namespace RockChat.Core
{
    public class RockApp
    {
        public static RockApp Current { get; private set; }
        public IContainer Container { get; private set; }
        internal ConcurrentDictionary<Guid, InstanceModel> ActiveInstance { get; } = new ConcurrentDictionary<Guid, InstanceModel>();


        internal Guid AddInstance(InstanceModel model)
        {
            var settings = this.Platform<ISettings>();
            var current = settings.Get("instance", new List<InstanceModel>());
            current.Add(model);
            settings.Set("instance", current);
            var guid = Guid.NewGuid();
            ActiveInstance.TryAdd(guid, model);
            return guid;
        }

        public Guid? GetDefaultInstance()
        {
            var settings = this.Platform<ISettings>();
            var current = settings.Get("instance", new List<InstanceModel>());
            if (!current.Any())
            {
                return null;
            }
            var guid = Guid.NewGuid();
            ActiveInstance.TryAdd(guid, current.FirstOrDefault());
            return guid;
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

    public static class RockAppExtensions
    {
        public static T Platform<T>(this object any)
        {
            return RockApp.Current.Container.Resolve<T>();
        }
    }
}
