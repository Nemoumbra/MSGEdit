using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Media;

namespace MSGEdit {
    /// <summary>
    /// Логика взаимодействия для SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window {
        public int last_found_index = -1; // we use this value as a start index <=> it != -1
        public int first_found_index; // when we reach this index, we stop searching
        public Key[] allowed_keys; // assigned to from the MainWindow

        public SearchWindow() {
            InitializeComponent();
            // maybe initialize its owner somewhere?
            DataObject.AddPastingHandler(start_index_textbox, OnPaste);
        }

        private void Search_window_Closed(object sender, EventArgs e) {
            MainWindow owner = this.Owner as MainWindow;
            owner.is_search_window_shown = false;
            owner.Activate();
        }

        private void Search_window_Loaded(object sender, RoutedEventArgs e) {
            MainWindow owner = this.Owner as MainWindow;
            if (owner.settings.autopaste_from_clipboard) {
                string prompt;
                if (Clipboard.ContainsText() && !(prompt = Clipboard.GetText()).Contains('\n')) {
                    // If the clipboard contains text and this text does not contain '\n', we use it as a prompt
                    search_textbox.Text = prompt;
                    search_textbox.Focus();
                    search_textbox.SelectAll();
                }
                else {
                    if (owner.EntryStringTextBox.SelectionLength != 0) {
                        search_textbox.Text = owner.EntryStringTextBox.SelectedText;
                        search_textbox.Focus();
                        search_textbox.SelectAll();
                    }
                }
            }
            else {
                if (owner.EntryStringTextBox.SelectionLength != 0) {
                    search_textbox.Text = owner.EntryStringTextBox.SelectedText;
                    search_textbox.Focus();
                    search_textbox.SelectAll();
                }
            }
        }

