using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace GestureRecognizer
{
    public class SwipeToRightGesture : GestureBase
    {

        public SwipeToRightGesture()
            : base(GestureType.SwipeRight) { }

        private SkeletonPoint validatePosition;
        private SkeletonPoint startingPostion;

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
              (handRightPoisition.Y > skeleton.Joints[JointType.ElbowRight].Position.Y) && (handLeftPosition.Y < spinePosition.Y))
            {
                validatePosition = skeleton.Joints[JointType.HandRight].Position;
                startingPostion = skeleton.Joints[JointType.HandRight].Position;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether [is gesture valid] [the specified skeleton data].
        /// </summary>
        /// <param name="skeleton">The skeleton data.</param>
        /// <returns>
        ///   <c>true</c> if [is gesture valid] [the specified skeleton data]; otherwise, <c>false</c>.
        /// </returns>
        protected override bool IsGestureValid(Skeleton skeleton)
        {
            var currentHandRightPoisition = skeleton.Joints[JointType.HandRight].Position;
            if (validatePosition.X > currentHandRightPoisition.X)
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
            double distance = Math.Abs(validatePosition.X - startingPostion.X);

            if (distance > 0.1)
                return true;

            return false;
        }

        /// <summary>
        /// Valids the base condition.
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
