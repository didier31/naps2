using System;
using NAPS2.Config;

namespace NAPS2.ImportExport.Pdf
{
    public class PdfSettings
    {
        private PdfMetadata metadata;
        private PdfEncryption encryption;

        public PdfSettings()
        {
            metadata = new PdfMetadata();
            encryption = new PdfEncryption();
        }

        public string DefaultFileName { get; set; }

        public bool? SkipSavePrompt { get; set; }

        [Child]
        public PdfMetadata Metadata
        {
            get => metadata;
            set => metadata = value ?? throw new ArgumentNullException(nameof(value));
        }

        [Child]
        public PdfEncryption Encryption
        {
            get => encryption;
            set => encryption = value ?? throw new ArgumentNullException(nameof(value));
        }

        public PdfCompat? Compat { get; set; }
    }
}
