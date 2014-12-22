using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.Diagnostics;

namespace GestureRecognizer
{
    public class PullDownGesture : GestureBase
    {

        public PullDownGesture() : base(GestureType.PullDown) { }
        
        private SkeletonPoint validateLeftPosition;
        private SkeletonPoint validateRightPosition;
        private Stopwatch watch = new Stopwatch();
        private SkeletonPoint startingLeftPosition;
        private SkeletonPoint startingRightPosition;

        private float leftShoulderDiff;
        private float rightShoulderDiff;

        protected override bool ValidateGestureStartCondition(Microsoft.Kinect.Skeleton skeleton)
        {

            var handRightPosition = skeleton.Joints[JointType.HandRight].Position;
            var handLeftPosition = skeleton.Joints[JointType.HandLeft].Position;
            var shoulderRightPosition = skeleton.Joints[JointType.ShoulderRight].Position;
            var shoulderLeftPosition = skeleton.Joints[JointType.ShoulderLeft].Position;
            //var spinePosition = skeleton.Joints[JointType.Spine].Position;

            //System.Diagnostics.Debug.WriteLine("hands: (" + handLeftPosition.Y + "," + handRightPosition.Y + ") && shoulders: (" +
            //    shoulderLeftPosition.Y + "," + shoulderRightPosition.Y + ")");

            if ((handRightPosition.Y > shoulderRightPosition.Y))
            {

                rightShoulderDiff = GestureHelper.GetJointDistance(skeleton.Joints[JointType.HandRight], skeleton.Joints[JointType.ShoulderRight]);
                leftShoulderDiff = GestureHelper.GetJointDistance(skeleton.Joints[JointType.HandLeft], skeleton.Joints[JointType.ShoulderLeft]);

                validateLeftPosition = skeleton.Joints[JointType.HandLeft].Position;
                startingLeftPosition = skeleton.Joints[JointType.HandLeft].Position;

                validateRightPosition = skeleton.Joints[JointType.HandRight].Position;
                startingRightPosition = skeleton.Joints[JointType.HandRight].Position;
                
                System.Diagnostics.Debug.WriteLine("PullDownGesture start condition validated");
                watch.Restart();
                return true;

            }
            watch.Reset();
            return false;

        }

        protected override bool ValidateGestureEndCondition(Microsoft.Kinect.Skeleton skeleton)
        {

            var handRightPosition = skeleton.Joints[JointType.HandRight].Position;
            var handLeftPosition = skeleton.Joints[JointType.HandLeft].Position;
            var shoulderRightPosition = skeleton.Joints[JointType.ShoulderRight].Position;
            var shoulderLeftPosition = skeleton.Joints[JointType.ShoulderLeft].Position;
            //var spinePosition = skeleton.Joints[JointType.Spine].Position;

            double leftDelta = startingLeftPosition.Y - validateLeftPosition.Y;
            double rightDelta = startingRightPosition.Y - validateRightPosition.Y;

            if (rightDelta > 0.13)
            {
                //System.Diagnostics.Debug.WriteLine("PullDownGesture end condition validated");
                watch.Stop();
                Debug.WriteLine("Time is");
                Debug.WriteLine(watch.ElapsedMilliseconds.ToString());
                Debug.WriteLine("=====");
                if (watch.ElapsedMilliseconds <= 200) return false;
                Debug.WriteLine(watch.ElapsedMilliseconds.ToString());
                Debug.WriteLine("*******");
                return true;
            }
            return false;

        }

        protected override bool ValidateBaseCondition(Microsoft.Kinect.Skeleton skeleton)
        {

            var handRightPosition = skeleton.Joints[JointType.HandRight].Position;
            var handLeftPosition = skeleton.Joints[JointType.HandLeft].Position;
            var shoulderRightPosition = skeleton.Joints[JointType.ShoulderRight].Position;
            var shoulderLeftPosition = skeleton.Joints[JointType.ShoulderLeft].Position;

            var spinePosition = skeleton.Joints[JointType.Spine].Position;

            //System.Diagnostics.Debug.WriteLine("Attempting validation of base...");

            if ((handRightPosition.Y < shoulderRightPosition.Y))
            {
                //System.Diagnostics.Debug.WriteLine("PullDownGesture validated");
                return true;
            }
        
            return false;

        }

        protected override bool IsGestureValid(Microsoft.Kinect.Skeleton skeleton)
        {

            var currentHandRightPosition = skeleton.Joints[JointType.HandRight].Position;
            var currentHandLeftPosition = skeleton.Joints[JointType.HandLeft].Position;
            
            if ((validateRightPosition.Y < currentHandRightPosition.Y))
            {
               
                return false;
            }

            validateRightPosition = currentHandRightPosition;
            validateLeftPosition = currentHandLeftPosition;
            //Debug.WriteLine("IsGestureValid");
            return true;

        }

    }
}
