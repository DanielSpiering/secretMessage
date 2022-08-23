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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;

namespace secretMessage {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            
        }//end main
        //GLOBAL VARS
        BitmapMaker _loadedImage;
        string _ImageFilePath;
        string _saveFile;
        int _height;
        int _width;
        string _idHeader;
        byte[] _p6Array;
        string[] _p3Array;
        int _index=0;
        int _pixelBytes;
        string _secretMessage;
        
        private void LoadImage_Click(object sender, RoutedEventArgs e) {
            //openfiledailog to open the file you want
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true) {
                _ImageFilePath = openFileDialog.FileName;
                                      
            }//end if

            if (_ImageFilePath!=null) {
                //read all line of file and store to a string array
                _p3Array = File.ReadAllLines(_ImageFilePath);

                ReadHeaders();

                if (_idHeader == "P3") {//for a P3 file

                    SetP3Pixels();

                    LoadImage();

                }//end if
                if (_idHeader == "P6") {//for a P6 file
                                        //read all bytes from the file and store to a byte array
                    _p6Array = File.ReadAllBytes(_ImageFilePath);

                    SetP6Pixels();

                    LoadImage();

                }//end if      

                //once image is loaded various boxes appear on the form
                if (imgLoadedImage.IsLoaded) {
                    lblEncodedMessage.Visibility = Visibility.Visible;
                    txtEncodedMessage.Visibility = Visibility.Visible;
                    lblCharacterCount.Visibility = Visibility.Visible;
                    lblCharacterCountLabel.Visibility = Visibility.Visible;
                    btnEncode.Visibility = Visibility.Visible;
                }//end if
            } else {

            }
            
        }//end LoadImage Click Event
        private void Encode_Click(object sender, RoutedEventArgs e) {
            Encode();
        }//end Encode Click Event
        private void btnEncode_Click(object sender, RoutedEventArgs e) {
            Encode();
        }//end Encode menu btn
        private void SaveImage_Click(object sender, RoutedEventArgs e) {
            //open dialog and force to save file as ppm
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PPM File | *.ppm";
            saveFileDialog.ShowDialog();
            //get the name you just gave the file
            _saveFile = saveFileDialog.FileName;

            if (_idHeader == "P3") {
                //write all lines to the file
                StreamWriter newStreamWriter = new StreamWriter(_saveFile);
                for (int index = 0; index < _p3Array.Length; index++) {
                    newStreamWriter.WriteLine(_p3Array[index]);
                }//end for
                newStreamWriter.Close();
            }//end if

            if (_idHeader == "P6") {
                //write all bytes to the file
                FileStream outfile = new FileStream(_saveFile, FileMode.Create, FileAccess.Write, FileShare.Write);
                for (int index = 0; index < _p6Array.Length; index++) {
                    outfile.WriteByte(_p6Array[index]);
                }
                outfile.Close();
                
            }//end if
        }//end SaveImage Click Event
        private void Exit_Click(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }//end Exit Click Event
        private void SetP6Pixels() {
            int index = _p6Array.Length-_pixelBytes;
            //set pixel data for the image
            for (int y = 0; y < _height; y++) {
                for (int x = 0; x < _width; x++) {
                    _loadedImage.SetPixel(x, y, _p6Array[index], _p6Array[index + 1], _p6Array[index + 2]);
                    index += 3;
                }//end for
            }//end for
        }//end SetP6Pixels
        private void SetP3Pixels() {
            _index = _p3Array.Length - _pixelBytes;
            //set pixel data for the image
            for (int y = 0; y < _height; y++) {
                for (int x = 0; x < _width; x++) {
                    _loadedImage.SetPixel(x, y, byte.Parse(_p3Array[_index]), byte.Parse(_p3Array[_index + 1]), byte.Parse(_p3Array[_index + 2]));
                    _index += 3;
                }//end for
            }//end for
        }//end SetP3Pixels
        private bool VerifyTextBox() {
            _secretMessage = txtEncodedMessage.Text;
            if (_secretMessage.Length > 256) {                
                MessageBoxResult result = MessageBox.Show("Your message is too long. Max length is 256 characters");
                return false;
            }//end if
            if (_height * _width < _secretMessage.Length * 3) {
                MessageBoxResult result = MessageBox.Show("Your messgae is too big to fit in this image");
                return false;
            }//end if
            return true;
        }//end VerifyTextBox      
        private void ReadHeaders() {
                     
            _index = 0;
            //read first line of file to get the type
            _idHeader = _p3Array[_index];
            _index += 1;

            //store second line of file which should be a comment 
            string record = _p3Array[_index];

            while (record[0] == '#') {//read throught all the comments
                record = _p3Array[_index];
                _index += 1;
            }//end while
            
            //split the next line to get the width and height of the image
            string[] fields = record.Split(' ');
            _width = int.Parse(fields[0]);
            _height = int.Parse(fields[1]);

            //get number of bytes in the pixels minus the alpha value from each pixel
            _pixelBytes = _height * _width * 3;

            //skip next line which is the max rgb channel           
            record =_p3Array[_index];           
            _index += 1;

            //set up a new bitmap maker with the width and height read from the file
            _loadedImage = new BitmapMaker(_width, _height);         
        }//end ReadHeaders
        private void LoadImage() {
            //convert pixel data into writable bitmap
            WriteableBitmap wbmImage = _loadedImage.MakeBitmap();

            //set nearest neighbor rendering mode for image box
            RenderOptions.SetBitmapScalingMode(imgLoadedImage, BitmapScalingMode.NearestNeighbor);

            //set uniform stretching to scale image cleanly 
            imgLoadedImage.Stretch = Stretch.Uniform;

            //set source for image box to the bitmap
            imgLoadedImage.Source = wbmImage;
                       
        }//end LoadImage
        private void LoadEncodedImage() {
            //convert pixel data into writable bitmap
            WriteableBitmap wbmImage = _loadedImage.MakeBitmap();

            //set nearest neighbor rendering mode for image box
            RenderOptions.SetBitmapScalingMode(imgEncodedImage, BitmapScalingMode.NearestNeighbor);

            //set uniform stretching to scale image cleanly 
            imgEncodedImage.Stretch = Stretch.Uniform;

            //set source for image box to the bitmap
            imgEncodedImage.Source = wbmImage;
        }//end LoadEncodedImage
        private void txtEncodedMessage_TextChanged(object sender, TextChangedEventArgs e) {
            //read from text box
            string userInput = txtEncodedMessage.Text;
            //if the length of the string in the text box changes update charCount
            string charCount = userInput.Length.ToString();
            //update label with charCount
            lblCharacterCount.Content = charCount;
        }//end event
        private void Encode() {
            if (VerifyTextBox() == true) {
                if (_idHeader == "P3") {
                    //set starting point of message
                    int j = _p3Array.Length - 2;

                    //place each character in the message
                    for (int index = 0; index < _secretMessage.Length; index++) {
                        int character = (int)_secretMessage[index];
                        _p3Array[j] = character.ToString();
                        j -= 9;
                    }//end for

                    SetP3Pixels();

                    LoadEncodedImage();

                }//end if
                if (_idHeader == "P6") {
                    //set starting point of message
                    int j = _p6Array.Length - 2;

                    //place each character in the message
                    for (int index = 0; index < _secretMessage.Length; index++) {
                        int character = (int)_secretMessage[index];
                        _p6Array[j] = (byte)character;
                        j -= 9;
                    }//end for

                    SetP6Pixels();

                    LoadEncodedImage();

                }//end if
            } else {

            }                     
        }//end Encode
        
    }//end class
}//end namespace
