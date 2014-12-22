using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.Diagnostics;

namespace GestureRecognizer
{

    public class GestureRecognitionEngine
    {

        public int SkipFramesAfterGestureIsDetected = 0;
        public event EventHandler<GestureEventArgs> GestureRecognized;
        public GestureType GestureType { get; set; }
        public Skeleton Skeleton { get; set; }
        public bool IsGestureDetected { get; set; }
        //private Stopwatch watch;
        private List<GestureBase> gestureCollection = null;

        public GestureRecognitionEngine()
        {
           // watch = new Stopwatch();
            this.InitializeGesture();
            
        }


        /// <summary>
        /// Initialize the gesture
        /// </summary>
        private void InitializeGesture()
        {

            this.gestureCollection = new List<GestureBase>();
            this.gestureCollection.Add(new SwipeToRightGesture());
            this.gestureCollection.Add(new SwipeToLeftGesture());
            this.gestureCollection.Add(new PullDownGesture());
            this.gestureCollection.Add(new PushUpGesture());

        }

        /// <summary>
        /// Starts the gesture recognition. 
        /// </summary>
        public void StartRecognize()
        {
            Stopwatch watch = new Stopwatch();
            if (this.IsGestureDetected)
            {
                while (this.SkipFramesAfterGestureIsDetected <= 30)
                {
                    this.SkipFramesAfterGestureIsDetected++;
                }
                this.RestGesture();
                return;
            }

            foreach (var item in this.gestureCollection)
            {
                if (item.CheckForGesture(this.Skeleton))

               //     watch.Start();
                {
                    if (this.GestureRecognized != null)
             //           watch.Stop();
                    {
                        this.GestureRecognized(this, new GestureEventArgs(RecognitionResult.Success, item.GestureType));
                        this.IsGestureDetected = true;
                       // Debug.WriteLine(watch.ElapsedMilliseconds.ToString());
                    }
                }
            }

        }


        /// <summary>
        /// Rests the gesture.
        /// </summary>
        private void RestGesture()
        {

            this.gestureCollection = null;
            this.InitializeGesture();
            this.SkipFramesAfterGestureIsDetected = 0;
            this.IsGestureDetected = false;
           // watch = new Stopwatch();
        }



    }
}
