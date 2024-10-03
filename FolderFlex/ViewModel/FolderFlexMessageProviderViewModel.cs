using FolderFlex.Util;
using FolderFlexCommon.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FolderFlex.ViewModel
{
    public class FolderFlexMessageProviderViewModel: INotifyPropertyChanged
    {
        private string _language = "pt";

        public string Language
        {
            get => _language;
            set
            {
                _language = value;
                OnPropertyChanged("StatusMessage");
                OnPropertyChanged("OriginLabel");
                OnPropertyChanged("DestinationLabel");
                OnPropertyChanged("MoveLabel");
                OnPropertyChanged("CopyLabel");
                OnPropertyChanged("RenameLabel");
                OnPropertyChanged("CancelLabel");
            }
        }
        public string StatusMessage => MessageMap.GetMessage("select_to_start", Language);

        public string OriginLabel => MessageMap.GetMessage("origin_label", Language);

        public string DestinationLabel => MessageMap.GetMessage("destination_label", Language);

        public string MoveLabel => MessageMap.GetMessage("move_label", Language);

        public string CopyLabel => MessageMap.GetMessage("copy_label", Language);

        public string RenameLabel => MessageMap.GetMessage("rename_label", Language);
        public string CancelLabel => MessageMap.GetMessage("cancel_label", Language);
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public ICommand ChangeLanguageCommand { get; }

        public FolderFlexMessageProviderViewModel()
        {
            ChangeLanguageCommand = new RelayCommand<string>(ChangeLanguage);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void ChangeLanguage(string newLanguage)
        {
            Language = newLanguage;
        }


    }
}
