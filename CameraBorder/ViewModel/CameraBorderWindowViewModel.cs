using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CameraBorder.BaseClass;

namespace CameraBorder.ViewModel
{
    internal class CameraBorderWindowViewModel : NotifyObject
    {

        
        public string? LeftLine1String { get; set; }
        public string? LeftLine2String { get; set; }
        public string? LeftLine3String { get; set; }
        public string? RightLine1String { get; set; }
        public string? RightLine2String { get; set; }
        public string? RightLine3String { get; set; }
        public string? MiddleLine1String { get; set; }
        public string? MiddleLine2String { get; set; }
        public string? MiddleLine3String { get; set; }

        private bool _leftLine1Enable = false;
        public bool LeftLine1Enable
        {
            get => _leftLine1Enable;
            set
            {
                if (_leftLine1Enable == value) return;
                _leftLine1Enable = value;
                RaisePropertyChanged(nameof(LeftLine1Enable));
            }
        }

        private bool _leftLine2Enable = false;
        public bool LeftLine2Enable
        {
            get => _leftLine2Enable;
            set
            {
                if (_leftLine2Enable == value) return;
                _leftLine2Enable = value;
                RaisePropertyChanged(nameof(LeftLine2Enable));
            }
        }

        private bool _leftLine3Enable = false;
        public bool LeftLine3Enable
        {
            get => _leftLine3Enable;
            set
            {
                if (_leftLine3Enable == value) return;
                _leftLine3Enable = value;
                RaisePropertyChanged(nameof(LeftLine3Enable));
            }
        }

        private bool _rightLine1Enable = false;
        public bool RightLine1Enable
        {
            get => _rightLine1Enable;
            set
            {
                if (_rightLine1Enable == value) return;
                _rightLine1Enable = value;
                RaisePropertyChanged(nameof(RightLine1Enable));
            }
        }

        private bool _rightLine2Enable = false;
        public bool RightLine2Enable
        {
            get => _rightLine2Enable;
            set
            {
                if (_rightLine2Enable == value) return;
                _rightLine2Enable = value;
                RaisePropertyChanged(nameof(RightLine2Enable));
            }
        }

        private bool _middleLine1Enable = false;
        public bool MiddleLine1Enable
        {
            get => _middleLine1Enable;
            set
            {
                if (_middleLine1Enable == value) return;
                _middleLine1Enable = value;
                RaisePropertyChanged(nameof(MiddleLine1Enable));
            }
        }

        private bool _middleLine2Enable = false;
        public bool MiddleLine2Enable
        {
            get => _middleLine2Enable;
            set
            {
                if (_middleLine2Enable == value) return;
                _middleLine2Enable = value;
                RaisePropertyChanged(nameof(MiddleLine2Enable));
            }
        }

        private bool _middleLine3Enable = false;
        public bool MiddleLine3Enable
        {
            get => _middleLine3Enable;
            set
            {
                if (_middleLine3Enable == value) return;
                _middleLine3Enable = value;
                RaisePropertyChanged(nameof(MiddleLine3Enable));
            }
        }


    }
}
