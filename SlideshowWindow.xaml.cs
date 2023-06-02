using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace WPFLab2 {
    public partial class SlideshowWindow : Window {
        private List<BitmapImage> bitmapImages;
        private int currentBitmapIndex;
        private bool timePaused;
        private DispatcherTimer timer;
        private IEffect Effect;

        //https://www.codeproject.com/Articles/354702/WPF-Slideshow
        public SlideshowWindow(List<BitmapImage> bitmapList, IEffect effectArg) {
            if (bitmapList == null || bitmapList.Count == 0) {
                this.Close();
            }
            bitmapImages = new List<BitmapImage>(bitmapList);

            InitializeComponent();

            currentBitmapIndex = 0;
            timePaused = false;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += this.Timer_Tick;
            timer.Start();

            Effect = effectArg;
            Effect.SetDuration(3);
            Effect.SetHeight(this.Height);
            Effect.SetWidth(this.Width);
            
        }
        private void Timer_Tick(object sender, EventArgs e) {
            this.imageCanva.Source = bitmapImages[currentBitmapIndex];
            Effect.GetEffect(this.imageCanva);

            currentBitmapIndex++;
            if (currentBitmapIndex >= bitmapImages.Count) {
                currentBitmapIndex = 0;
            }

        }

        private void StopSlideshow_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void PlayPauseSlideshow_Click(object sender, RoutedEventArgs e) {

            if(timePaused) {
                timer.Start();
                timePaused = false;
            }
            else {
                timer.Stop();
                timePaused = true;
            }
        }
    }
}
