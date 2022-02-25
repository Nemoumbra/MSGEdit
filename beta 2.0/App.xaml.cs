using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace MSGEdit
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            switch (e.Args.Length) {
                case 0: {
                    // Normal startup
                    MainWindow wnd = new MainWindow();
                    wnd.Show();
                    break;
                }
                case 1: {
                    // Expect "filename.txt or filename.pac or filename.msg"
                    //Console.WriteLine("This is where a new feature will be, for now it's just a placeholder\n");
                    MessageBox.Show("This is where a new feature will be, for now it's just a placeholder");
                    Application.Current.Shutdown();
                    break;
                }
                case 2: {
                    // "-gui filename.txt or filename.pac or filename.msg"
                    //Console.WriteLine("This is where another new feature will be, for now it's just a placeholder\n");
                    MessageBox.Show("This is where another new feature will be, for now it's just a placeholder");
                    Application.Current.Shutdown();
                    break;
                }
                default: {
                    // Usage from CMD: [-gui] filename.txt or .pac or .msg
                    //Console.WriteLine("Usage from CMD: [-gui] filename.extension, where extension is either *.txt, *.msg or *.pac\n");
                    MessageBox.Show("This is where yet another feature will be, for now it's just a placeholder");
                    Application.Current.Shutdown();
                    break;
                }
            }
        }
    }
}
