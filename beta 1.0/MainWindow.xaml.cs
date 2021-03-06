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

namespace MSGEdit
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool is_editing_in_progress = false;
        public bool magic_changed = false;
        public Key[] allowed_keys;
        public string readme_message = "The directory where MSGEdit.exe is located does not contain a file \"Readme.txt\".\nWould you like to restore it?";
        public string readme_content = "MSGEdit beta 1.0, by Nemoumbra © 2022";
        public string allowed_chars = @"[0-9]";
        public MSGtoTXT core;

        void make_readme() {
            File.WriteAllText("readme.txt", readme_content, Encoding.Unicode);
        }
        void check_readme() {
            try {
                if (!File.Exists("readme.txt") && MessageBox.Show(readme_message, "Suggestion", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                    make_readme();
                }
            }
            catch (Exception except) {
                MessageBox.Show("Readme file creating error: ", except.Message);
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            core = new MSGtoTXT();
            allowed_keys = new Key[10] { Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9, Key.D0, Key.NumPad0, Key.NumPad1,
            Key.NumPad2, Key.NumPad3, Key.NumPad4, Key.NumPad5, Key.NumPad6, Key.NumPad7, Key.NumPad8, Key.NumPad9, Key.Left, Key.Right, Key.Delete};
            DataObject.AddPastingHandler(EntryIndexTextBox, OnPaste);
            DataObject.AddPastingHandler(MagicValueTextBox, OnPaste);
            check_readme();
        }

        public void hide(UIElement elem) {
            elem.Visibility = Visibility.Hidden;
        }
        public void show(UIElement elem)
        {
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
        public void hide_title_screen(/*bool hide_go_back = false*/)
        {
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
        public void hide_workshop()
        {
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
            switch (format)
            {
                case PataponMessageFormat.TXT:
                    {
                        core.SaveTXT(filename);
                        break;
                    }
                case PataponMessageFormat.MSG:
                    {
                        core.SaveMSG(filename);
                        break;
                    }
            }
            if (core.LastError != "")
            {
                MessageBox.Show("File saving failed: " + core.LastError, "Error");
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
                MessageBox.Show("File loading failed: " + core.LastError, "Error");
            }
            else {
                hide_title_screen();
                show_workshop();
                is_editing_in_progress = true;
                FileContentListBox.Items.Clear();
                string[] entries = core.Content();
                foreach (string entry in entries) {
                    FileContentListBox.Items.Add(entry.Trim('\0'));
                }
                MagicValueTextBox.Text = core.Magic.ToString();
                EntriesCountLabel.Content = core.Count.ToString() +  " entries";
            }
        }
        //public void accept_given_file(string filename) {
        //    // понять, что это за файл
        //}

        private void LoadTXTButton_Click(object sender, RoutedEventArgs e)
        {
            // Доступно только в момент, когда нет текущего editing.
            OpenFileDialog choose_file_menu = new OpenFileDialog();
            choose_file_menu.Filter = "Patapon message files (Owocek format) (*.txt)|*.txt";
            
            //choose_file_menu.InitialDirectory = Environment.CurrentDirectory;
            if (choose_file_menu.ShowDialog() == true)
            {
               // accept_given_file(choose_file_menu.FileName);
                load_file(choose_file_menu.FileName, PataponMessageFormat.TXT);
            }

        }

        private void LoadMSGButton_Click(object sender, RoutedEventArgs e)
        {
            // Доступно только в момент, когда нет текущего editing.
            OpenFileDialog choose_file_menu = new OpenFileDialog();
            choose_file_menu.Filter = "Patapon message files (*.msg)|*.msg";
            //choose_file_menu.InitialDirectory = Environment.CurrentDirectory;
            if (choose_file_menu.ShowDialog() == true)
            {
               // accept_given_file(choose_file_menu.FileName);
                load_file(choose_file_menu.FileName, PataponMessageFormat.MSG);
            }

        }

        private void SaveTXTButton_Click(object sender, RoutedEventArgs e)
        {
            // Доступно только в момент editing.
            SaveFileDialog choose_file_menu = new SaveFileDialog();
            choose_file_menu.Filter = "Patapon message files (Owocek format) (*.txt)|*.txt";
            //choose_file_menu.AddExtension = true;
            if (choose_file_menu.ShowDialog() == true) 
            {
              //  save_file(choose_file_menu.FileName);
                save_file(choose_file_menu.FileName, PataponMessageFormat.TXT);
            }
        }

        private void SaveMSGButton_Click(object sender, RoutedEventArgs e)
        {
            // Доступно только в момент editing.
            SaveFileDialog choose_file_menu = new SaveFileDialog();
            choose_file_menu.Filter = "Patapon message files (*.msg)|*.msg";
            //choose_file_menu.AddExtension = true;
            if (choose_file_menu.ShowDialog() == true) 
            {
               // save_file(choose_file_menu.FileName);
                save_file(choose_file_menu.FileName, PataponMessageFormat.MSG);
            }
        }

        private void StopEditingButton_Click(object sender, RoutedEventArgs e)
        {
            // Доступно только в момент editing.
            hide_workshop();
            show_title_screen(/*true*/);
        }

        private void ResumeEditingButton_Click(object sender, RoutedEventArgs e)
        {
            // Доступно только в момент, когда нет текущего editing.
            hide_title_screen();
            show_workshop();
        }

        private void EraseButton_Click(object sender, RoutedEventArgs e)
        {
            // Доступно только в момент editing.
            MessageBoxResult res = MessageBox.Show("Warning! This action cannot be undone, are you sure this is what you want?", "Warning", MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes) {
                int start_index = 0;
                try
                {
                    start_index = Convert.ToInt32(EraseStartTextBox.Text);
                }
                catch (Exception except) {
                    MessageBox.Show("Start index format error: " + except.Message, "Error");
                    return;
                }
                int count = Convert.ToInt32(EraseCountTextBox.Text);
                core.Erase(start_index, count);
                if (core.LastError != "") {
                    MessageBox.Show("Entries erasement failed: " + core.LastError, "Error!");
                }
                else {
                    for (int i = 0; i < count; i++) {
                        FileContentListBox.Items.RemoveAt(start_index);
                    }
                    EntriesCountLabel.Content = core.Count.ToString() + " entries";
                }
            }
        }

        private void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            // Доступно только в момент editing.
            int index = 0;
            try {
                index = Convert.ToInt32(InsertIndexTextBox.Text);
            }
            catch (Exception except)
            {
                MessageBox.Show("Index format error: " + except.Message, "Error");
                return;
            }
            core.Insert(index, InsertStringTextBox.Text);
            if (core.LastError != "")
            {
                MessageBox.Show("Insertion failed: " + core.LastError, "Error!");
            }
            else {
                FileContentListBox.Items.Insert(index, InsertStringTextBox.Text);
                EntriesCountLabel.Content = core.Count.ToString() + " entries";
            }
        }

        private void SaveEntryButton_Click(object sender, RoutedEventArgs e)
        {
            // Доступно только в момент editing.
            int index = 0;
            try {
                index = Convert.ToInt32(EntryIndexTextBox.Text);
            }
            catch (Exception except)
            {
                MessageBox.Show("Index format error: " + except.Message, "Error");
                return;
            }
            core[index] = EntryStringTextBox.Text;
            if (core.LastError != "") {
                MessageBox.Show("Assignment failed: " + core.LastError, "Error!");
            }
            else {
                while (index >= FileContentListBox.Items.Count) {
                    FileContentListBox.Items.Add("");
                }
                FileContentListBox.Items.RemoveAt(index);
                FileContentListBox.Items.Insert(index, EntryStringTextBox.Text);
                //FileContentListBox.Items.ge
            }
        }

        private void EntryIndexTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Доступно только в момент editing.
            if (EntryIndexTextBox.Text != "")
            {
                int index = Convert.ToInt32(EntryIndexTextBox.Text);
                if (index < core.Count) {
                    //string test = core[index];
                    EntryStringTextBox.Text = core[index];
                    FileContentListBox.SelectedIndex = index;
                    FileContentListBox.ScrollIntoView(FileContentListBox.Items.GetItemAt(index));
                    //FileContentListBox.Items.ge
                }
            }
        }

        private void DragAndDropRectangle_Drop(object sender, DragEventArgs e)
        {
            string filename = "";
            try
            {
                if (e.Data.GetFormats().Contains(DataFormats.FileDrop))
                {
                    filename = ((e.Data.GetData(DataFormats.FileDrop)) as string[])[0];
                }
            }
            catch (Exception exept)
            {
                MessageBox.Show("Error: " + exept.Message);
            }
            if (!is_editing_in_progress || MessageBox.Show("Warning! This action will unload the file you are currently editing from the memory. Proceed?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                if (System.IO.Path.GetExtension(filename) != ".msg") {
                    load_file(filename, PataponMessageFormat.TXT);
                }
                else {
                    load_file(filename, PataponMessageFormat.MSG);
                }
            }
        }

        private void MagicValueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (magic_changed && MagicValueTextBox.Text != "") {
                core.Magic = Convert.ToInt32(MagicValueTextBox.Text);
                magic_changed = false;
            }
        }

        private void MagicValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            magic_changed = true;
            //MessageBox.Show(e.OriginalSource.ToString());
        }

        private void EntryIndexTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!allowed_keys.Contains(e.Key))
            {
                e.Handled = true;
            }
        }

        private void EraseStartTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!allowed_keys.Contains(e.Key))
            {
                e.Handled = true;
            }
        }

        private void EraseCountTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!allowed_keys.Contains(e.Key))
            {
                e.Handled = true;
            }
        }

        private void EraseCountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //int start_index = Convert.ToInt32(EraseStartLabel.Content);
            //int count = Convert.ToInt32(EraseCountLabel.Content);
            //if (start_index >= 0 && count < Convert.ToInt32(EntriesCountLabel.Content) - start_index) {
            //    
            //}
        }

        private void EraseStartTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void NewFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (!is_editing_in_progress || MessageBox.Show("Warning! This action will unload the file you are currently editing from the memory. Proceed?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                core.MakeNewFile();
                hide_title_screen();
                show_workshop();
                is_editing_in_progress = true;
                FileContentListBox.Items.Clear();
                string[] entries = core.Content();
                foreach (string entry in entries)
                {
                    FileContentListBox.Items.Add(entry.Trim('\0'));
                }
                MagicValueTextBox.Text = core.Magic.ToString();
                EntriesCountLabel.Content = core.Count.ToString() + " entries";
                EntryStringTextBox.Text = "";
            }
        }

        private void RandomizeSeedTextbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!allowed_keys.Contains(e.Key))
            {
                e.Handled = true;
            }
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e) {
            var text = e.SourceDataObject.GetData(DataFormats.UnicodeText);
            if (!(text as string).All(char.IsDigit)) {
                e.CancelCommand();
            }
        }

        private void RandomizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Warning! This action cannot be undone! Proceed?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                try {
                    int seed = Convert.ToInt32(RandomizeSeedTextbox.Text);
                    core.Randomize(seed);
                    FileContentListBox.Items.Clear();
                    string[] entries = core.Content();
                    foreach (string entry in entries)
                    {
                        FileContentListBox.Items.Add(entry.Trim('\0'));
                    }
                }
                catch (Exception except) {
                    MessageBox.Show("Randomization failed! Error: " + except.Message);
                }
            }
        }

        private void MagicValueTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!allowed_keys.Contains(e.Key))
            {
                e.Handled = true;
            }
        }

        private void MagicValueTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //MessageBox.Show("Test");
        }
    }
}
