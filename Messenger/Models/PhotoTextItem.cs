using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Messenger
{
    public class PhotoTextItem
    {
        public Brush Brush { get; set; }
        public ImageBrush avatar { get; set; }
        public string Text { get; set; }
    }
    public enum MessageSide
    {
        Right,
        Left
    }
}
