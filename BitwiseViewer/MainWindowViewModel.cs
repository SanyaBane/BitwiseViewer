using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Xml.Linq;

namespace BitwiseViewer
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private int _InputBit = 0;
        public int InputBit
        {
            get => _InputBit;
            set
            {
                _InputBit = value;
                NotifyPropertyChanged();

                CalculateUsedBitsFromInputBit(_InputBit);
            }
        }

        private int _ResultBit = 0;
        public int ResultBit
        {
            get => _ResultBit;
            set { _ResultBit = value; NotifyPropertyChanged(); }
        }

        private Dictionary<int, string> _BitwiseTable;
        public Dictionary<int, string> BitwiseTable
        {
            get => _BitwiseTable;
            set { _BitwiseTable = value; NotifyPropertyChanged(); }
        }

        private IList _SelectedNeededBits;
        public IList SelectedNeededBits
        {
            get => _SelectedNeededBits;
            set
            {
                _SelectedNeededBits = value;
                NotifyPropertyChanged();

                CalculateResultBitFromSelectedBits(SelectedNeededBits);
            }
        }

        private List<string> _ResultOfBitwiseOperation;
        public List<string> ResultOfBitwiseOperation
        {
            get => _ResultOfBitwiseOperation;
            set { _ResultOfBitwiseOperation = value; NotifyPropertyChanged(); }
        }

        #region OpenFileCommand
        private DelegateCommand _OpenFileCommand;
        public DelegateCommand OpenFileCommand
        {
            get => _OpenFileCommand;
            set { _OpenFileCommand = value; NotifyPropertyChanged(); }
        }

        public void ExecuteOpenFileCommand(object param)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Xml files (*.xml) | *.xml; | All files | *.*; "
            };
            if (openFileDialog.ShowDialog() == true)
            {
                if (!String.IsNullOrEmpty(openFileDialog.FileName))
                {
                    if (File.Exists(openFileDialog.FileName))
                        SelectFile(openFileDialog.FileName);
                }
            }
        }
        #endregion

        #region ExitCommand
        private DelegateCommand _ExitCommand;
        public DelegateCommand ExitCommand
        {
            get => _ExitCommand;
            set { _ExitCommand = value; NotifyPropertyChanged(); }
        }

        public void ExecuteExitCommand(object param)
        {
            System.Windows.Application.Current.Shutdown();
        }
        #endregion

        public MainWindowViewModel()
        {
            _OpenFileCommand = new DelegateCommand(ExecuteOpenFileCommand);
            _ExitCommand = new DelegateCommand(ExecuteExitCommand);
        }

        public void SelectFile(string pathToFile)
        {
            string inputFileReadAllText = File.ReadAllText(pathToFile);

            try
            {
                BitwiseTable = ParseXmlFile(inputFileReadAllText);
                NotifyPropertyChanged(nameof(BitwiseTable));

                InputBit = 0; //reset InputBit
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Dictionary<int, string> ParseXmlFile(string inputFileReadAllText)
        {
            var bitwiseTable = new Dictionary<int, string>();

            XDocument doc = XDocument.Parse(inputFileReadAllText);

            var root = doc.Element("root");

            int bitValue = 1;
            foreach (XElement bitXmlElement in root.Elements())
            {
                var bitName = bitXmlElement.Attribute("name").Value;
                bitwiseTable.Add(bitValue, bitName);

                // edit bit for next iteration
                bitValue *= 2;
            }

            return bitwiseTable;
        }

        private void CalculateUsedBitsFromInputBit(int bit)
        {
            if (BitwiseTable == null)
                return;

            var ret = new List<string>();

            foreach (var element in BitwiseTable)
            {
                var bitInTable = element.Key;
                var bitName = element.Value;

                string str = string.Format("[{0}, {1}]", bitInTable, bitName);

                if ((bit & bitInTable) > 0)
                    ret.Add(str);
            }

            ResultOfBitwiseOperation = ret;
        }

        private void CalculateResultBitFromSelectedBits(IList selectedBits)
        {
            int ret = 0;

            foreach (var element in selectedBits)
            {
                int elementKey = (int)((dynamic)element).Key;
                ret = ret | elementKey;
            }

            ResultBit = ret;
        }
    }
}
