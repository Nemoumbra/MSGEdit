using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace MSGEdit
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application {
        public Settings settings; // when do I have to initialize them?
        public MSGtoTXT core;

        private bool convert(string path, PataponMessageFormat format) {
            if (format == PataponMessageFormat.TXT) {
                core.LoadTXT(path);
            }
            else {
                core.LoadMSG(path);
            }

            if (!core.isLoaded) {
                return false;
            }

            if (format == PataponMessageFormat.TXT) {
                core.SaveMSG(path + ".msg");
            }
            else {
                core.SaveTXT(path + ".txt");
            }

            if (core.LastError != "") {
                return false;
            }
            return true; // can be rewritten as return core.LastError == "";
        }
        private bool convert(string path) {
            PataponMessageFormat format = core.LoadAny(path);
            if (!core.isLoaded) {
                return false;
            }
            // now format is correct
            if (format == PataponMessageFormat.MSG) {
                core.SaveTXT(path + ".txt");
            }
            else {
                core.SaveMSG(path + ".msg");
            }
            if (core.LastError != "") {
                return false;
            }
            return true; // can be rewritten as return core.LastError == "";
        }

        void NormalStartup() {
            MainWindow wnd = new MainWindow();
            if (settings.check_readme) {
                wnd.check_readme();
            }

            wnd.settings = settings;
            wnd.core = core;
            wnd.Show();
        }

        private void read_settings_from_file() {
            try {
                StreamReader stream = File.OpenText("settings.txt");
                string[] data = stream.ReadToEnd().Replace("\r", "").Split('\n');
                stream.Close();
                if (data.Length == 3) { // current number of settings
                    settings.enable_warnings = Convert.ToBoolean(data[0].Split('=')[1]);
                    settings.check_readme = Convert.ToBoolean(data[1].Split('=')[1]);
                    settings.deduce_file_type = Convert.ToBoolean(data[2].Split('=')[1]);
                }
            }
            catch (Exception except) {
                MessageBox.Show("Error! " + except.Message + "\nUsing default settings");
                settings.restore_default_settings();
            }
        }
        private void make_settings_file() {
            try {
                StreamWriter stream = File.CreateText("settings.txt");
                /*"Enable_warnings"=True
                  "Check_readme"=True
                  "Deduce_file_type"=False b*/
                stream.WriteLine("\"Enable_warnings\"=True");
                stream.WriteLine("\"Check_readme\"=True");
                stream.Write("\"Deduce_file_type\"=False");
                stream.Close();
            }
            catch (Exception except) {
                MessageBox.Show("Error! " + except.Message + "\nUnable make settings file");
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e) {
            try {
                Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
            }
            catch (Exception except) {
                MessageBox.Show("Error!" + except.Message + "\nCannot set working directory.\nHalting");
                Application.Current.Shutdown();
            }

            //for (int i = 0; i < e.Args.Length; ++i) {
            //    MessageBox.Show(e.Args[i]);
            //}
            //MessageBox.Show(Directory.GetCurrentDirectory());

            settings = new Settings();
            core = new MSGtoTXT();

            if (File.Exists("settings.txt")) {
                read_settings_from_file();
            }
            else {
                if (MessageBox.Show("Settings file not found, should new one be made?", "Prompt", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                    make_settings_file();
                }
            }

            /*
             * Варианты разрешённых команд:
             * 1) Стандартный запуск (вне зависимости от настроек) - 0 аргументов.
             * 2) 1 аргумент - путь к файлу. Здесь происходит либо автоматическое угадывание, либо MessageBox.
             * 3) 2 аргумента - путь к файлу и -txt или -msg. Здесь игнорируется настройка и происходит запуск согласно типу.
             * 4) Любое нечётное # аргументов, где 1 - -convert, а остальные - пути к файлам 
             * с указанием типа -txt или -msg, если deduce = false, иначе без указаний
             * Правда, возможно -convert file_path, если deduce = true
             */

            switch (e.Args.Length) {
                case 0: {
                    // Normal startup
                    NormalStartup();
                    break;
                }
                case 1: {
                    MainWindow wnd = new MainWindow();

                    wnd.settings = settings;
                    wnd.core = core;

                    if (!settings.deduce_file_type) {

                        if (MessageBox.Show("Does this file contain MSG?", "Yes/No", MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.Yes) {
                            // try to access the file and launch as PataponMessageFormat.MSG at workshop state
                            wnd.load_file(e.Args[0], PataponMessageFormat.MSG);
                        }
                        else {
                            wnd.load_file(e.Args[0], PataponMessageFormat.TXT);
                            // try to access the file and launch as PataponMessageFormat.TXT at workshop state
                        }
                    }
                    else {
                        // try to access the file, figure out its content and then launch at workshop state
                        wnd.load_file(e.Args[0]);
                    }

                    if (settings.check_readme) {
                        wnd.check_readme();
                    }
                    wnd.Show();
                    break;
                }
                case 2: {
                    MainWindow wnd = new MainWindow();

                    wnd.settings = settings;
                    wnd.core = core;
                    if (e.Args[0] == "-convert" && settings.deduce_file_type) {
                        if (!convert(e.Args[1])) {
                            // conversion failure
                            MessageBox.Show("Conversion of file " + e.Args[1] + " failed!\nError: " + core.LastError, "Error!",
                                       MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    if (e.Args[1] == "-txt") {
                        // try to access the file and launch as PataponMessageFormat.TXT at workshop state
                        wnd.load_file(e.Args[0], PataponMessageFormat.TXT);
                    }
                    if (e.Args[1] == "-msg") {
                        // try to access the file and launch as PataponMessageFormat.MSG at workshop state
                        wnd.load_file(e.Args[0], PataponMessageFormat.MSG);
                    }
                    // the branch which was executed has already done everything needed (if it was needed)
                    // if none of them were executed, nothing changes
                    if (settings.check_readme) {
                        wnd.check_readme();
                    }
                    wnd.Show();
                    break;
                }
                default: {
                    if (e.Args[0] != "-convert") {
                        // Shutdown? Or maybe normal startup?
                        NormalStartup();
                    }
                    else {
                        if (settings.deduce_file_type) {
                            // expect any number of file paths
                            for (int i = 1; i < e.Args.Length; ++i) {
                                if (!convert(e.Args[i])) {
                                    // conversion failure
                                    MessageBox.Show("Conversion of file " + e.Args[i] + " failed!\nError: " + core.LastError, "Error!",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            NormalStartup();
                        }
                        else {
                            // expect odd number of args
                            if (e.Args.Length % 2 == 0) {
                                // Shutdown? Or maybe normal startup?
                                NormalStartup();
                                return;
                            }

                            int count = e.Args.Length / 2;
                            List<Tuple<string, PataponMessageFormat>> to_convert = new List<Tuple<string, PataponMessageFormat>>(count);
                            for (int i = 0; i < count; ++i) {
                                if (e.Args[2 * (i + 1)] == "-txt") {
                                    to_convert.Add(new Tuple<string, PataponMessageFormat>(e.Args[2 * i + 1], PataponMessageFormat.TXT));
                                }
                                else {
                                    if (e.Args[2 * (i + 1)] == "-msg") {
                                        to_convert.Add(new Tuple<string, PataponMessageFormat>(e.Args[2 * i + 1], PataponMessageFormat.MSG));
                                    }
                                    else {
                                        // this is unacceptable, stop the operation
                                        MessageBox.Show("Wrong file type specification: " + e.Args[2 * (i + 1)] + 
                                            "\n(Must be either -msg or -txt\nAborting conversion", 
                                            "Error!", MessageBoxButton.OK, MessageBoxImage.Error);

                                        NormalStartup();
                                        return;
                                    }
                                }
                            }
                            // now process every file one-by-one
                            for (int i = 0; i < count; ++i) {
                                if (!convert(to_convert[i].Item1, to_convert[i].Item2)) {
                                    // convertation failure, do something
                                    MessageBox.Show("Conversion of file " + e.Args[i] + " failed!\nError: " + core.LastError, "Error!",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            NormalStartup();
                        }
                    }

                    //Application.Current.Shutdown();
                    break;
                }
            }
        }
    }
}
