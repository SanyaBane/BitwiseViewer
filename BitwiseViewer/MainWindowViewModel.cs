using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace BitwiseTranslator
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

        public MainWindowViewModel()
        {
            BitwiseTable = new Dictionary<int, string>();

            string inputTable = File.ReadAllText("InputTable.xml");

            XDocument doc = XDocument.Parse(inputTable);

            var root = doc.Element("root");

            int bitValue = 1;
            foreach (XElement bitXmlElement in root.Elements())
            {
                var bitName = bitXmlElement.Attribute("name").Value;
                BitwiseTable.Add(bitValue, bitName);

                // edit bit for next iteration
                bitValue *= 2;
            }

            NotifyPropertyChanged(nameof(BitwiseTable));
        }
    }
}
