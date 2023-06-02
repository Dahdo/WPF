using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace WPFLab2 {
    public interface IEffect {
        public void SetHeight(double height);
        public void SetWidth(double width);
        public void SetDuration(double duration);

        public void GetEffect(Image image);

    }

    //https://stackoverflow.com/questions/56259611/how-can-i-animate-an-image-from-one-point-to-another-programmatically-using-a-wp
    public class HorizontalEffect : IEffect {
        private double WindowWidth;
        private double WindowHeight;
        private double Duration;
        public HorizontalEffect() { }

        public void SetDuration(double duration) {
            this.Duration = duration;
        }

        public void SetHeight(double height) {
            this.WindowHeight = height;
        }

        public void SetWidth(double width) {
            this.WindowWidth = width;
        }

        public override string ToString() {
            return "Horizontal Effect";
        }

        public void GetEffect(Image image) {
            TranslateTransform translateTransform = new TranslateTransform();
            image.RenderTransform = translateTransform;
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = this.WindowWidth;
            animation.To = 0;
            animation.Duration = TimeSpan.FromSeconds(this.Duration);
            translateTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }
    }

    public class VerticalEffect : IEffect {
        private double WindowWidth;
        private double WindowHeight;
        private double Duration;

        public void SetDuration(double duration) {
            this.Duration = duration;
        }

        public void SetHeight(double height) {
            this.WindowHeight = height;
        }

        public void SetWidth(double width) {
            this.WindowWidth = width;
        }

        public override string ToString() {
            return "Vertical Effect";
        }
        public VerticalEffect() { }

        public void GetEffect(Image image) {
            TranslateTransform translateTransform = new TranslateTransform();
            image.RenderTransform = translateTransform;
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = this.WindowHeight;
            animation.To = 0;
            animation.Duration = TimeSpan.FromSeconds(this.Duration);
            translateTransform.BeginAnimation(TranslateTransform.YProperty, animation);
        }
    }

    public class OpacityEffect : IEffect {
        private double WindowWidth;
        private double WindowHeight;
        private double Duration;
        public OpacityEffect() { }

        public void SetDuration(double duration) {
            this.Duration = duration;
        }

        public void SetHeight(double height) {
            this.WindowHeight = height;
        }

        public void SetWidth(double width) {
            this.WindowWidth = width;
        }

        public override string ToString() {
            return "Opacity Effect";
        }
        public void GetEffect(Image image) {
            DoubleAnimation fadeOutAnimation = new DoubleAnimation();
            fadeOutAnimation.From = 1;
            fadeOutAnimation.To = 0;
            fadeOutAnimation.Duration = TimeSpan.FromSeconds(this.Duration);

            image.BeginAnimation(Image.OpacityProperty, fadeOutAnimation);

            DoubleAnimation fadeInAnimation = new DoubleAnimation();
            fadeInAnimation.From = 0;
            fadeInAnimation.To = 1;
            fadeInAnimation.Duration = TimeSpan.FromSeconds(this.Duration);

            image.BeginAnimation(Image.OpacityProperty, fadeInAnimation);

        }
    }
}
