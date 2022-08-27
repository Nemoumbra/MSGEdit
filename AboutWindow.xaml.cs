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
using System.Diagnostics;

namespace MSGEdit {
    /// <summary>
    /// Логика взаимодействия для AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window {
        public AboutWindow() {
            InitializeComponent();
        }

        private void About_window_Closed(object sender, EventArgs e) {
            MainWindow owner = this.Owner as MainWindow;
            owner.is_about_window_shown = false;
            owner.Activate();
        }

        //private void Regex_docs_hyperlink_Click(object sender, RoutedEventArgs e) {
            
        //}

        //private void Regex_docs_hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
        //    Process.Start(e.Uri.AbsoluteUri);
        //}

        //private void Github_hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
        //    Process.Start(e.Uri.AbsoluteUri);
        //}

        private void OpenHyperLinkInBrowser(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
            Process.Start(e.Uri.AbsoluteUri);
        }

        private void Cancel_button_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
