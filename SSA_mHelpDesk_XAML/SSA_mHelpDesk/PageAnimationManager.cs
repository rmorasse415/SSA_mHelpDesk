using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace SSA_mHelpDesk
{
    class ContentAnimationManager
    {
        private bool _animate = true;
        private Duration _duration = new Duration(TimeSpan.FromMilliseconds(150));

        private bool _allowDirectNavigation = false;
        private object _content = null;
        private readonly ContentControl _frame;
        private readonly UIElement _parent;

        public bool Animate { get => _animate; set => _animate = value; }
        public Duration Duration { get => _duration; set => _duration = value; }

        public ContentAnimationManager(UIElement parent, ContentControl frame)
        {
            _parent = parent;
            _frame = frame;
        }

        public void AnimateToContent(object content)
        {
            //TODO queue up animations to preventoverlayying them
            if (Animate)
            {
                _content = content;
                DoubleAnimation fadeOutAnimation = new DoubleAnimation
                {
                    From = _frame.Opacity,
                    To = 0,
                    Duration = _duration,
                };
                fadeOutAnimation.Completed += AnimateComplete;
                _frame.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);
            }
            else
                _frame.Content = content;
        }

        private void AnimateComplete(object sender, EventArgs e)
        {
            _frame.Content = _content;
            
            _frame.Opacity = 0;
            _parent.Dispatcher.BeginInvoke(DispatcherPriority.Loaded,
                (ThreadStart)delegate ()
                {
                    DoubleAnimation fadeInAnimation = new DoubleAnimation()
                    {
                        From = 0,
                        To = 1,
                        Duration = _duration,
                    };
                    _frame.BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
                });
        }
    }
}
