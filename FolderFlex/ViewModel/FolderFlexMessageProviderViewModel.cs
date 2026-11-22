using FolderFlex.Util;
using FolderFlexCommon.Messages;
using FolderFlexCommon.Settings.ApplicationSettings;

namespace FolderFlex.ViewModel;

public sealed class FolderFlexMessageProviderViewModel : ObservableObject
{
    private readonly ApplicationSettings _settings;

    public FolderFlexMessageProviderViewModel(ApplicationSettings settings)
    {
        _settings = settings;
        LanguageOptions = [.. MessageMap.ListLanguages().Values];
        StatusMessage = MessageMap.GetMessage("select_to_start", Language);
        SelectedLanguageName = MessageMap.ListLanguages().GetValueOrDefault(Language)
            ?? (LanguageOptions.Count > 0 ? LanguageOptions[0] : string.Empty);
    }

    public IReadOnlyList<string> LanguageOptions { get; }

    public string SelectedLanguageName
    {
        get => field;
        set
        {
            if (!SetProperty(ref field, value))
                return;

            string? languageKey = MessageMap.GetLanguageKeyByDescription(value);
            if (!string.IsNullOrEmpty(languageKey) && !string.Equals(Language, languageKey, StringComparison.OrdinalIgnoreCase))
                Language = languageKey;
        }
    } = string.Empty;

    public bool SomenteCopiar
    {
        get => field;
        set
        {
            if (SetProperty(ref field, value))
                OnPropertyChanged(nameof(MoveLabel));
        }
    }

    public string Language
    {
        get => _settings.Language;
        set
        {
            if (string.Equals(_settings.Language, value, StringComparison.OrdinalIgnoreCase))
                return;

            _settings.Language = value;
            MessageMap.SetDefaultLanguage(value);

            StatusMessage = MessageMap.GetMessage("select_to_start", value);

            OnPropertyChanged();
            OnPropertyChanged(nameof(OriginLabel));
            OnPropertyChanged(nameof(DestinationLabel));
            OnPropertyChanged(nameof(MoveLabel));
            OnPropertyChanged(nameof(CopyLabel));
            OnPropertyChanged(nameof(RenameLabel));
            OnPropertyChanged(nameof(CancelLabel));
            OnPropertyChanged(nameof(AllDoneLabel));
            OnPropertyChanged(nameof(SeparateByExtensionLabel));
        }
    }

    public string StatusMessage
    {
        get => field;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public string AllDoneLabel
    {
        get => field;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public string OriginLabel => MessageMap.GetMessage("origin_label", Language);
    public string DestinationLabel => MessageMap.GetMessage("destination_label", Language);
    public string MoveLabel => SomenteCopiar ? MessageMap.GetMessage("copy_label", Language) : MessageMap.GetMessage("move_label", Language);
    public string CopyLabel => MessageMap.GetMessage("copy_label", Language);
    public string RenameLabel => MessageMap.GetMessage("rename_label", Language);
    public string CancelLabel => MessageMap.GetMessage("cancel_label", Language);
    public string SeparateByExtensionLabel => MessageMap.GetMessage("separate_by_extension_label", Language);
}
