﻿using System.IO;
using PdfSharp.Pdf;

namespace NAPS2.Images.Storage
{
    public class PdfConverters
    {
        private readonly ImageContext imageContext;

        public PdfConverters(ImageContext imageContext)
        {
            this.imageContext = imageContext;
        }

        [StorageConverter]
        public FileStorage ConvertToFile(PdfStorage input, StorageConvertParams convertParams)
        {
            var path = convertParams.Temporary
                ? Path.Combine(Paths.Temp, Path.GetRandomFileName())
                : imageContext.FileStorageManager.NextFilePath() + ".pdf";
            input.Document.Save(path);
            return new FileStorage(path);
        }

        [StorageConverter]
        public PdfStorage ConvertToMemory(FileStorage input, StorageConvertParams convertParams) => new PdfStorage(new PdfDocument(input.FullPath));
    }
}
