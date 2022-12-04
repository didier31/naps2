using Eto.Drawing;
using Eto.Forms;
using NAPS2.Config.Model;
using NAPS2.EtoForms.Layout;
using NAPS2.ImportExport.Pdf;

namespace NAPS2.EtoForms.Ui;

public class PdfSettingsForm : EtoDialogBase
{
    private readonly DialogHelper _dialogHelper;

    private readonly TextBox _defaultFilePath = new();
    private readonly Button _chooseFolder = new() { Text = UiStrings.Ellipsis };
    private readonly LinkButton _placeholders = new() { Text = UiStrings.Placeholders };
    private readonly CheckBox _skipSavePrompt = new() { Text = UiStrings.SkipSavePrompt };
    private readonly CheckBox _singlePageFiles = new() { Text = UiStrings.SinglePageFiles };
    private readonly TextBox _title = new();
    private readonly TextBox _author = new();
    private readonly TextBox _subject = new();
    private readonly TextBox _keywords = new();
    private readonly CheckBox _encryptPdf = new() { Text = UiStrings.EncryptPdf };
    private readonly TextBox _ownerPassword = new();
    private readonly CheckBox _showOwnerPassword = new() { Text = UiStrings.Show };
    private readonly TextBox _userPassword = new();
    private readonly CheckBox _showUserPassword = new() { Text = UiStrings.Show };
    private readonly CheckBox _rememberSettings = new() { Text = UiStrings.RememberTheseSettings };
    private readonly Button _restoreDefaults = new() { Text = UiStrings.RestoreDefaults };

    private readonly List<CheckBox> _permissions = new()
    {
        new CheckBox { Text = UiStrings.AllowPrinting },
        new CheckBox { Text = UiStrings.AllowFullQualityPrinting },
        new CheckBox { Text = UiStrings.AllowDocumentModification },
        new CheckBox { Text = UiStrings.AllowDocumentAssembly },
        new CheckBox { Text = UiStrings.AllowContentCopying },
        new CheckBox { Text = UiStrings.AllowContentCopyingForAccessibility },
        new CheckBox { Text = UiStrings.AllowAnnotations },
        new CheckBox { Text = UiStrings.AllowFormFilling }
    };

    private readonly DropDown _compat = C.EnumDropDown<PdfCompat>(compat => compat switch
    {
        PdfCompat.Default => UiStrings.Default,
        PdfCompat.PdfA1B => "PDF/A-1b",
        PdfCompat.PdfA2B => "PDF/A-2b",
        PdfCompat.PdfA3B => "PDF/A-3b",
        PdfCompat.PdfA3U => "PDF/A-3u",
        _ => throw new ArgumentException()
    });

    public PdfSettingsForm(Naps2Config config, DialogHelper dialogHelper) : base(config)
    {
        _dialogHelper = dialogHelper;
        Title = UiStrings.PdfSettingsFormTitle;
        Icon = new Icon(1f, Icons.file_extension_pdf_small.ToEtoImage());

        UpdateValues(Config);
        UpdateEnabled();

        _restoreDefaults.Click += RestoreDefaults_Click;
        _defaultFilePath.TextChanged += DefaultFilePath_TextChanged;
        _encryptPdf.CheckedChanged += EncryptPdf_CheckedChanged;
        _showOwnerPassword.CheckedChanged += ShowOwnerPassword_CheckedChanged;
        _showUserPassword.CheckedChanged += ShowUserPassword_CheckedChanged;
        _placeholders.Click += Placeholders_Click;
        _chooseFolder.Click += ChooseFolder_Click;

        FormStateController.DefaultExtraLayoutSize = new Size(60, 0);
        FormStateController.FixedHeightLayout = true;
        LayoutController.Content = L.Column(
            C.Label(UiStrings.DefaultFilePathLabel),
            L.Row(
                _defaultFilePath.XScale().AlignCenter(),
                _chooseFolder.Width(40).MaxHeight(22)
            ).SpacingAfter(2),
            _placeholders,
            _skipSavePrompt,
            _singlePageFiles,
            L.GroupBox(
                UiStrings.Metadata,
                L.Column(
                    C.Label(UiStrings.TitleLabel),
                    _title,
                    C.Label(UiStrings.AuthorLabel),
                    _author,
                    C.Label(UiStrings.SubjectLabel),
                    _subject,
                    C.Label(UiStrings.KeywordsLabel),
                    _keywords
                )
            ),
            L.GroupBox(
                UiStrings.Encryption,
                L.Column(
                    _encryptPdf,
                    L.Row(
                        C.Label(UiStrings.OwnerPasswordLabel),
                        C.Filler(),
                        _showOwnerPassword
                    ).SpacingAfter(2),
                    _ownerPassword,
                    L.Row(
                        C.Label(UiStrings.UserPasswordLabel),
                        C.Filler(),
                        _showUserPassword
                    ).SpacingAfter(2),
                    _userPassword,
                    L.Column(_permissions.Expand()).Spacing(0)
                )
            ),
            L.GroupBox(
                UiStrings.Compatibility,
                _compat
            ),
            C.Filler(),
            _rememberSettings,
            L.Row(
                _restoreDefaults.MinWidth(140),
                C.Filler(),
                L.OkCancel(
                    C.OkButton(this, Save),
                    C.CancelButton(this))
            )
        );
    }

