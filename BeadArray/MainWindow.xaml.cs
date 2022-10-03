using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BeadArray
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    

    public partial class MainWindow : Window
    {
        List<Tuple<string, List<string>>> palettes = new List<Tuple<string, List<string>>>();
        Database db = new Database();
        public MainWindow()
        {
            // setup db
            db.createDbFile();
            Debug.WriteLine(db.createDbConnection());
            db.createTables();

            loadPalettes();
            InitializeComponent();
        }

        private void savePalettes()
        {

        }
        private void loadPalettes()
        {
            SQLiteDataReader reader = db.readTable();
            while (reader.Read())
            {
                string name = reader.GetString(0);
                string colors = reader.GetString(1);
                Tuple<string, List<string>> palette = new Tuple<string, List<string>>(name, new List<string>());
                foreach (string s in colors.Split(','))
                {
                    palette.Item2.Add(s);
                }
                palettes.Add(palette);
            }
        }

        private void ClrPcker_Background_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            string text = "#" + ClrPcker_Background.SelectedColor.ToString();
            Debug.WriteLine(text);
        }
    }
}
