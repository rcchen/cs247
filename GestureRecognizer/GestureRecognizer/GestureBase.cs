using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace GestureRecognizer
{
    public abstract class GestureBase
    {

        public bool IsRecognitionStarted { get; set; }
        private int CurrentFrameCount { get; set; }
        public GestureType GestureType { get; set; }
        protected virtual int MaximumNumberOfFrameToProcess { get { return 15; } }
        public long GestureTimeStamp { get; set; }
        protected abstract bool ValidateGestureStartCondition(Skeleton skeleton);
        protected abstract bool ValidateGestureEndCondition(Skeleton skeleton);
        protected abstract bool ValidateBaseCondition(Skeleton skeleton);
        protected abstract bool IsGestureValid(Skeleton skeleton);

        public virtual bool CheckForGesture(Skeleton skeleton)
        {
            if (this.IsRecognitionStarted == false)
            {
                if (this.ValidateGestureStartCondition(skeleton))
                {
                    this.IsRecognitionStarted = true;
                    this.CurrentFrameCount = 0;
                }
            }

            else
            {
                if (this.CurrentFrameCount == this.MaximumNumberOfFrameToProcess)
                {
                    this.IsRecognitionStarted = false;
                    if (ValidateBaseCondition(skeleton) && ValidateGestureEndCondition(skeleton))
                    {
                        return true;
                    }
                }

                this.CurrentFrameCount++;

                if (!IsGestureValid(skeleton) && !ValidateBaseCondition(skeleton))
                {
                    this.IsRecognitionStarted = false;
                }
            }

            return false;

        }


        public GestureBase(GestureType type)
        {

            this.CurrentFrameCount = 0;
            this.GestureType = type;

        }
            

    }
}
