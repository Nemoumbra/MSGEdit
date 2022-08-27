using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MSGEdit {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window {
        public Settings settings;
        public bool is_editing_in_progress = false;
        public bool magic_changed = false;
        public bool selection_lock = false;

        public string current_file_path = "";
        public string current_file_name = "";
        public string current_file_ext = "";

        public bool is_about_window_shown = false;
        public AboutWindow aboutWindow;

        public bool is_search_window_shown = false;
        public SearchWindow searchWindow;

        public Key[] allowed_keys;
        public string allowed_chars = @"[0-9]";

        public string readme_message = "The directory where MSGEdit.exe is located does not contain a file \"readme.txt\".\nWould you like to restore it?";
        public string unload_file_warning = "Warning! This action will unload the file you are currently editing from the memory. Proceed?";
        public string irreversible_action_warning = "Warning! This action cannot be undone, are you sure this is what you want?";

        public MSGtoTXT core;

        public ObservableCollection<PataponMessage> Items;
        public void make_readme() {
            // This method is run from a try-catch block
            List<string> lines = new List<string>();
            // The following code is auto-generated based on a manually made file

            lines.Add("MSGEdit 4.0, by Nemoumbra © 2022");
            lines.Add("");
            lines.Add("The official git repository containing the source code can be found at");
            lines.Add("https://github.com/Nemoumbra/MSGEdit");
            lines.Add("");
            lines.Add("This tool was designed to be as user-friendly as possible. Because of that, all input fields have labels next to them explaning their meaning.");
            lines.Add("");
            lines.Add("USAGE:");
            lines.Add("The tool supports three ways of working with files: ");
            lines.Add(" - making a new one from scratch or");
            lines.Add(" - loading some already existing file to view or edit it (which is intended to be the most popular way of using MSGEdit) or");
            lines.Add(" - converting files from one format to another from command line");
            lines.Add("");
            lines.Add("The settings file can be used to adjust MSGEdit to one's preferences. ");
            lines.Add("The format is pretty simple:");
            lines.Add("");
            lines.Add("first_setting_name=first_setting_value");
            lines.Add("second_setting_name=second_setting_value");
            lines.Add("...");
            lines.Add("last_setting_name=last_setting_value");
            lines.Add("");
            lines.Add("(You can have a look at the file provided with the tool or the automatically generated one).");
            lines.Add("Omitting a setting makes MSGEdit go with the default value for it.");
            lines.Add("");
            lines.Add("If enable_warnings is set to false, no confirmation will ever be asked.");
            lines.Add("If check_readme is set to false, MSGEdit won't offer to restore the readme if it is not present in tool's directory.");
            lines.Add("If deduce_file_type is set to true, the tool will attemp to figure out the file type (MSG or TXT) based on its content. This setting also changes the way command line arguments are processed.");
            lines.Add("If autopaste_from_clipboard is set to true, the tool will paste the clipboard contents into the search field when Ctrl+F is pressed (unless the data contains line breaks).");
            lines.Add("");
            lines.Add("You may find it easier to get used to the tool if you start by creating a new file and try to play with as many buttons as possible.");
            lines.Add("");
            lines.Add("Please note that:");
            lines.Add(" - entering a message index in the field will highlight the entry in the messages list and load the message into the textbox, so please make sure to apply your changes before moving to the next entry you want to edit");
            lines.Add(" - selecting an entry in the list will enter the message index in the field and load it into the textbox as well, so exercise caution");
            lines.Add(" - saving the entry with an index which does not correspond to any message in the file will result in the tool creating a new entry and filling all the entries between with blank lines");
            lines.Add(" - MSGEdit does not support undoing edits and sometimes you will be asked to corfirm your action, unless you explicitly forbid doing so");
            lines.Add("");
            lines.Add("Please bear in mind that, unlike Patapon 3, the Patapon 1 and Patapon 2 message files have a *.pac extension (not to be confused with so called Patapon Assembly Code).");
            lines.Add("");
            lines.Add("SEARCH:");
            lines.Add("Pressing Ctrl+F makes a feature-rich Search window appear. You can use a variety of settings to make the search more accurate.");
            lines.Add("The button \"Find all\" puts references to found messages into a combobox. Select something from there to go to the message in the main window.");
            lines.Add("Pressing Escape or clicking \"Cancel\" closes the window and pressing Enter or F5 is equivalent to clicking \"Find next\".");
            lines.Add("The \"Help\" button opens the About window.");
            lines.Add("");
            lines.Add("The about window contains a short explanation of MSGEdit functionality, it can also be accessed by pressing F1.");
            lines.Add("");
            lines.Add("");
            lines.Add("Command line arguments support (for advanced users)");
            lines.Add("Before or instead of showing the window, MSGEdit can attempt to perform some actions according to given command line arguments.");
            lines.Add("As of the release version 4.0 MSGEdit supports 4 such actions:");
            lines.Add("");
            lines.Add("1) No command line arguments.");
            lines.Add("No actions (normal startup).");
            lines.Add("");
            lines.Add("2) A single file path.");
            lines.Add("The tool attempts to load the file on startup (if settings state that the file type must not be deduced, a question will be asked).");
            lines.Add("");
            lines.Add("3) A single file path, followed by either -txt or -msg.");
            lines.Add("Same as 2), but the setting is ignored as the tool will use the format specified as the second argument.");
            lines.Add("");
            lines.Add("4) -convert, followed by a list of file paths (if settings state that the file type must not be deduced, it is necessary to specify the type after every file).");
            lines.Add("The tool attempts to convert the files from MSG to TXT and vice-versa. The original files are preserved, the names of new ones are formed by appending the corresponding extension to original file names.");
            lines.Add("Unsuccessful conversions do not halt the execution and after finishing its job MSGEdit shuts down.");
            lines.Add("");
            lines.Add("");
            lines.Add("Credits:");
            lines.Add("The unique way of organizing the data inside a TXT format was created by Owocek in his MSGTools tool.");
            lines.Add("The background image was provided by WondaOxigen.");
            lines.Add("The icons are based on the Patapon 3 textures.");
            lines.Add("");
            lines.Add("Finally, I express my gratitude to everyone who researched the MSG file format.");
            lines.Add("");
            lines.Add("");
            lines.Add("There is no known way to crash this application, but you can contact the author in the");
            lines.Add("Patapon Modding Discord server regarding any found bugs.");
            lines.Add("");
            lines.Add("");
            lines.Add("LINKS");
            lines.Add("Owocek's website:");
            lines.Add("http://owocektv.usermd.net/modding/");
            lines.Add("");
            lines.Add("Modding Discord server join link:");
            lines.Add("https://discord.gg/ZsZmgA7");
            lines.Add("");
            lines.Add("Author's Youtube channel:");
            lines.Add("https://www.youtube.com/channel/UCRy1IkH2QsmZ4R2NRE2X-Xg");


            // Auto-generated section end
            using (StreamWriter stream = new StreamWriter("readme.txt", false, Encoding.Unicode)) {
                for (int i = 0; i < lines.Count - 1; ++i) {
                    stream.WriteLine(lines[i]);
                }
                stream.Write(lines[lines.Count - 1]);
            }
        }
        public void check_readme() {
            try {
                if (!File.Exists("readme.txt") && MessageBox.Show(readme_message, "Suggestion", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                    make_readme();
                }
            }
            catch (Exception except) {
                MessageBox.Show("Readme file creation error: " + except.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public MainWindow() {
            InitializeComponent();

            allowed_keys = new Key[] {
                Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9, Key.D0,
                Key.NumPad0, Key.NumPad1, Key.NumPad2, Key.NumPad3, Key.NumPad4, Key.NumPad5,
                Key.NumPad6, Key.NumPad7, Key.NumPad8, Key.NumPad9,
                Key.Left, Key.Right, Key.Delete, Key.Insert, Key.Back, Key.Tab
            };

            DataObject.AddPastingHandler(EntryIndexTextBox, OnPaste);
            DataObject.AddPastingHandler(MagicValueTextBox, OnPaste);
            DataObject.AddPastingHandler(RandomizeSeedTextbox, OnPaste);
            DataObject.AddPastingHandler(EraseStartTextBox, OnPaste);
            DataObject.AddPastingHandler(EraseCountTextBox, OnPaste);
            DataObject.AddPastingHandler(InsertIndexTextBox, OnPaste);

            Items = new ObservableCollection<PataponMessage>();
            FileContentListBox.ItemsSource = Items;           
        }

        public void hide(UIElement elem) {
            elem.Visibility = Visibility.Hidden;
        }
        public void show(UIElement elem) {
            elem.Visibility = Visibility.Visible;
        }

        public void show_title_screen() {
            show(LoadTXTButton);
            show(LoadMSGButton);
            show(DragAndDropRectangle);
            show(ResumeEditingButton);
            show(NewFileButton);
            show(CurrentFilePathTextBox);
            show(CurrentFilePathTextBlock);
        }
        public void hide_title_screen() {
            hide(LoadTXTButton);
            hide(LoadMSGButton);
            hide(DragAndDropRectangle);
            hide(ResumeEditingButton);
            hide(NewFileButton);
            hide(CurrentFilePathTextBox);
            hide(CurrentFilePathTextBlock);
        }

        public void show_workshop() {
            show(FileContentListBox);

            //show(EntriesCountLabel);
            show(MsgCountWrapPanel);

            show(MagicValueTextBox);
            show(MagicValueLabel);

            show(InsertButton);
            show(InsertIndexTextBox);
            show(InsertStringTextBox);
            show(InsertIndexLabel);

            show(EraseButton);
            show(EraseStartTextBox);
            show(EraseCountTextBox);
            show(EraseStartLabel);
            show(EraseCountLabel);

            show(SaveTXTButton);
            show(SaveMSGButton);

            show(StopEditingButton);

            show(EntryIndexTextBox);
            show(EntryStringTextBox);
            show(SaveEntryButton);
            show(EntryIndexLabel);

            show(RandomizeButton);
            show(RandomizeLabel);
            show(RandomizeSeedTextbox);
        }
        public void hide_workshop() {
            hide(FileContentListBox);

            //hide(EntriesCountLabel);
            hide(MsgCountWrapPanel);

            hide(MagicValueTextBox);
            hide(MagicValueLabel);

            hide(InsertButton);
            hide(InsertIndexTextBox);
            hide(InsertStringTextBox);
            hide(InsertIndexLabel);

            hide(EraseButton);
            hide(EraseStartTextBox);
            hide(EraseCountTextBox);
            hide(EraseStartLabel);
            hide(EraseCountLabel);

            hide(SaveTXTButton);
            hide(SaveMSGButton);

            hide(StopEditingButton);

            hide(EntryIndexTextBox);
            hide(EntryStringTextBox);
            hide(SaveEntryButton);
            hide(EntryIndexLabel);

            hide(RandomizeButton);
            hide(RandomizeLabel);
            hide(RandomizeSeedTextbox);
        }

        public void show_about_window() {
            if (!is_about_window_shown) {
                aboutWindow = new AboutWindow();
                is_about_window_shown = true;
                aboutWindow.Owner = this;
                aboutWindow.Show();
            }
            else {
                aboutWindow.Activate();
                if (aboutWindow.WindowState == WindowState.Minimized) {
                    aboutWindow.WindowState = WindowState.Normal;
                }
            }
        }
        public void show_search_window() {
            if (is_editing_in_progress) {
                if (!is_search_window_shown) {
                    searchWindow = new SearchWindow();
                    is_search_window_shown = true;
                    searchWindow.allowed_keys = allowed_keys;
                    searchWindow.Owner = this;
                    searchWindow.Show();
                }
                else {
                    searchWindow.Activate();
                    if (searchWindow.WindowState == WindowState.Minimized) {
                        searchWindow.WindowState = WindowState.Normal;
                    }
                }
            }
        }

        public void save_file(string filename, PataponMessageFormat format) {
            switch (format) {
                case PataponMessageFormat.TXT: {
                    core.SaveTXT(filename);
                    break;
                }
                case PataponMessageFormat.MSG: {
                    core.SaveMSG(filename);
                    break;
                }
            }
            if (core.LastError != "") {
                MessageBox.Show($"File saving failed: {core.LastError}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void load_file(string filename) {
            core.LoadAny(filename);
            if (!core.isLoaded) {
                MessageBox.Show($"File loading failed: {core.LastError}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // TO DO: think about a better way to manage this...
                hide(ResumeEditingButton);
                return;
            }

            hide_title_screen();
            show_workshop();
            is_editing_in_progress = true;

            // Prepare Items for the listbox
            Items.Clear();
            string[] entries = core.Content(); // Messages with trailing zero-bytes
            int i = 0;
            foreach (string entry in entries) {
                PataponMessage message = new PataponMessage(entry.Trim('\0'));
                message.Index = i;
                Items.Add(message);
                ++i;
            }

            if (core.Count > 0) {
                FileContentListBox.ScrollIntoView(FileContentListBox.Items[0]); // Reset the listbox
            }

            EntryIndexTextBox.Text = ""; // This triggers the TextChanged event which sets EntryStringTextBox.Text to "" for us
            MagicValueTextBox.Text = core.Magic.ToString();

            // EntriesCountLabel.Content = $"{core.Count.ToString()} entries";
            MsgCountTextBox.Text = core.Count.ToString();

            InsertStringTextBox.Text = "String to be inserted";
            InsertIndexTextBox.Text = "0";

            EraseStartTextBox.Text = "0";
            EraseCountTextBox.Text = "1";

            RandomizeSeedTextbox.Text = "0";

            this.Title = $"MSGEdit - [{filename}]";
            CurrentFilePathTextBox.Text = filename;

            current_file_path = filename;
            current_file_name = System.IO.Path.GetFileName(filename); // With extension
            current_file_ext = System.IO.Path.GetExtension(current_file_path); // With dot
        }
        public void load_file(string filename, PataponMessageFormat format) {
            switch (format) {
                case PataponMessageFormat.TXT: {
                    core.LoadTXT(filename);
                    break;
                }
                case PataponMessageFormat.MSG: {
                    core.LoadMSG(filename);
                    break;
                }
            }
            if (!core.isLoaded) {
                MessageBox.Show($"File loading failed: {core.LastError}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // TO DO: think about a better way to manage this...
                hide(ResumeEditingButton);
                return;
            }

            hide_title_screen();
            show_workshop();
            is_editing_in_progress = true;

            // Prepare Items for the listbox
            Items.Clear();
            string[] entries = core.Content(); // Messages with trailing zero-bytes
            int i = 0;
            foreach (string entry in entries) {
                PataponMessage message = new PataponMessage(entry.Trim('\0'));
                message.Index = i;
                Items.Add(message);
                ++i;
            }

            if (core.Count > 0) {
                FileContentListBox.ScrollIntoView(FileContentListBox.Items[0]); // Reset the listbox
            }

            EntryIndexTextBox.Text = ""; // This triggers the TextChanged event which sets EntryStringTextBox.Text to "" for us
            MagicValueTextBox.Text = core.Magic.ToString();

            // EntriesCountLabel.Content = $"{core.Count.ToString()} entries";
            MsgCountTextBox.Text = core.Count.ToString();

            InsertStringTextBox.Text = "String to be inserted";
            InsertIndexTextBox.Text = "0";

            EraseStartTextBox.Text = "0";
            EraseCountTextBox.Text = "1";

            RandomizeSeedTextbox.Text = "0";

            this.Title = $"MSGEdit - [{filename}]";
            CurrentFilePathTextBox.Text = filename;

            current_file_path = filename;
            current_file_name = System.IO.Path.GetFileName(filename); // With extension
            current_file_ext = System.IO.Path.GetExtension(current_file_path); // With dot
        }

        private void LoadTXTButton_Click(object sender, RoutedEventArgs e) {
            // Available only when the editing is not in progress
            OpenFileDialog choose_file_menu = new OpenFileDialog();
            choose_file_menu.Filter = "Patapon message files (*.txt)|*.txt";

            if (choose_file_menu.ShowDialog() == true) {
                load_file(choose_file_menu.FileName, PataponMessageFormat.TXT);
            }
        }

        private void LoadMSGButton_Click(object sender, RoutedEventArgs e) {
            // Available only when the editing is not in progress
            OpenFileDialog choose_file_menu = new OpenFileDialog();
            choose_file_menu.Filter = "Patapon message files (*.msg, *.pac)|*.msg;*.pac";
            choose_file_menu.DefaultExt = ".msg";

            if (choose_file_menu.ShowDialog() == true) {
                load_file(choose_file_menu.FileName, PataponMessageFormat.MSG);
            }
        }

        private void SaveTXTButton_Click(object sender, RoutedEventArgs e) {
            // Available only when the editing is in progress
            SaveFileDialog choose_file_menu = new SaveFileDialog();
            choose_file_menu.Filter = "Patapon message files (*.txt)|*.txt";

            if (current_file_path != "") { // If the file has been loaded and not created from scratch
                string filename_prompt = current_file_name;
                if (current_file_ext == "") {
                    filename_prompt += ".txt";
                }
                else {
                    if (current_file_ext != ".txt") {
                        filename_prompt += ".txt";
                    }
                }
                choose_file_menu.FileName = filename_prompt;
            }
            // No prompt if the file is newly made

            if (choose_file_menu.ShowDialog() == true) {
                save_file(choose_file_menu.FileName, PataponMessageFormat.TXT);
            }
        }

        private void SaveMSGButton_Click(object sender, RoutedEventArgs e) {
            // Available only when the editing is in progress
            SaveFileDialog choose_file_menu = new SaveFileDialog();
            choose_file_menu.Filter = "Patapon message files (*.msg, *.pac)|*.msg;*.pac";

            if (current_file_path != "") { // If the file has been loaded and not created from scratch
                string filename_prompt = current_file_name;
                if (current_file_ext == "") {
                    filename_prompt += ".msg";
                }
                else {
                    if (current_file_ext != ".msg") {
                        filename_prompt += ".msg";
                    }
                }
                choose_file_menu.FileName = filename_prompt;
            }
            
            if (choose_file_menu.ShowDialog() == true) {
                save_file(choose_file_menu.FileName, PataponMessageFormat.MSG);
            }
        }

        private void StopEditingButton_Click(object sender, RoutedEventArgs e) {
            // Available only when the editing is in progress
            hide_workshop();
            show_title_screen();
        }

        private void ResumeEditingButton_Click(object sender, RoutedEventArgs e) {
            // Available only when the editing is not in progress
            hide_title_screen();
            show_workshop();
        }

        private void EraseButton_Click(object sender, RoutedEventArgs e) {
            // Available only when the editing is in progress
            if (!settings.enable_warnings || MessageBox.Show(irreversible_action_warning, "Warning",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) {

                int start_index = 0;
                try {
                    start_index = Convert.ToInt32(EraseStartTextBox.Text);
                }
                catch (Exception except) {
                    MessageBox.Show($"Start index format error: {except.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int count;
                try {
                    count = Convert.ToInt32(EraseCountTextBox.Text);
                }
                catch (Exception except) {
                    MessageBox.Show($"Count format error: {except.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                core.Erase(start_index, count);
                if (core.LastError != "") {
                    MessageBox.Show($"Entries erasement failed: {core.LastError}", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else {
                    // Successful erasure
                    int saved_index = FileContentListBox.SelectedIndex;
                    
                    for (int i = 0; i < count; ++i) {
                        Items.RemoveAt(start_index);
                    }
                    for (int i = start_index; i < Items.Count; ++i) {
                        Items[i].Index = Items[i].Index - count;
                    }
                    // EntriesCountLabel.Content = $"{core.Count.ToString()} entries";
                    MsgCountTextBox.Text = core.Count.ToString();

                    if (start_index <= saved_index) {
                        // If the EntryIndexTextBox.Text needs to be adjusted
                        // Please note that if nothing was selected, index is compared to -1. This never evaluates to true.
                        if (saved_index < start_index + count) {
                            // Same as saved_index <= start_index + count - 1 <=>
                            // <=> EntryIndexTextBox.Text should be assigned an empty string
                            EntryIndexTextBox.Text = "";
                        }
                        else {
                            // Just decrement EntryIndexTextBox.Text by count
                            EntryIndexTextBox.Text = (saved_index - count).ToString();
                        }
                    }
                }
            }
        }

        private void InsertButton_Click(object sender, RoutedEventArgs e) {
            // Available only when the editing is in progress
            int index = 0;
            try {
                index = Convert.ToInt32(InsertIndexTextBox.Text);
            }
            catch (Exception except) {
                MessageBox.Show($"Index format error: {except.Message}", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            core.Insert(index, InsertStringTextBox.Text);
            if (core.LastError != "") {
                MessageBox.Show($"Insertion failed: {core.LastError}", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else {
                // Successful insertion
                bool adjust_current_index = index <= FileContentListBox.SelectedIndex;
                // If the EntryIndexTextBox.Text needs to be adjusted
                // Please note that if nothing is selected, index is compared to -1. This never evaluates to true.

                PataponMessage message = new PataponMessage(InsertStringTextBox.Text);
                message.Index = index;

                for (int i = index; i < Items.Count; ++i) {
                    Items[i].Index = Items[i].Index + 1;
                }

                Items.Insert(index, message);

                // EntriesCountLabel.Content = $"{core.Count.ToString()} entries";
                MsgCountTextBox.Text = core.Count.ToString();
                if (adjust_current_index) {
                    EntryIndexTextBox.Text = FileContentListBox.SelectedIndex.ToString();
                }
            }
        }

        private void SaveEntryButton_Click(object sender, RoutedEventArgs e) {
            // Available only when the editing is in progress
            int index = 0;
            try {
                index = Convert.ToInt32(EntryIndexTextBox.Text);
            }
            catch (Exception except) {
                MessageBox.Show($"Index format error: {except.Message}", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            core[index] = EntryStringTextBox.Text;
            if (core.LastError != "") {
                MessageBox.Show($"Assignment failed: {core.LastError}", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else {
                while (index >= Items.Count) {
                    PataponMessage message = new PataponMessage("");
                    message.Index = Items.Count;
                    Items.Add(message);
                }
                Items[index].Message = EntryStringTextBox.Text;
            }
        }

        private void EntryIndexTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (selection_lock) {
                // That was not user's input
                selection_lock = false;
                return;
            }
            // Available only when the editing is in progress
            if (EntryIndexTextBox.Text != "") {
                try {
                    int index = Convert.ToInt32(EntryIndexTextBox.Text);
                    if (index < core.Count) {
                        EntryStringTextBox.Text = core[index];
                        if (index != FileContentListBox.SelectedIndex) {
                            selection_lock = true;
                            FileContentListBox.SelectedIndex = index;
                            FileContentListBox.ScrollIntoView(FileContentListBox.SelectedItem);
                        }
                    }
                }
                catch (Exception) {
                    // Do nothing
                }
            }
            else {
                EntryStringTextBox.Text = "";
            }
        }

        private void DragAndDropRectangle_Drop(object sender, DragEventArgs e) {
            string filename = "";
            try {
                if (e.Data.GetFormats().Contains(DataFormats.FileDrop)) {
                    filename = ((e.Data.GetData(DataFormats.FileDrop)) as string[])[0];
                }
            }
            catch (Exception except) {
                MessageBox.Show($"Drop error: {except.Message}", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // If editing is not in progress, we accept the drop
            // If editing is in progress, but warnings are disabled, we still accept the drop
            // If editing is in progress and the warnings are enabled, we let the used decide

            if (!is_editing_in_progress || !settings.enable_warnings || MessageBox.Show(unload_file_warning, 
                "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) {

                if (!settings.deduce_file_type) {
                    MessageBoxResult res = MessageBox.Show("Does this file contain MSG?", "Question",
                        MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (res == MessageBoxResult.Yes) {
                        load_file(filename, PataponMessageFormat.MSG);
                    }
                    else if (res == MessageBoxResult.No) {
                        load_file(filename, PataponMessageFormat.TXT);
                    }
                    // Else do nothing
                }
                else {
                    load_file(filename);
                }
            }
        }

        private void MagicValueTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            magic_changed = true;
        }

        private void MagicValueTextBox_LostFocus(object sender, RoutedEventArgs e) {
            if (magic_changed && MagicValueTextBox.Text != "") {
                try {
                    core.Magic = Convert.ToInt32(MagicValueTextBox.Text);
                }
                catch (Exception) {
                    // Do nothing
                }
                magic_changed = false;
            }
        }

        private void NewFileButton_Click(object sender, RoutedEventArgs e) {

            // If editing is not in progress, we make a new file
            // If editing is in progress, but warnings are disabled, we still make a new file
            // If editing is in progress and the warnings are enabled, we let the used decide
            if (!is_editing_in_progress || !settings.enable_warnings || MessageBox.Show(unload_file_warning, 
                "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) {
                core.MakeNewFile();

                hide_title_screen();
                show_workshop();
                is_editing_in_progress = true;

                Items.Clear();

                MagicValueTextBox.Text = core.Magic.ToString();
                // EntriesCountLabel.Content = $"{core.Count.ToString()} entries";
                MsgCountTextBox.Text = core.Count.ToString();

                EntryIndexTextBox.Text = ""; // This triggers the TextChanged event which sets EntryStringTextBox.Text to "" for us

                InsertStringTextBox.Text = "String to be inserted";
                InsertIndexTextBox.Text = "0";

                EraseStartTextBox.Text = "0";
                EraseCountTextBox.Text = "1";

                RandomizeSeedTextbox.Text = "0";

                this.Title = "MSGEdit [Untitled]";

                CurrentFilePathTextBox.Text = "None (new file is being made)";

                current_file_path = "";
                current_file_name = "";
                current_file_ext = "";
            }
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e) {
            var text = e.SourceDataObject.GetData(DataFormats.UnicodeText);
            if (!(text as string).All(char.IsDigit)) {
                e.CancelCommand();
            }
        }

        private void RandomizeButton_Click(object sender, RoutedEventArgs e) {
            if (core.Count == 0) {
                MessageBox.Show("There is nothing to randomize!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (!settings.enable_warnings || MessageBox.Show(irreversible_action_warning, "Warning", 
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) {
                try {
                    int seed = Convert.ToInt32(RandomizeSeedTextbox.Text);
                    core.Randomize(seed);
                    string[] entries = core.Content();
                    int i = 0;
                    foreach (string entry in entries) {
                        Items[i].Message = entry.Trim('\0');
                        ++i;
                    }
                }
                catch (Exception except) {
                    MessageBox.Show($"Randomization failed! Error: {except.Message}", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void FileContentListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (selection_lock) {
                // That was not user's input
                selection_lock = false;
                return;
            }
            if (FileContentListBox.SelectedIndex != -1) {
                selection_lock = true;
                EntryIndexTextBox.Text = FileContentListBox.SelectedIndex.ToString();
                EntryStringTextBox.Text = (FileContentListBox.SelectedItem as PataponMessage).Message;
            }
        }

        private void MyWindow_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.F1) {
                show_about_window();
            }

            if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))) {
                return;
            }

            if (Keyboard.IsKeyDown(Key.F)) {
                show_search_window();
            }

            //if (Keyboard.IsKeyDown(Key.O)) {
            //    // just a stump
            //}
            //if (Keyboard.IsKeyDown(Key.S)) {
            //    // just a stump
            //}
        }

        private void FilterInput(object sender, KeyEventArgs e) {
            if (!allowed_keys.Contains(e.Key)) {
                // If this key is not in our whitelist
                if (!(
                    (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                    (Keyboard.IsKeyDown(Key.C) || Keyboard.IsKeyDown(Key.V) || Keyboard.IsKeyDown(Key.Z) || Keyboard.IsKeyDown(Key.Y))
                    )) {
                    // not "Ctrl + C", not "Ctrl + V", not "Ctrl + Z" and not "Ctrl + Y"
                    e.Handled = true;
                }
            }
        }

        private void CurrentFilePathTextBox_SizeChanged(object sender, SizeChangedEventArgs e) {
            if (e.WidthChanged) {
                //MessageBox.Show($"{e.Source.ToString()}");
                //MessageBox.Show($"{e.PreviousSize}\n{e.NewSize}");
                Thickness new_margin = new Thickness(
                (MyWindow.ActualWidth - CurrentFilePathTextBox.ActualWidth) / 2,
                CurrentFilePathTextBox.Margin.Top,
                0,
                0);
                CurrentFilePathTextBox.Margin = new_margin;
            }
        }

        // Here are the methods that will be erased soon if I don't come up with some ideas how to put them to use
        private void EraseCountTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            
        }
        private void EraseStartTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            
        }
        private void MagicValueTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            
        }

        // These methods have the same body => they have been replaced with FilterInput method
        private void InsertIndexTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (!allowed_keys.Contains(e.Key)) {
                // If this key is not in our whitelist
                if (!(
                    (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                    (Keyboard.IsKeyDown(Key.C) || Keyboard.IsKeyDown(Key.V))
                    )) {
                    // not "Ctrl + C" and not "Ctrl + V"
                    e.Handled = true;
                }
            }
        }
        private void MagicValueTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (!allowed_keys.Contains(e.Key)) {
                // If this key is not in our whitelist
                if (!(
                    (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                    (Keyboard.IsKeyDown(Key.C) || Keyboard.IsKeyDown(Key.V))
                    )) {
                    // not "Ctrl + C" and not "Ctrl + V"
                    e.Handled = true;
                }
            }
        }
        private void RandomizeSeedTextbox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (!allowed_keys.Contains(e.Key)) {
                // If this key is not in our whitelist
                if (!(
                    (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                    (Keyboard.IsKeyDown(Key.C) || Keyboard.IsKeyDown(Key.V))
                    )) {
                    // not "Ctrl + C" and not "Ctrl + V"
                    e.Handled = true;
                }
            }
        }
        private void EraseCountTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (!allowed_keys.Contains(e.Key)) {
                // If this key is not in our whitelist
                if (!(
                    (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                    (Keyboard.IsKeyDown(Key.C) || Keyboard.IsKeyDown(Key.V))
                    )) {
                    // not "Ctrl + C" and not "Ctrl + V"
                    e.Handled = true;
                }
            }
        }
        private void EraseStartTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (!allowed_keys.Contains(e.Key)) {
                // If this key is not in our whitelist
                if (!(
                    (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                    (Keyboard.IsKeyDown(Key.C) || Keyboard.IsKeyDown(Key.V))
                    )) {
                    // not "Ctrl + C" and not "Ctrl + V"
                    e.Handled = true;
                }
            }
        }
        private void EntryIndexTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (!allowed_keys.Contains(e.Key)) {
                // If this key is not in our whitelist
                if (!(
                    (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                    (Keyboard.IsKeyDown(Key.C) || Keyboard.IsKeyDown(Key.V))
                    )) {
                    // not "Ctrl + C" and not "Ctrl + V"
                    e.Handled = true;
                }
            }
        }
    }
}
