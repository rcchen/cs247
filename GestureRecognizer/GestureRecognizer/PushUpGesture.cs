using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.Diagnostics;
//using Kinect.Toolbox;


namespace GestureRecognizer
{
    public class PushUpGesture : GestureBase
    {

        public PushUpGesture() : base(GestureType.PushUp) { }
        private SkeletonPoint validateLeftPosition;
        private SkeletonPoint validateRightPosition;
        private Stopwatch watch  = new Stopwatch();
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

            if ((handRightPosition.Y < shoulderRightPosition.Y))
            {

                rightShoulderDiff = GestureHelper.GetJointDistance(skeleton.Joints[JointType.HandRight], skeleton.Joints[JointType.ShoulderRight]);
                leftShoulderDiff = GestureHelper.GetJointDistance(skeleton.Joints[JointType.HandLeft], skeleton.Joints[JointType.ShoulderLeft]);

                validateLeftPosition = skeleton.Joints[JointType.HandLeft].Position;
                startingLeftPosition = skeleton.Joints[JointType.HandLeft].Position;

                validateRightPosition = skeleton.Joints[JointType.HandRight].Position;
                startingRightPosition = skeleton.Joints[JointType.HandRight].Position;

                watch.Restart();
                return true;
            }

            return false;

        }

        protected override bool ValidateGestureEndCondition(Microsoft.Kinect.Skeleton skeleton)
        {

            var handRightPosition = skeleton.Joints[JointType.HandRight].Position;
            var handLeftPosition = skeleton.Joints[JointType.HandLeft].Position;
            var shoulderRightPosition = skeleton.Joints[JointType.ShoulderRight].Position;
            var shoulderLeftPosition = skeleton.Joints[JointType.ShoulderLeft].Position;
            //var spinePosition = skeleton.Joints[JointType.Spine].Position;

            double leftDelta = Math.Abs(startingLeftPosition.Y - validateLeftPosition.Y);
            double rightDelta = Math.Abs(startingRightPosition.Y - validateRightPosition.Y);

            //System.Diagnostics.Debug.WriteLine("I'M HERE GUYS");

            if (rightDelta > 0.13)
            {
                //System.Diagnostics.Debug.WriteLine("PushUpGesture end condition validated");
                watch.Stop();
                if (watch.ElapsedMilliseconds >1000 || watch.ElapsedMilliseconds < 200) return false;
                //Debug.WriteLine("Watch elapsed time for pull down: " + watch.ElapsedMilliseconds.ToString());
                return true;
            }

            return false;

            /*double distance = Math.Abs(startingPosition.X - validatePosition.X);
            float currentShoulderDiff = GestureHelper.GetJointDistance(skeleton.Joints[JointType.HandRight], skeleton.Joints[JointType.ShoulderLeft]);

            if (distance > 0.1 && currentShoulderDiff < shoulderDiff)
            {
                return true;
            }

            return false;*/

        }

        protected override bool ValidateBaseCondition(Microsoft.Kinect.Skeleton skeleton)
        {

            var handRightPosition = skeleton.Joints[JointType.HandRight].Position;
            var handLeftPosition = skeleton.Joints[JointType.HandLeft].Position;
            var shoulderRightPosition = skeleton.Joints[JointType.ShoulderRight].Position;
            var shoulderLeftPosition = skeleton.Joints[JointType.ShoulderLeft].Position;

            var spinePosition = skeleton.Joints[JointType.Spine].Position;

            if ((handRightPosition.Y > shoulderRightPosition.Y))
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

            if ((validateRightPosition.Y > currentHandRightPosition.Y))
            {
                return false;
            }

            validateRightPosition = currentHandRightPosition;
            validateLeftPosition = currentHandLeftPosition;
            //System.Diagnostics.Debug.WriteLine("PullDownGesture is valid! ValidateData are Hands(" + validateLeftPosition.Y + "," + validateRightPosition.Y + ")" );
            //System.Diagnostics.Debug.WriteLine("========================= OriignalData are Hands(" + startingLeftPosition.Y + "," + startingRightPosition.Y + ")");

            return true;

        }
    }
}