    private void UpdateValues(Naps2Config config)
    {
        _defaultFilePath.Text = config.Get(c => c.PdfSettings.DefaultFileName);
        _skipSavePrompt.Checked = config.Get(c => c.PdfSettings.SkipSavePrompt);
        _title.Text = config.Get(c => c.PdfSettings.Metadata.Title);
        _author.Text = config.Get(c => c.PdfSettings.Metadata.Author);
        _subject.Text = config.Get(c => c.PdfSettings.Metadata.Subject);
        _keywords.Text = config.Get(c => c.PdfSettings.Metadata.Keywords);
        _encryptPdf.Checked = config.Get(c => c.PdfSettings.Encryption.EncryptPdf);
        _ownerPassword.Text = config.Get(c => c.PdfSettings.Encryption.OwnerPassword);
        _userPassword.Text = config.Get(c => c.PdfSettings.Encryption.UserPassword);
        _permissions[0].Checked = config.Get(c => c.PdfSettings.Encryption.AllowPrinting);
        _permissions[1].Checked = config.Get(c => c.PdfSettings.Encryption.AllowFullQualityPrinting);
        _permissions[2].Checked = config.Get(c => c.PdfSettings.Encryption.AllowDocumentModification);
        _permissions[3].Checked = config.Get(c => c.PdfSettings.Encryption.AllowDocumentAssembly);
        _permissions[4].Checked = config.Get(c => c.PdfSettings.Encryption.AllowContentCopying);
        _permissions[5].Checked =
            config.Get(c => c.PdfSettings.Encryption.AllowContentCopyingForAccessibility);
        _permissions[6].Checked = config.Get(c => c.PdfSettings.Encryption.AllowAnnotations);
        _permissions[7].Checked = config.Get(c => c.PdfSettings.Encryption.AllowFormFilling);
        _compat.SelectedIndex = (int) config.Get(c => c.PdfSettings.Compat);
        _rememberSettings.Checked = config.Get(c => c.RememberPdfSettings);
    }

    private void UpdateEnabled()
    {
        _skipSavePrompt.Enabled = Path.IsPathRooted(_defaultFilePath.Text);

        bool encrypt = _encryptPdf.IsChecked();
        _userPassword.Enabled = _ownerPassword.Enabled = _showOwnerPassword.Enabled = _showUserPassword.Enabled =
            _userPassword.Enabled = _ownerPassword.Enabled = encrypt;
        foreach (var perm in _permissions)
        {
            perm.Enabled = encrypt;
        }

        _compat.Enabled = !Config.AppLocked.TryGet(c => c.PdfSettings.Compat, out _);
    }

    private void Save()
    {
        var pdfSettings = new PdfSettings
        {
            DefaultFileName = _defaultFilePath.Text,
            SkipSavePrompt = _skipSavePrompt.IsChecked(),
            Metadata = new PdfMetadata
            {
                Title = _title.Text,
                Author = _author.Text,
                Subject = _subject.Text,
                Keywords = _keywords.Text
            },
            Encryption = new PdfEncryption
            {
                EncryptPdf = _encryptPdf.IsChecked(),
                OwnerPassword = _ownerPassword.Text,
                UserPassword = _userPassword.Text,
                AllowPrinting = _permissions[0].IsChecked(),
                AllowFullQualityPrinting = _permissions[1].IsChecked(),
                AllowDocumentModification = _permissions[2].IsChecked(),
                AllowDocumentAssembly = _permissions[3].IsChecked(),
                AllowContentCopying = _permissions[4].IsChecked(),
                AllowContentCopyingForAccessibility = _permissions[5].IsChecked(),
                AllowAnnotations = _permissions[6].IsChecked(),
                AllowFormFilling = _permissions[7].IsChecked()
            },
            Compat = (PdfCompat) _compat.SelectedIndex
        };

        bool remember = _rememberSettings.IsChecked();
        var writeScope = remember ? Config.User : Config.Run;
        Config.Run.Remove(c => c.PdfSettings);
        Config.User.Remove(c => c.PdfSettings);
        Config.User.Set(c => c.RememberPdfSettings, remember);
        writeScope.Set(c => c.PdfSettings, pdfSettings);
    }

    private void RestoreDefaults_Click(object sender, EventArgs e)
    {
        UpdateValues(Config.DefaultsOnly);
        UpdateEnabled();
    }

    private void DefaultFilePath_TextChanged(object sender, EventArgs e)
    {
        UpdateEnabled();
    }

    private void EncryptPdf_CheckedChanged(object sender, EventArgs e)
    {
        UpdateEnabled();
    }

    private void ShowOwnerPassword_CheckedChanged(object sender, EventArgs e)
    {
        // TODO: Switch between password box
        // txtOwnerPassword.UseSystemPasswordChar = !cbShowOwnerPassword.Checked;
    }

    private void ShowUserPassword_CheckedChanged(object sender, EventArgs e)
    {
        // txtUserPassword.UseSystemPasswordChar = !cbShowUserPassword.Checked;
    }

    private void Placeholders_Click(object sender, EventArgs eventArgs)
    {
        // var form = FormFactory.Create<PlaceholdersForm>();
        // form.FileName = _defaultFilePath.Text;
        // if (form.ShowModal() == DialogResult.OK)
        // {
        //     _defaultFilePath.Text = form.FileName;
        // }
    }

    private void ChooseFolder_Click(object sender, EventArgs e)
    {
        if (_dialogHelper.PromptToSavePdf(_defaultFilePath.Text, out string? savePath))
        {
            _defaultFilePath.Text = savePath!;
        }
    }
}