using Eto.Forms;
using NAPS2.WinForms;

namespace NAPS2.EtoForms;

public abstract class EtoDialogBase : Dialog, IFormBase
{
    private IFormFactory? _formFactory;
    
    protected EtoDialogBase(Naps2Config config)
    {
        Config = config;
        FormStateController = new FormStateController(this, config);
    }

    public IFormStateController FormStateController { get; }

    public IFormFactory FormFactory
    {
        get => _formFactory ?? throw new InvalidOperationException();
        set => _formFactory = value;
    }

    public Naps2Config Config { get; set; }
}