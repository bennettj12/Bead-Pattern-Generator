﻿using System;
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
        List<TextBlock> paletteEntries = new List<TextBlock>();
        Database db = new Database();
        int selectedPalette = -1;
        public MainWindow()
        {
            // setup db
            db.createDbFile();
            Debug.WriteLine(db.createDbConnection());
            db.createTables();
            InitializeComponent();

            loadPalettes();
            clearPaletteView();
            refreshPaletteNames();
        }

        private void savePalettes()
        {
            foreach(var palette in palettes)
            {
                string name = palette.Item1;
                string colors = String.Join(",", palette.Item2);
                db.addPalette(name, colors);
            }
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

        private void NewColorPalette(object sender, RoutedEventArgs e)
        {
            var dialog = new NamePopup();
            if (dialog.ShowDialog() == true)
            {
                foreach(var palette in palettes)
                {
                    if(palette.Item1 == dialog.ResponseText)
                    {
                        MessageBox.Show("Palette with this name already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                palettes.Add(new Tuple<string, List<string>>(dialog.ResponseText, new List<string>()));
                //MessageBox.Show("Palette created: " + dialog.ResponseText + "PaletteCount: " + palettes.Count());
                refreshPaletteNames();
                savePalettes();
            }



        }


        private void refreshPaletteNames()
        {
            PalettesList.Children.Clear();
            paletteEntries.Clear();
            var title = new TextBlock();
            title.Text = "Palettes ";
            PalettesList.Children.Add(title);
            PalettesList.Children.Add(new Separator());

            foreach (var palette in palettes)
            {
                var palEntry = new TextBlock();
                palEntry.MouseEnter += new MouseEventHandler(PaletteName_Enter);
                palEntry.MouseLeave += new MouseEventHandler(PaletteName_Exit);
                palEntry.MouseDown += new MouseButtonEventHandler(PaletteName_Click);
                palEntry.Text = palette.Item1;
                palEntry.Name = "pal_" + palettes.IndexOf(palette);
                PalettesList.Children.Add(palEntry);
                paletteEntries.Add(palEntry);
            }



        }

        private void PaletteName_Enter(object sender, MouseEventArgs e)
        {
            var name = ((TextBlock)e.Source).Name;
            if (selectedPalette != Int32.Parse(name.Split('_')[1]))
                ((TextBlock)e.Source).Background = Brushes.LightGray;
        }
        private void PaletteName_Exit(object sender, MouseEventArgs e)
        {
            var name = ((TextBlock)e.Source).Name;
            if (selectedPalette != Int32.Parse(name.Split('_')[1]))
                ((TextBlock)e.Source).Background = Brushes.Transparent;
        }
        private void PaletteName_Click(object sender, MouseButtonEventArgs e)
        {
            var name = ((TextBlock)e.Source).Name;
            //names are formatted as pal_{id}
            selectedPalette = Int32.Parse(name.Split('_')[1]);
            populatePaletteView();
            //reset coloring
            foreach (var pal in paletteEntries)
            {
                pal.Background = Brushes.Transparent;
                pal.Foreground = Brushes.Black;
            }
            // darken selected palette
            ((TextBlock)e.Source).Background = Brushes.Gray;
            ((TextBlock)e.Source).Foreground = Brushes.White;





            //TODO: load palette into center window



        }
        private void populatePaletteView()
        {
            var paletteToView = palettes[selectedPalette];
            PaletteViewTitle.Text = paletteToView.Item1;
            PaletteDisplay.Items.Clear();
            foreach (string color in paletteToView.Item2)
            {
                if(color != "")
                {
                    ListViewItem item = new ListViewItem();
                    item.Content = color;
                    item.Background = (Brush)new BrushConverter().ConvertFromString(color);
                    PaletteDisplay.Items.Add(item);
                }
            }
        }
        private void clearPaletteView()
        {
            selectedPalette = -1;
            PaletteViewTitle.Text = "Select a Palette";
            PaletteDisplay.Items.Clear();
        }
        private void AddColor(object sender, RoutedEventArgs e)
        {
            if (selectedPalette >= 0)
            {
                var selectedColor = ClrPcker_Background.SelectedColor;
                if (palettes[selectedPalette].Item2.Contains(selectedColor.ToString())) {
                    MessageBox.Show("Color already in palette", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (selectedColor != null)
                {
                    palettes[selectedPalette].Item2.Add(selectedColor.ToString());
                    populatePaletteView();
                    savePalettes();
                }
                else
                    MessageBox.Show("No color selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            } else
            {
                MessageBox.Show("No palette selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteColor(object sender, RoutedEventArgs e)
        {
            if(PaletteDisplay.SelectedItem != null)
            {
               
                palettes[selectedPalette].Item2.Remove(((ListViewItem)PaletteDisplay.SelectedItem).Content.ToString());
                populatePaletteView();
                savePalettes();
                
            } else
            {
                MessageBox.Show("No color selected", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void DeletePalette(object sender, RoutedEventArgs e)
        {
            if(selectedPalette < 0)
            {
                MessageBox.Show("No palette selected to delete", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            } 

            if(MessageBox.Show("Are you sure you want to delete " + palettes[selectedPalette].Item1 + "?","Confirm deletion",MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                db.removePalette(palettes[selectedPalette].Item1);
                palettes.RemoveAt(selectedPalette);
                clearPaletteView();
                refreshPaletteNames();
            }
        }
    }
}
