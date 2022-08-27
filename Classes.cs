using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
/// <summary>
/// 
/// </summary>

public class Settings {
    public bool enable_warnings, check_readme, deduce_file_type, autopaste_from_clipboard;
    public Settings() {
        enable_warnings = true;
        check_readme = true;
        deduce_file_type = true;
        autopaste_from_clipboard = false;
    }
    public void restore_default_settings() {
        enable_warnings = true;
        check_readme = true;
        deduce_file_type = true;
        autopaste_from_clipboard = false;
    }
}

public class PataponMessage : INotifyPropertyChanged {

    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged(string property_name = "") {
        if (PropertyChanged != null) {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(property_name));
        }
    }

    private int index;
    private string message;

    public int Index {
        get {
            return index;
        }
        set {
            index = value;
            NotifyPropertyChanged("Index");
        }
    }
    public string Message {
        get {
            return message;
        }
        set {
            message = value;
            NotifyPropertyChanged("Message");
        }
    }
    public PataponMessage(string msg) {
        Message = msg;
    }
}

/*public class MessageCollection : INotifyPropertyChanged {
    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged(string property_name = "")
    {
        if (PropertyChanged != null)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(property_name));
        }
    }
    public MessageCollection() {
    }
    private ObservableCollection<PataponMessage> items = new ObservableCollection<PataponMessage>();
    public ObservableCollection<PataponMessage> Items {
        get {
            return items;
        }
        set {
            NotifyPropertyChanged("Items");
            items = value;
        }
    }
    public void refresh(string property_name = "") {
        NotifyPropertyChanged(property_name);
    }
}*/
