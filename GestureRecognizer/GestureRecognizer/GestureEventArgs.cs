using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureRecognizer
{

    public class GestureEventArgs : EventArgs
    {

        public RecognitionResult Result { get; internal set; }
        public GestureType GestureType { get; internal set; }
        
        public GestureEventArgs(RecognitionResult result, GestureType type)
        {
            this.Result = result;
            this.GestureType = type;
        }

    }

}