        private void Find_next_button_Click(object sender, RoutedEventArgs e) {
            if (search_textbox.Text == "") {
                MessageBox.Show("The search field is empty.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            MainWindow owner = this.Owner as MainWindow;
            Tuple<int, int> res;

            // evaluate the first needed index
            int start = 0;

            if (last_found_index != -1) { // the search operation is "find next"
                start = last_found_index;
                // now we need to mofidy it in accordance with the search parameters and current core state
                if (search_backwards_checkbox.IsChecked == true) {
                    if (start == 0) {
                        if (cycle_search_checkbox.IsChecked == true) {
                            start = owner.core.Count - 1;
                        }
                        else {
                            // we are done and cannot proceed
                            last_found_index = -1;
                            MessageBox.Show("Nothing else found.", "Information", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                    }
                    else {
                        --start;
                    }
                }
                else {
                    if (start == owner.core.Count - 1) {
                        if (cycle_search_checkbox.IsChecked == true) {
                            start = 0;
                        }
                        else {
                            // we are done and cannot proceed
                            last_found_index = -1;
                            MessageBox.Show("Nothing else found.", "Information",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                    }
                    else {
                        ++start;
                    }
                }

                // now it may turn out that the start is not a correct index
                if (start > owner.core.Count) {
                    if (MessageBox.Show("The data has been modified in between calls to the find operation. The next attempt will start over.",
                        "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel) {
                    }
                    last_found_index = -1;
                    return;
                }
            }
            else { // the search operation is "find first"
                // start over
                if (search_starts_from_selected_index_checkbox.IsChecked == true) {
                    if (owner.FileContentListBox.SelectedIndex != -1) {
                        // we start the search from selected index
                        start = owner.FileContentListBox.SelectedIndex;
                    }
                    else {
                        start = 0; // exceptional case.
                    }
                }
                else {
                    // else we don't...
                    try {
                        start = Convert.ToInt32(start_index_textbox.Text);
                    }
                    catch (Exception except) {
                        MessageBox.Show($"Start index format error: {except.Message}", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }
            
            // debug
            //MessageBox.Show($"Debug: starting from {start}");
            
            if (regex_checkbox.IsChecked == true) {
                res = owner.core.FindRegex(search_textbox.Text, start, !search_backwards_checkbox.IsChecked.Value, 
                    cycle_search_checkbox.IsChecked.Value);
            }
            else {
                res = owner.core.Find(search_textbox.Text, start, case_sensitive_checkbox.IsChecked.Value,
                    whole_words_checkbox.IsChecked.Value, !search_backwards_checkbox.IsChecked.Value,
                    cycle_search_checkbox.IsChecked.Value);
            }
            if (owner.core.LastError != "") {
                MessageBox.Show($"Search error: {owner.core.LastError}", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (res == null) {
                // Not found
                MessageBox.Show($"...but nothing found.", "Success...", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else {
                // Found
                if (last_found_index == -1) { // find first
                    first_found_index = res.Item1;
                }
                else { // find next
                    if (first_found_index == res.Item1) {
                        MessageBox.Show("The search has reached its starting point", "Information",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        last_found_index = -1;
                        owner.EntryIndexTextBox.Text = res.Item1.ToString();
                        return;
                    }
                }
                last_found_index = res.Item1;
                owner.EntryIndexTextBox.Text = last_found_index.ToString();
            }
        }

        private void Find_all_button_Click(object sender, RoutedEventArgs e) {
            if (search_textbox.Text == "") {
                MessageBox.Show("The search field is empty.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            last_found_index = -1;
            MainWindow owner = this.Owner as MainWindow;
            
            List<Tuple<int, int>> res;
            if (regex_checkbox.IsChecked == true) {
                res = owner.core.FindRegexAll(search_textbox.Text);
            }
            else {
                res = owner.core.FindAll(
                    search_textbox.Text, 
                    case_sensitive_checkbox.IsChecked.Value,
                    whole_words_checkbox.IsChecked.Value
                    );
            }
            if (owner.core.LastError != "") {
                MessageBox.Show($"Search error: {owner.core.LastError}", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            search_results_combobox.Items.Clear();
            if (res.Count != 0) {
                for (int i = 0; i < res.Count; ++i) {
                    search_results_combobox.Items.Add($"{res[i].Item1}"); // in this case the position ends up being unused.
                }
                if (res.Count == 1) {
                    MessageBox.Show($"Found 1 message.", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else {
                    MessageBox.Show($"Found {res.Count} messages.", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else {
                MessageBox.Show($"...but nothing found.", "Success...", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Help_button_Click(object sender, RoutedEventArgs e) {
            MainWindow owner = this.Owner as MainWindow;
            owner.show_about_window();
        }

        private void Cancel_button_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Search_start_checkbox_Checked(object sender, RoutedEventArgs e) {
            if (start_index_textbox == null)
                return;
            if (start_index_textblock == null)
                return;

            start_index_textbox.IsEnabled = false;
            start_index_textblock.IsEnabled = false;
        }

        private void Search_start_checkbox_Unchecked(object sender, RoutedEventArgs e) {
            start_index_textbox.IsEnabled = true;
            start_index_textblock.IsEnabled = true;
        }

        private void Regex_checkbox_Checked(object sender, RoutedEventArgs e) {
            whole_words_checkbox.IsEnabled = false;
            case_sensitive_checkbox.IsEnabled = false;
        }

        private void Regex_checkbox_Unchecked(object sender, RoutedEventArgs e) {
            whole_words_checkbox.IsEnabled = true;
            case_sensitive_checkbox.IsEnabled = true;
        }

        private void Search_results_combobox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (search_results_combobox.SelectedIndex == -1) {
                return;
            }
            MainWindow owner = this.Owner as MainWindow;
            owner.EntryIndexTextBox.Text = search_results_combobox.SelectedItem as string;
        }

        private void Search_textbox_TextChanged(object sender, TextChangedEventArgs e) {
            last_found_index = -1; // the search must be restarted
        }

        private void Start_index_textbox_TextChanged(object sender, TextChangedEventArgs e) {
            last_found_index = -1; // the search must be restarted
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e) {
            var text = e.SourceDataObject.GetData(DataFormats.UnicodeText);
            if (!(text as string).All(char.IsDigit)) {
                e.CancelCommand();
            }
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

        private void Search_window_PreviewKeyDown(object sender, KeyEventArgs e) {
            //switch (e.Key) {
            //    case Key.F5: {
            //        // Regular search
            //        find_next_button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            //        break;
            //    }
            //    case Key.F7: {
            //        // Force restart
            //        last_found_index = -1;
            //        find_next_button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            //        break;
            //    }
            //}
            if (e.Key == Key.F5) {
                // Regular search
                find_next_button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            if (e.Key == Key.F1) {
                MainWindow owner = this.Owner as MainWindow;
                owner.show_about_window();
            }
        }
    }
}
