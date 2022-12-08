using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
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
        string workingFilePath = "";

        static int CELL_SIZE = 25;

        public MainWindow()
        {
            // setup db
            db.createDbFile();
            //Debug.WriteLine(db.createDbConnection());
            db.createTables();
            InitializeComponent();

            loadPalettes();
            refreshPaletteDropDown();
            clearPaletteView();
            refreshPaletteNames();
        }

        private List<string> getPalette(string name)
        {
            return palettes.Find(pal => pal.Item1 == name).Item2;
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
        private void refreshPaletteDropDown()
        {
            PaletteDropDown.Items.Clear();
            foreach(Tuple<string, List<string>> p in palettes)
            {
                PaletteDropDown.Items.Add(new ComboBoxItem().Content = p.Item1);
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
                refreshPaletteDropDown();
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
                refreshPaletteDropDown();
            }
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG";
            if(openFileDialog.ShowDialog() == true)
            {
                PathDisplay.Text = openFileDialog.FileName;
                ImageDisplay.Source = (ImageSource) new ImageSourceConverter().ConvertFromString(openFileDialog.FileName);
                ImageLoadedLabel.Content = openFileDialog.FileName;
            }
        }
        private Color findClosestColor(Color cRef, List<Color> palette)
        {
            Color closest = new Color();
            double bestDiff = -1;
            
            //double cRefMagnitude = Math.Sqrt((Math.Pow(cRef.R, 2) + Math.Pow(cRef.G, 2) + Math.Pow(cRef.B, 2)));
            
            foreach(Color c in palette)
            {
                double diff = Math.Sqrt((Math.Pow(c.R - cRef.R, 2) + Math.Pow(c.G - cRef.G, 2) + Math.Pow(c.B - cRef.B, 2)));
                //double diff = Math.Abs(cRefMagnitude - cMagnitude);
                if(bestDiff == -1 || diff < bestDiff)
                {
                    closest = c;
                    bestDiff = diff;
                }
            }

            return closest;
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            // export to file
            ProcessImage_Click(sender, e);
            Mat original = CvInvoke.Imread(workingFilePath, ImreadModes.AnyColor);
            Image<Bgr, Byte> originalImage = original.ToImage<Bgr, Byte>();

            int width = originalImage.Width;
            int height = originalImage.Height;

            Image<Bgr, Byte> outputImage = new Image<Bgr, Byte>(width * CELL_SIZE, height * CELL_SIZE, new Bgr(255, 0 , 0));

            List<Color> sp = new List<Color>();
            foreach (string s in getPalette(PaletteDropDown.Text))
            {
                sp.Add((Color)ColorConverter.ConvertFromString(s));
            }

            for (int y = 0; y < width; y++)
            {
                for(int x = 0; x < height; x++)
                {
                    //originalImage Pixels
                    Bgr col = originalImage[x, y];
                    Bgr black = new Bgr(0,0,0);

                    for(int i = y * CELL_SIZE; i < y* CELL_SIZE + CELL_SIZE; i++)
                    {
                        for(int j = x * CELL_SIZE; j < x * CELL_SIZE + CELL_SIZE; j++)
                        {
                            if(
                                j == x * CELL_SIZE || 
                                j == x * CELL_SIZE + CELL_SIZE ||
                                i == y * CELL_SIZE ||
                                i == y * CELL_SIZE + CELL_SIZE
                            )
                            {
                                outputImage[j, i] = black;
                            } else
                            {
                                outputImage[j, i] = col;
                            }
                            
                        }
                    }
                    for(int i = 0; i < sp.Count; i++)
                    {
                        Color c = sp[i];
                        Bgr thiscol = new Bgr(c.B, c.G, c.R);
                        if (col.Equals(thiscol))
                        {
                            outputImage.Draw(((char)(65 + i)).ToString(), new System.Drawing.Point(y * CELL_SIZE + 1, x * CELL_SIZE + CELL_SIZE), FontFace.HersheyPlain, CELL_SIZE/10, black, 1, LineType.AntiAlias, false);
                        }

                    }
                    //outputImage.Draw("A", new System.Drawing.Point(y*CELL_SIZE + 1, x*CELL_SIZE + CELL_SIZE), FontFace.HersheyPlain, CELL_SIZE/16, black ,1,LineType.EightConnected,false);


                }
            }
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "PNG Image|*.png";
            saveFile.Title = "Save Chart";
            saveFile.ShowDialog();

            if(saveFile.FileName != "")
            {
                outputImage.Save(saveFile.FileName);
            }

            outputImage.Save(workingFilePath.Substring(0,workingFilePath.Length-4) +"final.png");
            ImageDisplay.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(workingFilePath.Substring(0, workingFilePath.Length - 4) + "final.png");
            // end processing



        }
        private void Export_CSV_Click(object sender, RoutedEventArgs e)
        {
            ProcessImage_Click(sender, e);
            Mat original = CvInvoke.Imread(workingFilePath, ImreadModes.AnyColor);
            Image<Bgr, Byte> originalImage = original.ToImage<Bgr, Byte>();

            int width = originalImage.Width;
            int height = originalImage.Height;
            var output = new System.Text.StringBuilder();
            output.AppendLine("sep=;");
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < height; x++)
                {
                    Bgr col = originalImage[y, x];
                    output.Append(col.ToString() + ";");
                }
                output.AppendLine();
            }
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "CSV file|*.csv";
            saveFile.Title = "Save CSV";
            saveFile.ShowDialog();
            File.WriteAllText(saveFile.FileName, output.ToString());
        }
        private void ProcessImage_Click(object sender, RoutedEventArgs e)
        {
            int height, width;

            if(!int.TryParse(PatHeight.Text, out height) || !int.TryParse(PatWidth.Text, out width))
            {
                MessageBox.Show("Please enter a valid number");
                return;
            }
            if(height < 1 || width < 1)
            {
                MessageBox.Show("Number must be greater than zero");
                return;
            }

            // get colors and convert to color format
            List<Color> sp = new List<Color>();
            if (PaletteDropDown.SelectedItem == null)
            {
                MessageBox.Show("You must select a palette first");
                return;
            }
            foreach(string s in getPalette(PaletteDropDown.Text))
            {
                sp.Add((Color) ColorConverter.ConvertFromString(s));
            }
            if(PathDisplay.Text == "")
            {
                MessageBox.Show("You must select an image to convert first");
                return;
            }
            if (!File.Exists(PathDisplay.Text))
            {
                MessageBox.Show("Invalid file");
                return;
            }
            switch (System.IO.Path.GetExtension(PathDisplay.Text).ToLower()) 
            {
                case ".gif":
                case ".png":
                case ".jpg":
                case ".jpeg":
                    break;
                default:
                    MessageBox.Show("Invalid file");
                    return;
            }

            Mat mat = CvInvoke.Imread(PathDisplay.Text, ImreadModes.AnyColor);

            Image<Bgr, Byte> img = mat.ToImage<Bgr, Byte>();
            img = img.Resize(width, height, Inter.Lanczos4);
            // [x,y,0 (blue)] [x,y,1 (green)] [x,y,2 (red)] 
            //MessageBox.Show("Top Left Color Info: " + img.Data[0,0,0] + " : " + img.Data[0, 0, 1] + " : " + img.Data[0, 0, 2]);


            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < height; x++)
                {
                    byte blue = img.Data[x, y, 0];
                    byte green = img.Data[x, y, 1];
                    byte red = img.Data[x, y, 2];
                    Color pixelColor = Color.FromRgb(red, green, blue);
                    Color matchedColor = findClosestColor(pixelColor, sp);
                    img.Data[x, y, 0] = matchedColor.B;
                    img.Data[x, y, 1] = matchedColor.G;
                    img.Data[x, y, 2] = matchedColor.R;

                }
            }

            string tFileName = System.IO.Path.GetTempFileName() + ".png";
            img.Save(tFileName);
            workingFilePath = tFileName;
            ImageDisplay.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(tFileName);
            
        }

        private void selectText(object sender)
        {
            ((TextBox)sender).SelectAll();
        }
        private void PatWidth_GotFocus(object sender, RoutedEventArgs e)
        {
            selectText(sender);
        }

        private void PatHeight_GotFocus(object sender, RoutedEventArgs e)
        {
            selectText(sender);
        }


    }
}
