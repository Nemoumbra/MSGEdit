using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace MSGEdit {
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application {
        public Settings settings; // when do I have to initialize them?
        public MSGtoTXT core;

        private bool convert_file(string path, PataponMessageFormat format) {
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
            return core.LastError == "";
        }
        private bool convert_file(string path) {
            PataponMessageFormat format = core.LoadAny(path);
            if (!core.isLoaded) {
                return false;
            }

            if (format == PataponMessageFormat.MSG) {
                core.SaveTXT(path + ".txt");
            }
            else {
                core.SaveMSG(path + ".msg");
            }
            return core.LastError == "";
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

        private Dictionary<string, string> get_settings_from_raw(string[] data) {
            Dictionary<string, string> res = new Dictionary<string, string>();
            foreach (string line in data) {
                string[] words = line.Split('=');
                if (words.Count() == 2) {
                    res.Add(words[0], words[1]);
                }
            }
            return res;
        }

        private void read_settings_from_file() {
            // This method is only called if the file exists
            // It's superfluous to restore default settings if some settings are omitted in the file
            try {
                string[] data;
                using (StreamReader stream = File.OpenText("settings.txt")) {
                    data = stream.ReadToEnd().Replace("\r", "").Split('\n');
                }
                Dictionary<string, string> dict = get_settings_from_raw(data);

                string value;
                if (dict.TryGetValue("Enable_warnings", out value)) {
                    settings.enable_warnings = Convert.ToBoolean(value);
                }
                if (dict.TryGetValue("Check_readme", out value)) {
                    settings.check_readme = Convert.ToBoolean(value);
                }
                if (dict.TryGetValue("Deduce_file_type", out value)) {
                    settings.deduce_file_type = Convert.ToBoolean(value);
                }
                if (dict.TryGetValue("Autopaste_from_clipboard", out value)) {
                    settings.autopaste_from_clipboard = Convert.ToBoolean(value);
                }
            }
            catch (Exception except) {
                MessageBox.Show("Error! " + except.Message + "\nUsing default settings", "Error!",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                settings.restore_default_settings();
            }
        }
        private void make_settings_file() {
            try {
                /*Enable_warnings=True
                  Check_readme=True
                  Deduce_file_type=True */
                using (StreamWriter stream = File.CreateText("settings.txt")) {
                    stream.WriteLine("Enable_warnings=" + settings.enable_warnings.ToString());
                    stream.WriteLine("Check_readme=" + settings.check_readme.ToString());
                    stream.WriteLine("Deduce_file_type=" + settings.deduce_file_type.ToString());
                    stream.Write("Autopaste_from_clipboard=" + settings.autopaste_from_clipboard.ToString());
                }
            }
            catch (Exception except) {
                MessageBox.Show("Error! " + except.Message + "\nUnable to make settings file");
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
                if (MessageBox.Show("Settings file not found.\nShould the new one be made?", "Prompt", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                    make_settings_file();
                }
            }

            /*
             * Allowed commands:
             * 1) 0 arguments => Standard launch (regardless of settings).
             * 
             * 2) 1 argument - the path to the file => Either automatic guessing or MessageBox happens here.
             * 
             * 3) 2 arguments - file path and -txt or -msg => This ignores the setting and 
             * launches the application in accordance with the specified type.
             * 
             * 4) If deduce is false, any odd number of arguments, where the first is -convert
             * and the rest are file paths with type specifications (-txt or -msg)
             * 
             * 5) If deduce is true, any number of arguments, where the first one is -convert
             * and the rest are file paths without type specifications
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

                    // If not for this part here, we could have used NormalStartup()
                    if (!settings.deduce_file_type) {

                        if (MessageBox.Show("Does this file contain MSG?", "Yes/No", MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.Yes) {
                            // Try to access the file and launch as PataponMessageFormat.MSG at workshop state
                            wnd.load_file(e.Args[0], PataponMessageFormat.MSG);
                        }
                        else {
                            // Try to access the file and launch as PataponMessageFormat.TXT at workshop state
                            wnd.load_file(e.Args[0], PataponMessageFormat.TXT);
                        }
                    }
                    else {
                        // Try to access the file, figure out its content and then launch at workshop state
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
                    /*
                     * 2 argument examples:
                     * -convert filename.msg
                     * mission.txt -txt
                     */
                    
                    // Special case:
                    if (e.Args[0] == "-convert" && settings.deduce_file_type) {
                        if (!convert_file(e.Args[1])) {
                            // Conversion failure
                            MessageBox.Show($"Conversion of file {e.Args[1]} failed!\nError: {core.LastError}",
                                "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        Application.Current.Shutdown();
                    }

                    if (e.Args[1] == "-txt") {
                        // Try to access the file and launch as PataponMessageFormat.TXT at workshop state
                        wnd.load_file(e.Args[0], PataponMessageFormat.TXT);
                    }
                    else if (e.Args[1] == "-msg") {
                        // Try to access the file and launch as PataponMessageFormat.MSG at workshop state
                        wnd.load_file(e.Args[0], PataponMessageFormat.MSG);
                    }
                    else {
                        MessageBox.Show($"Unknown Patapon message file type {e.Args[1]} (must be either -msg or -txt)",
                                "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    // The branch which was executed has already done everything needed
                    // If none of them were executed, nothing changes
                    if (settings.check_readme) {
                        wnd.check_readme();
                    }
                    wnd.Show();
                    break;
                }
                default: {
                    if (e.Args[0] != "-convert") {
                        // We ignore command line arguments
                        NormalStartup();
                    }
                    else {
                        // First argument is -convert
                        if (settings.deduce_file_type) {
                            // Expect any number of file paths
                            for (int i = 1; i < e.Args.Length; ++i) {
                                if (!convert_file(e.Args[i])) {
                                    // Conversion failure
                                    MessageBox.Show($"Conversion of file {e.Args[1]} failed!\nError: {core.LastError}", "Error!",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            Application.Current.Shutdown();
                        }
                        else {
                            // Expect odd number of args
                            if (e.Args.Length % 2 == 0) {
                                // Shutdown? Or maybe normal startup?
                                MessageBox.Show($"File type of {e.Args.Last()} is not specified!", "Error!", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                                Application.Current.Shutdown();
                                return;
                            }

                            int count = e.Args.Length / 2;
                            List<Tuple<string, PataponMessageFormat>> to_convert = new List<Tuple<string, PataponMessageFormat>>(count);
                            for (int i = 0; i < count; ++i) {
                                if (e.Args[2 * (i + 1)] == "-txt") {
                                    to_convert.Add(new Tuple<string, PataponMessageFormat>(e.Args[2 * i + 1], PataponMessageFormat.TXT));
                                }
                                else if (e.Args[2 * (i + 1)] == "-msg") {
                                    to_convert.Add(new Tuple<string, PataponMessageFormat>(e.Args[2 * i + 1], PataponMessageFormat.MSG));
                                }
                                else {
                                    // This is unacceptable, stop the operation
                                    MessageBox.Show($"Wrong file type specification: {e.Args[2 * (i + 1)]}" + 
                                        "\n(Must be either -msg or -txt)\nAborting conversion", 
                                        "Error!", MessageBoxButton.OK, MessageBoxImage.Error);

                                    Application.Current.Shutdown();
                                    return;
                                }
                            }

                            // now process every file one-by-one
                            for (int i = 0; i < count; ++i) {
                                if (!convert_file(to_convert[i].Item1, to_convert[i].Item2)) {
                                    // Conversion failure, do something
                                    MessageBox.Show("Conversion of file " + e.Args[i] + " failed!\nError: " + core.LastError, "Error!",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            Application.Current.Shutdown();
                        }
                    }
                    break;
                }
            }
        }
    }
}
