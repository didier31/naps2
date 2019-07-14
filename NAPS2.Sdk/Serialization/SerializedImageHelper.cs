﻿using System;
using System.IO;
using Google.Protobuf;
using NAPS2.Images;
using NAPS2.Images.Storage;

namespace NAPS2.Serialization
{
    public static class SerializedImageHelper
    {
        public static SerializedImage Serialize(ImageContext imageContext, ScannedImage image, SerializeOptions options) =>
            Serialize(imageContext, image, image.Metadata, options);

        public static SerializedImage Serialize(ImageContext imageContext, ScannedImage.Snapshot snapshot, SerializeOptions options) =>
            Serialize(imageContext, snapshot.Source, snapshot.Metadata, options);

        private static SerializedImage Serialize(ImageContext imageContext, ScannedImage image, IImageMetadata metadata, SerializeOptions options)
        {
            MemoryStream thumbStream = null;
            var thumb = image.GetThumbnail();
            if (thumb != null && options.IncludeThumbnail)
            {
                thumbStream = imageContext.Convert<MemoryStreamStorage>(thumb, new StorageConvertParams { Lossless = true }).Stream;
            }

            var fileStorage = image.BackingStorage as FileStorage;
            if (fileStorage == null && options.RequireFileStorage)
            {
                throw new InvalidOperationException("FileStorage is required for serialization.");
            }

            MemoryStream imageStream = null;
            if (fileStorage == null)
            {
                imageStream = imageContext.Convert<MemoryStreamStorage>(image.BackingStorage, new StorageConvertParams()).Stream;
            }

            var result = new SerializedImage
            {
                TransferOwnership = options.TransferOwnership,
                MetadataXml = metadata.Serialize(),
                Thumbnail = thumbStream != null ? ByteString.FromStream(thumbStream) : ByteString.Empty,
                RenderedFilePath = options.RenderedFilePath ?? ""
            };
            if (fileStorage != null)
            {
                result.FilePath = fileStorage.FullPath;
            }
            else
            {
                result.FileContent = ByteString.FromStream(imageStream);
            }
            return result;
        }

        public static ScannedImage Deserialize(ImageContext imageContext, SerializedImage serializedImage, DeserializeOptions options)
        {
            IStorage storage;
            if (!string.IsNullOrEmpty(serializedImage.FilePath))
            {
                if (serializedImage.TransferOwnership)
                {
                    storage = new FileStorage(serializedImage.FilePath);
                }
                else if (options.ShareFileStorage)
                {
                    storage = new FileStorage(serializedImage.FilePath, true);
                }
                else
                {
                    // TODO: With this logic centralized, maybe we can remove UnownedFileStorage and just move the file copy logic here?
                    storage = new UnownedFileStorage(serializedImage.FilePath);
                }
            }
            else
            {
                var memoryStream = new MemoryStream(serializedImage.FileContent.ToByteArray());
                storage = new MemoryStreamStorage(memoryStream);
            }
            var scannedImage = serializedImage.TransferOwnership
                ? imageContext.CreateScannedImage(storage, serializedImage.MetadataXml, new StorageConvertParams())
                : imageContext.CreateScannedImage(storage, CreateStubMetadata(serializedImage.MetadataXml), new StorageConvertParams());
            var thumbnail = serializedImage.Thumbnail.ToByteArray();
            if (thumbnail.Length > 0)
            {
                var thumbnailStorage = new MemoryStreamStorage(new MemoryStream(thumbnail));
                scannedImage.SetThumbnail(imageContext.ConvertToImage(thumbnailStorage, new StorageConvertParams()));
            }
            return scannedImage;
        }

        private static IImageMetadata CreateStubMetadata(string metadataXml)
        {
            var metadata = new StubImageMetadata();
            metadata.Deserialize(metadataXml);
            return metadata;
        }

        public class SerializeOptions
        {
            public bool TransferOwnership { get; set; }

            public bool IncludeThumbnail { get; set; }

            public bool RequireFileStorage { get; set; }

            public string RenderedFilePath { get; set; }
        }

        public class DeserializeOptions
        {
            /// <summary>
            /// If true, the deserializer guarantees that the file storage will not be used for longer than the duration of the RPC call.
            /// In this way, files can be reused even if ownership isn't transferred to the callee.
            /// </summary>
            public bool ShareFileStorage { get; set; }
        }
    }
}
