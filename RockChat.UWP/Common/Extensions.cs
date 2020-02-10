using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace RockChat.UWP.Common
{
    internal static class Extensions
    {
        public static async Task<StorageFile> SaveCacheFile(this IRandomAccessStreamReference reference,
            string? name = null)
        {
            var file = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync($"{name ?? new Random().Next().ToString()}.jpg", CreationCollisionOption.GenerateUniqueName);
            using (var fstream = await file.OpenStreamForWriteAsync())
            {
                using var stream = await reference.OpenReadAsync();
                var decoder = await BitmapDecoder.CreateAsync(stream);
                var pixels = await decoder.GetPixelDataAsync();
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, fstream.AsRandomAccessStream());
                encoder.SetPixelData(decoder.BitmapPixelFormat, BitmapAlphaMode.Ignore,
                    decoder.OrientedPixelWidth, decoder.OrientedPixelHeight,
                    decoder.DpiX, decoder.DpiY,
                    pixels.DetachPixelData());
                await encoder.FlushAsync();
            }

            return file;
        }
    }
}
