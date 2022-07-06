﻿using System;
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
        public string current_filename = "";
        public Key[] allowed_keys;
        public string readme_message = "The directory where MSGEdit.exe is located does not contain a file \"Readme.txt\".\nWould you like to restore it?";
        public string unload_file_warning = "Warning! This action will unload the file you are currently editing from the memory. Proceed?";
        public string irreversible_action_warning = "Warning! This action cannot be undone, are you sure this is what you want?";
        public string allowed_chars = @"[0-9]";
        public MSGtoTXT core;
        //public ObservableCollection<string> Items {get; set;}
        //public ObservableCollection<PataponMessage> Items { get; set; }
        public ObservableCollection<PataponMessage> Items;
        public void make_readme() {
            List<string> lines = new List<string>();
            lines.Add("MSGEdit beta 3.0, by Nemoumbra © 2022");
            lines.Add("");
            lines.Add("This tool was designed to be as user-friendly as possible. Because of that, all input fields have labels next to them explaning their meaning.");
            lines.Add("There is no known way to crash this application, but the author would love to hear about any found bugs in Patapon Modding Discord server.");
            lines.Add("");

            lines.Add("USAGE:");
            lines.Add("The tool supports three ways of working with files: making a new one from scratch, loading some already existing file to edit it (which is intended to be the most popular way of using MSGEdit) or converting files from one format to another from command line.");
            lines.Add("Please note that MSGEdit does not support undoing edits and, unless you explicitly forbid it, sometimes you will be asked to corfirm your action.");
            lines.Add("");

            lines.Add("The settings file can be used to adjust MSGEdit to one's preferences. It must contain exactly 3 lines in the predefined order (please refer to the file provided with the tool or the automatically generated one).");
            lines.Add("");
            lines.Add("If enable warnings is set to false, no confirmation will ever be asked.");
            lines.Add("If check readme is set to false, MSGEdit won't offer to restore the readme if it is not present in tool's directory.");
            lines.Add("If deduce file type is set to true, the tool will attemp to figure out the file type (MSG or TXT) based on its content. This setting also changes the way command line arguments are processed.");
            lines.Add("");

            lines.Add("You may find it easier to get used to the tool if you start by creating a new file and try to play with as many buttons as possible.");
            lines.Add("");

            lines.Add("Please note that:");
            lines.Add(" - entering the message index in the field will highlight it in the messages list and load it into the textbox, so please make sure to apply your changes before moving to the next entry you want to edit");
            lines.Add(" - selecting an existing entry in the list WILL enter the message index in the field and load it into the textbox as well, so exercise caution");
            lines.Add(" - saving the entry with an index which does not correspond to any message in the file will result in the tool creating a new entry and filling all the entries between with blank lines");
            lines.Add("");

            lines.Add("The unique way of organizing the data inside a TXT format was created by Owocek in his MSGTools conversion tool.");
            lines.Add("");

            lines.Add("Please bear in mind that, unlike Patapon 3, the Patapon 1 and Patapon 2 message files have a *.pac extension (not to be confused with Patapon Assembly Code).");

            lines.Add("");
            lines.Add("Command line arguments support.");
            lines.Add("Before showing the window, MSGEdit  can now attempt to perform some  actions according to command line arguments.");
            lines.Add("As of July 6, 2022, MSGEdit supports 4 such actions:");
            lines.Add("");
            lines.Add("1) No command line arguments.");
            lines.Add("No actions.");
            lines.Add("");
            lines.Add("2) A single file path.");
            lines.Add("The tool attempts to load the file on startup (if settings state that the file type must not be deduced, a question will be asked).");
            lines.Add("");
            lines.Add("3) A single file path, followed by either -txt or -msg.");
            lines.Add("Same as 2), but the setting is ignored as the tool will use the format specified as the second argument.");
            lines.Add("");
            lines.Add("4) -convert, followed by a list of file paths (if settings state that the file type must not be deduced, it is necessary to specify the type after every file).");
            lines.Add("The tool attempts to convert the files from MSG to TXT and vice-versa. The original files are preserved, the new ones are formed by appending the corresponding extension to original file names.");

            //File.WriteAllLines("readme.txt", lines, Encoding.Unicode);
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
                MessageBox.Show("Readme file creating error: " + except.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public MainWindow() {
            InitializeComponent();
            //core = new MSGtoTXT(); // remove!
            allowed_keys = new Key[25] { Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9, Key.D0, Key.NumPad0, Key.NumPad1,
            Key.NumPad2, Key.NumPad3, Key.NumPad4, Key.NumPad5, Key.NumPad6, Key.NumPad7, Key.NumPad8, Key.NumPad9, Key.Left, Key.Right, Key.Delete, Key.Back, Key.Tab};
            DataObject.AddPastingHandler(EntryIndexTextBox, OnPaste);
            DataObject.AddPastingHandler(MagicValueTextBox, OnPaste);
            Items = new ObservableCollection<PataponMessage>();
            FileContentListBox.ItemsSource = Items;
            //CollectionViewSource.GetDefaultView(Items).Refresh();
            //DataContext = Items;            
        }

        public void hide(UIElement elem) {
            elem.Visibility = Visibility.Hidden;
        }
        public void show(UIElement elem) {
            elem.Visibility = Visibility.Visible;
        }
        //public void select_range(int start_index, int count) {

        //}

        public void show_title_screen(/*bool show_go_back = false*/) {
            show(LoadTXTButton);
            show(LoadMSGButton);
            show(DragAndDropRectangle);
            //if (show_go_back) {
            //    show(ResumeEditingButton);
            //}
            show(ResumeEditingButton);
            show(NewFileButton);
            //show(AuthorLabel);
        }
        public void hide_title_screen(/*bool hide_go_back = false*/) {
            hide(LoadTXTButton);
            hide(LoadMSGButton);
            hide(DragAndDropRectangle);
            //if (hide_go_back)
            //{
            //    hide(ResumeEditingButton);
            //}
            hide(ResumeEditingButton);
            hide(NewFileButton);
            //hide(AuthorLabel);
        }
        public void show_workshop() {
            show(FileContentListBox);
            show(EntriesCountLabel);
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
            hide(EntriesCountLabel);
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
        /*public void add_item_to_listbox(string item) {
            FileContentListBox.Items.Add(item);
        }*/

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
                MessageBox.Show("File saving failed: " + core.LastError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void load_file(string filename) {
            core.LoadAny(filename);
            if (!core.isLoaded) {
                MessageBox.Show("File loading failed: " + core.LastError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else {
                hide_title_screen();
                show_workshop();
                is_editing_in_progress = true;
                //FileContentListBox.Items.Clear();
                Items.Clear();
                string[] entries = core.Content();
                int i = 0;
                foreach (string entry in entries) {
                    //TextBlock item = new TextBlock();
                    //item.Inlines.Add(new Run(i.ToString() + " ") { FontWeight = FontWeights.Bold});
                    //item.Inlines.Add(entry.Trim('\0'));
                    //FileContentListBox.Items.Add(i.ToString() + " " + entry.Trim('\0'));
                    PataponMessage message = new PataponMessage(entry.Trim('\0'));
                    message.Index = i;
                    //Items.Add(entry.Trim('\0'));
                    Items.Add(message);
                    i++;
                }
                MagicValueTextBox.Text = core.Magic.ToString();
                EntriesCountLabel.Content = core.Count.ToString() + " entries";
                this.Title = "MSGEdit - [" + filename + "]";
                current_filename = filename;
            }
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
                MessageBox.Show("File loading failed: " + core.LastError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else {
                hide_title_screen();
                show_workshop();
                is_editing_in_progress = true;
                //FileContentListBox.Items.Clear();
                Items.Clear();
                string[] entries = core.Content();
                int i = 0;
                foreach (string entry in entries) {
                    //TextBlock item = new TextBlock();
                    //item.Inlines.Add(new Run(i.ToString() + " ") { FontWeight = FontWeights.Bold});
                    //item.Inlines.Add(entry.Trim('\0'));
                    //FileContentListBox.Items.Add(i.ToString() + " " + entry.Trim('\0'));
                    PataponMessage message = new PataponMessage(entry.Trim('\0'));
                    message.Index = i;
                    //Items.Add(entry.Trim('\0'));
                    Items.Add(message);
                    i++;
                }
                MagicValueTextBox.Text = core.Magic.ToString();
                EntriesCountLabel.Content = core.Count.ToString() + " entries";
                this.Title = "MSGEdit - [" + filename + "]";
                current_filename = filename;
            }
        }
        //public void accept_given_file(string filename) {
        //    // понять, что это за файл
        //}

        private void LoadTXTButton_Click(object sender, RoutedEventArgs e) {
            // Доступно только в момент, когда нет текущего editing.
            OpenFileDialog choose_file_menu = new OpenFileDialog();
            choose_file_menu.Filter = "Patapon message files (*.txt)|*.txt";

            //choose_file_menu.InitialDirectory = Environment.CurrentDirectory;
            if (choose_file_menu.ShowDialog() == true) {
                // accept_given_file(choose_file_menu.FileName);
                load_file(choose_file_menu.FileName, PataponMessageFormat.TXT);
            }

        }

        private void LoadMSGButton_Click(object sender, RoutedEventArgs e) {
            // Доступно только в момент, когда нет текущего editing.
            OpenFileDialog choose_file_menu = new OpenFileDialog();
            choose_file_menu.Filter = "Patapon message files (*.msg, *.pac)|*.msg;*.pac";
            choose_file_menu.DefaultExt = ".msg";
            //choose_file_menu.InitialDirectory = Environment.CurrentDirectory;
            if (choose_file_menu.ShowDialog() == true) {
                // accept_given_file(choose_file_menu.FileName);
                load_file(choose_file_menu.FileName, PataponMessageFormat.MSG);
            }

        }

        private void SaveTXTButton_Click(object sender, RoutedEventArgs e) {
            // Доступно только в момент editing.
            SaveFileDialog choose_file_menu = new SaveFileDialog();
            choose_file_menu.Filter = "Patapon message files (*.txt)|*.txt";
            //choose_file_menu.DefaultExt = ".txt";
            //choose_file_menu.AddExtension = true;
            if (current_filename != "") {
                string filename = current_filename.Substring(current_filename.LastIndexOf('\\') + 1); // path contains >= 1 backslash
                int dot_position = filename.LastIndexOf('.');
                if (dot_position == -1) {
                    // no extension
                    filename += ".txt";
                }
                else {
                    if (filename.Substring(dot_position + 1) != "txt") {
                        filename += ".txt";
                    }
                }

                choose_file_menu.FileName = filename;
            }
            
            if (choose_file_menu.ShowDialog() == true) {
                //  save_file(choose_file_menu.FileName);
                save_file(choose_file_menu.FileName, PataponMessageFormat.TXT);
            }
        }

        private void SaveMSGButton_Click(object sender, RoutedEventArgs e) {
            // Доступно только в момент editing.
            SaveFileDialog choose_file_menu = new SaveFileDialog();
            choose_file_menu.Filter = "Patapon message files (*.msg, *.pac)|*.msg;*.pac";
            //choose_file_menu.DefaultExt = ".msg";
            //choose_file_menu.AddExtension = true;
            if (current_filename != "") {
                string filename = current_filename.Substring(current_filename.LastIndexOf('\\') + 1); // path contains >= 1 backslash
                int dot_position = filename.LastIndexOf('.');
                if (dot_position == -1) {
                    // no extension
                    filename += ".msg";
                }
                else {
                    if (filename.Substring(dot_position + 1) != "msg") {
                        filename += ".msg";
                    }
                }

                choose_file_menu.FileName = filename;
            }
            
            if (choose_file_menu.ShowDialog() == true) {
                // save_file(choose_file_menu.FileName);
                save_file(choose_file_menu.FileName, PataponMessageFormat.MSG);
            }
        }

        private void StopEditingButton_Click(object sender, RoutedEventArgs e) {
            // Доступно только в момент editing.
            hide_workshop();
            show_title_screen(/*true*/);
        }

        private void ResumeEditingButton_Click(object sender, RoutedEventArgs e) {
            // Доступно только в момент, когда нет текущего editing.
            hide_title_screen();
            show_workshop();
        }

        private void EraseButton_Click(object sender, RoutedEventArgs e) {
            // Доступно только в момент editing.
            if (!settings.enable_warnings || MessageBox.Show(irreversible_action_warning, "Warning",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) {
                int start_index = 0;
                try {
                    start_index = Convert.ToInt32(EraseStartTextBox.Text);
                }
                catch (Exception except) {
                    MessageBox.Show("Start index format error: " + except.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                int count = Convert.ToInt32(EraseCountTextBox.Text);
                core.Erase(start_index, count);
                if (core.LastError != "") {
                    MessageBox.Show("Entries erasement failed: " + core.LastError, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else {
                    for (int i = 0; i < count; i++) {
                        //FileContentListBox.Items.RemoveAt(start_index);
                        Items.RemoveAt(start_index);

                    }
                    for (int i = start_index; i < Items.Count; i++) {
                        Items[i].Index = Items[i].Index - count;
                    }

                    //CollectionViewSource.GetDefaultView(Items).Refresh();
                    EntriesCountLabel.Content = core.Count.ToString() + " entries";
                }
            }
        }

        private void InsertButton_Click(object sender, RoutedEventArgs e) {
            // Доступно только в момент editing.
            int index = 0;
            try {
                index = Convert.ToInt32(InsertIndexTextBox.Text);
            }
            catch (Exception except) {
                MessageBox.Show("Index format error: " + except.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            core.Insert(index, InsertStringTextBox.Text);
            if (core.LastError != "") {
                MessageBox.Show("Insertion failed: " + core.LastError, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else {
                PataponMessage message = new PataponMessage(InsertStringTextBox.Text);
                message.Index = index;
                //FileContentListBox.Items.Insert(index, message);

                for (int i = index/* + 1*/; i < Items.Count; i++) {
                    Items[i].Index = Items[i].Index + 1;
                }

                Items.Insert(index, message);
                //for (int i = 0; i < Items.Count; i++) {
                //    Items[i].Index = i;
                //}
                EntriesCountLabel.Content = core.Count.ToString() + " entries";
            }
        }

        private void SaveEntryButton_Click(object sender, RoutedEventArgs e) {
            // Доступно только в момент editing.
            int index = 0;
            try {
                index = Convert.ToInt32(EntryIndexTextBox.Text);
            }
            catch (Exception except) {
                MessageBox.Show("Index format error: " + except.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            core[index] = EntryStringTextBox.Text;
            if (core.LastError != "") {
                MessageBox.Show("Assignment failed: " + core.LastError, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else {
                while (index >= Items.Count) {
                    PataponMessage message = new PataponMessage("");
                    message.Index = Items.Count;
                    //FileContentListBox.Items.Add("");
                    Items.Add(message);
                }
                //FileContentListBox.Items.RemoveAt(index);
                Items[index].Message = EntryStringTextBox.Text;
                //FileContentListBox.Items.Insert(index, EntryStringTextBox.Text);
                //FileContentListBox.Items.ge
            }
        }

        private void EntryIndexTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (selection_lock) {
                selection_lock = false;
                return;
                //that's not user's input
            }
            // Доступно только в момент editing.
            if (EntryIndexTextBox.Text != "") {
                try {
                    int index = Convert.ToInt32(EntryIndexTextBox.Text);
                    if (index < core.Count) {
                        //string test = core[index];
                        EntryStringTextBox.Text = core[index];
                        if (index != FileContentListBox.SelectedIndex) {
                            selection_lock = true;
                            FileContentListBox.SelectedIndex = index;
                            FileContentListBox.ScrollIntoView(FileContentListBox.Items.GetItemAt(index));
                        }
                        //FileContentListBox.Items.ge
                    }
                }
                catch (FormatException except) {

                }
                catch (OverflowException except) {

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
            catch (Exception exept) {
                MessageBox.Show("Drop error: " + exept.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (!is_editing_in_progress || !settings.enable_warnings || MessageBox.Show(unload_file_warning, 
                "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) {
                //string extension = System.IO.Path.GetExtension(filename);
                //if (extension != ".msg" && extension != ".pac") {
                //    load_file(filename, PataponMessageFormat.TXT);
                //}
                //else {
                //    load_file(filename, PataponMessageFormat.MSG);
                //}
                if (!settings.deduce_file_type) { 
                    if (MessageBox.Show("Does this file contain MSG?", "Yes/No", MessageBoxButton.YesNo, 
                        MessageBoxImage.Question) == MessageBoxResult.Yes) {
                        load_file(filename, PataponMessageFormat.MSG);
                    }
                    else {
                        load_file(filename, PataponMessageFormat.TXT);
                    }
                }
                else {
                    load_file(filename);
                }
            }
        }

        private void MagicValueTextBox_LostFocus(object sender, RoutedEventArgs e) {
            if (magic_changed && MagicValueTextBox.Text != "") {
                try {
                    core.Magic = Convert.ToInt32(MagicValueTextBox.Text);
                }
                catch (Exception except) {

                }
                magic_changed = false;
            }
        }

        private void MagicValueTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            magic_changed = true;
            //MessageBox.Show(e.OriginalSource.ToString());
        }

        private void EntryIndexTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (!allowed_keys.Contains(e.Key)) {
                if (!((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && (Keyboard.IsKeyDown(Key.C) || Keyboard.IsKeyDown(Key.V)))) {
                    e.Handled = true;
                }
            }
        }

        private void EraseStartTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (!allowed_keys.Contains(e.Key)) {
                e.Handled = true;
            }
        }

        private void EraseCountTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (!allowed_keys.Contains(e.Key)) {
                e.Handled = true;
            }
        }

        private void EraseCountTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            //int start_index = Convert.ToInt32(EraseStartLabel.Content);
            //int count = Convert.ToInt32(EraseCountLabel.Content);
            //if (start_index >= 0 && count < Convert.ToInt32(EntriesCountLabel.Content) - start_index) {
            //    
            //}
        }

        private void EraseStartTextBox_TextChanged(object sender, TextChangedEventArgs e) {

        }

        private void NewFileButton_Click(object sender, RoutedEventArgs e) {
            if (!is_editing_in_progress || !settings.enable_warnings || MessageBox.Show(unload_file_warning, 
                "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) {
                core.MakeNewFile();
                hide_title_screen();
                show_workshop();
                is_editing_in_progress = true;
                //FileContentListBox.Items.Clear();
                Items.Clear();
                /*string[] entries = core.Content();
                foreach (string entry in entries)
                {
                    FileContentListBox.Items.Add(entry.Trim('\0'));
                }*/
                MagicValueTextBox.Text = core.Magic.ToString();
                EntriesCountLabel.Content = core.Count.ToString() + " entries";
                EntryStringTextBox.Text = "";
                InsertStringTextBox.Text = "String to be inserted";
                this.Title = "MSGEdit [Untitled]";
                current_filename = "";
            }
        }

        private void RandomizeSeedTextbox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (!allowed_keys.Contains(e.Key)) {
                e.Handled = true;
            }
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e) {
            var text = e.SourceDataObject.GetData(DataFormats.UnicodeText);
            if (!(text as string).All(char.IsDigit)) {
                e.CancelCommand();
            }
        }

        private void RandomizeButton_Click(object sender, RoutedEventArgs e) {
            if (!settings.enable_warnings || MessageBox.Show(irreversible_action_warning, "Warning", 
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) {
                try {
                    int seed = Convert.ToInt32(RandomizeSeedTextbox.Text);
                    core.Randomize(seed);
                    //FileContentListBox.Items.Clear();
                    string[] entries = core.Content();
                    int i = 0;
                    foreach (string entry in entries) {
                        Items[i].Message = entry.Trim('\0');
                        i++;
                    }
                }
                catch (Exception except) {
                    MessageBox.Show("Randomization failed! Error: " + except.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MagicValueTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (!allowed_keys.Contains(e.Key)) {
                if (!((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && (Keyboard.IsKeyDown(Key.C) || Keyboard.IsKeyDown(Key.V)))) {
                    e.Handled = true;
                }
            }
        }

        private void MagicValueTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            //MessageBox.Show("Test");
        }

        private void FileContentListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (selection_lock) {
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
            if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))) {
                return;
            }
            if (Keyboard.IsKeyDown(Key.O)) {

            }
            if (Keyboard.IsKeyDown(Key.S)) {

            }
        }
    }
}
