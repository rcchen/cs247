
namespace GestureRecognizer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Kinect;

    /// <summary>
    /// Swipe to Left Gesture
    /// </summary>
    public class SwipeToLeftGesture : GestureBase
    {
        private SkeletonPoint validatePosition;
        private SkeletonPoint startingPostion;
        private float shoulderDiff;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwipeToLeftGesture" /> class.
        /// </summary>
        public SwipeToLeftGesture() : base(GestureType.SwipeLeft) { }


        /// <summary>
        /// Validates the gesture start condition.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns></returns>
        protected override bool ValidateGestureStartCondition(Skeleton skeleton)
        {
            var handRightPoisition = skeleton.Joints[JointType.HandRight].Position;
            var handLeftPosition = skeleton.Joints[JointType.HandLeft].Position;
            var shoulderRightPosition = skeleton.Joints[JointType.ShoulderRight].Position;
            var spinePosition = skeleton.Joints[JointType.Spine].Position;
            if ((handRightPoisition.Y < shoulderRightPosition.Y) &&
                 (handRightPoisition.Y > skeleton.Joints[JointType.ElbowRight].Position.Y) && handLeftPosition.Y < spinePosition.Y)
            {

                shoulderDiff = GestureHelper.GetJointDistance(skeleton.Joints[JointType.HandRight], skeleton.Joints[JointType.ShoulderLeft]);
                validatePosition = skeleton.Joints[JointType.HandRight].Position;
                startingPostion = skeleton.Joints[JointType.HandRight].Position;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether [is gesture valid] [the specified skeleton data].
        /// </summary>
        /// <param name="skeletonData">The skeleton data.</param>
        /// <returns>
        ///   <c>true</c> if [is gesture valid] [the specified skeleton data]; otherwise, <c>false</c>.
        /// </returns>
        protected override bool IsGestureValid(Skeleton skeletonData)
        {
            var currentHandRightPoisition = skeletonData.Joints[JointType.HandRight].Position;
            if (validatePosition.X < currentHandRightPoisition.X)
            {
                return false;
            }
            validatePosition = currentHandRightPoisition;
            return true;
        }

        /// <summary>
        /// Validates the gesture end condition.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns></returns>
        protected override bool ValidateGestureEndCondition(Skeleton skeleton)
        {
            double distance = Math.Abs(startingPostion.X - validatePosition.X);
            float currentshoulderDiff = GestureHelper.GetJointDistance(skeleton.Joints[JointType.HandRight], skeleton.Joints[JointType.ShoulderLeft]);

            if (distance > 0.1 && currentshoulderDiff < shoulderDiff)
                return true;


            return false;
        }

        /// <summary>
        /// Validates the base condition.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns></returns>
        protected override bool ValidateBaseCondition(Skeleton skeleton)
        {
            var handRightPoisition = skeleton.Joints[JointType.HandRight].Position;
            var handLeftPosition = skeleton.Joints[JointType.HandLeft].Position;
            var shoulderRightPosition = skeleton.Joints[JointType.ShoulderRight].Position;
            var spinePosition = skeleton.Joints[JointType.Spine].Position;
            if ((handRightPoisition.Y < shoulderRightPosition.Y) &&
                 (handRightPoisition.Y > skeleton.Joints[JointType.ElbowRight].Position.Y) && (handLeftPosition.Y < spinePosition.Y))
            {
                return true;
            }
            return false;
        }
    }
}