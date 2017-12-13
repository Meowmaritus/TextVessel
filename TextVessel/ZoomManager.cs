using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TextVessel
{
    public class ZoomManager : INotifyPropertyChanged
    {
        static int ZoomLevelMin = -5;
        static int ZoomLevelMax = 20;

        double _zoomLevel = 0;
        public double ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                double newVal = value;

                if (newVal > ZoomLevelMax)
                    newVal = ZoomLevelMax;
                else if (newVal < ZoomLevelMin)
                    newVal = ZoomLevelMin;

                _zoomLevel = newVal;
                RaisePropertyChanged(nameof(GuiScale));
                RaisePropertyChanged(nameof(GuiDispString));
            }
        }

        static double ZoomLevelCoefIncrementGreater = 0.2;
        static double ZoomLevelCoefIncrementLesser = 0.1;

        public double BaseZoomScale { get; set; } = 0.75;

        double ZoomCoef 
            => 1.0 + ZoomLevel * 
            ((ZoomLevel >= 0) ? ZoomLevelCoefIncrementGreater : ZoomLevelCoefIncrementLesser);

        public double GuiScale
            => BaseZoomScale * ZoomCoef;

        public string GuiDispString => $"Zoom: {ZoomCoef:P}";



        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
