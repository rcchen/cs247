//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.Slideshow
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;
    using System.Windows.Controls;
    using Microsoft.Kinect;
    using Microsoft.Samples.Kinect.SwipeGestureRecognizer;
    using GestureRecognizer;
    using KinectPresentor;
    
    /*
     * public form1() { InitializeComponent();}
     * 
     * private void button(object sender, EventArgs e) {
     * Graphics g = this.CreateGraphics();
     * Pen black = new Pen(Color.Green, 10);
     * g.FillRectangle(green.Brush, new Rectangle(200, 200, 100, 300);
     * }
     */
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        /// <summary>
        /// The recognizer being used.
        /// </summary>
        private readonly Recognizer activeRecognizer;
        GestureRecognitionEngine recognitionEngine;

        /// <summary>
        /// The paths of the picture files.
        /// </summary>
        private readonly string[] picturePaths; 
        //private Stopwatch watch = new Stopwatch();
        /// <summary>
        /// Array of arrays of contiguous line segements that represent a skeleton.
        /// </summary>
        private static readonly JointType[][] SkeletonSegmentRuns = new JointType[][]
        {
            new JointType[] 
            { 
                JointType.Head, JointType.ShoulderCenter, JointType.HipCenter 
            },
            new JointType[] 
            { 
                JointType.HandLeft, JointType.WristLeft, JointType.ElbowLeft, JointType.ShoulderLeft,
                JointType.ShoulderCenter,
                JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, JointType.HandRight
            },
            new JointType[]
            {
                JointType.FootLeft, JointType.AnkleLeft, JointType.KneeLeft, JointType.HipLeft,
                JointType.HipCenter,
                JointType.HipRight, JointType.KneeRight, JointType.AnkleRight, JointType.FootRight
            }
        };

        /// <summary>
        /// The sensor we're currently tracking.
        /// </summary>
        private KinectSensor nui;



        private bool relatedActivated = false;
        private bool relatedJustDeactivated = false;

        /// <summary>
        /// There is currently no connected sensor.
        /// </summary>
        private bool isDisconnectedField = true;

        /// <summary>
        /// Any message associated with a failure to connect.
        /// </summary>
        private string disconnectedReasonField;

        /// <summary>
        /// Array to receive skeletons from sensor, resize when needed.
        /// </summary>
        public Skeleton[] skeletons = new Skeleton[0];

        /// <summary>
        /// Time until skeleton ceases to be highlighted.
        /// </summary>
        private DateTime highlightTime = DateTime.MinValue;
        
        /// <summary>
        /// The ID of the skeleton to highlight.
        /// </summary>
        private int highlightId = -1;

        /// <summary>
        /// The ID if the skeleton to be tracked.
        /// </summary>
        private int nearestId = -1;

        /// <summary>
        /// The index of the current image.
        /// </summary>
        private int indexField = 1;

        /// <summary>
        /// The stopwatch that we use to detect click events.
        /// </summary>
        private Stopwatch stopwatch;

        /// <summary>
        /// Current x value of pointer
        /// </summary>
        private double currentX = 600;

        /// <summary>
        /// Current y value of pointer
        /// </summary>
        private double currentY = 500;

       
       // private double APPROX_VALUE = 50;

        /// <summary>
        /// True if the related items pane is currently being displayed
        /// </summary>
        private bool relatedItemsDown = false;

        /// <summary>
        /// True if currently in AnnotationMode
        /// </summary>
        private bool inAnnotationMode = false;


        /// <summary>
        /// The presentation object that holds all the slides
        /// </summary>
        private static Presentation p;

        private BitmapImage blankImage;

        //private int POINTS_QUEUE_SIZE = 100;

        private bool videoPlaying;


        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow" /> class.
        /// </summary>
        public MainWindow()
        {

            InitializePresentation();
            picturePaths = CreatePicturePaths();
            videoPlaying = false;
            this.PreviousPicture = p.getPreviousSlide().getImage();
            this.Picture = p.getCurrentSlide().getImage();
            this.NextPicture = p.getNextSlide().getImage();
            this.ParentPicture = null;

            InitializeComponent();

            // Create the gesture recognizer.
            this.activeRecognizer = this.CreateRecognizer();
            recognitionEngine = new GestureRecognitionEngine();
            recognitionEngine.GestureRecognized += new EventHandler<GestureEventArgs>(recognitionEngine_GestureRecognized);


            // Wire-up window loaded event.
            Loaded += this.OnMainWindowLoaded;

            GetVideo(p.getCurrentSlide());
           
        }

        void RefreshRelated()
        {

            List<Slide> associated = p.getCurrentSlide().getAllAssociated();

            if (associated.Count > 0)
                this.RelatedPicture1 = associated[0].getImage();
            else
                this.RelatedPicture1 = blankImage;

            if (associated.Count > 1)
                this.RelatedPicture2 = associated[1].getImage();
            else
                this.RelatedPicture2 = blankImage;

            if (associated.Count > 2)
                this.RelatedPicture3 = associated[2].getImage();
            else
                this.RelatedPicture3 = blankImage;

            if (associated.Count > 3)
                this.RelatedPicture4 = associated[3].getImage();
            else
                this.RelatedPicture4 = blankImage;

            this.PropertyChanged(this, new PropertyChangedEventArgs("RelatedPicture1"));
            this.PropertyChanged(this, new PropertyChangedEventArgs("RelatedPicture2"));
            this.PropertyChanged(this, new PropertyChangedEventArgs("RelatedPicture3"));
            this.PropertyChanged(this, new PropertyChangedEventArgs("RelatedPicture4"));
            this.PropertyChanged(this, new PropertyChangedEventArgs("PlayButton"));
            canvas.Children.Clear();
        }

        void recognitionEngine_GestureRecognized(object sender, GestureEventArgs e)
        {

            String recognizedGesture = e.GestureType.ToString();

            switch (recognizedGesture) {

                case "PullDown":
                   
                    break;

                case "PushUp":

                    if (!relatedActivated)
                        break;
                    
                    var pushUpStoryboard = Resources["TopPushUp"] as Storyboard;

                    if (pushUpStoryboard != null)
                    {
                        pushUpStoryboard.Begin();
                    }
                    //watch.Restart();
                    relatedItemsDown = false;
                    relatedActivated = false;
                    relatedJustDeactivated = true;

                    break;

                case "SwipeLeft":

                    Debug.WriteLine("SWIPED LEFT :DDDDDDDDDDDDDDDDDDDDDD");

                    break;

                case "SwipeRight":

                    Debug.WriteLine("SWIPED RIGHT DDDDDDDDDDDDDDDDDDDDD:");

                    break;
                
                default:
                    break;

            }
        }

        /// <summary>
        /// Event implementing INotifyPropertyChanged interface.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets a value indicating whether no Kinect is currently connected.
        /// </summary>
        public bool IsDisconnected
        {
            get
            {
                return this.isDisconnectedField;
            }

            private set
            {
                if (this.isDisconnectedField != value)
                {
                    this.isDisconnectedField = value;

                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("IsDisconnected"));
                    }
                }
            }
        }

        /// <summary>
        /// Gets any message associated with a failure to connect.
        /// </summary>
        public string DisconnectedReason
        {
            get
            {
                return this.disconnectedReasonField;
            }

            private set
            {
                if (this.disconnectedReasonField != value)
                {
                    this.disconnectedReasonField = value;

                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("DisconnectedReason"));
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the index number of the image to be shown.
        /// </summary>
        public int Index
        {
            get
            {
                return this.indexField;
            }

            set
            {
                if (this.indexField != value)
                {
                    this.indexField = value;

                    // Notify world of change to Index and Picture.
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("Index"));
                    }
                }
            }
        }

        /// <summary>
        /// where to put the play button
        /// </summary>
        public double VideoHorizCenter { get; private set; }

        /// <summary>
        /// where to put the play button
        /// </summary>
        public double VideoVertCenter { get; private set; }

        /// <summary>
        /// where to put the play button overlay
        /// </summary>
        public double VideoVertOverlay{ get; private set; }

        /// <summary>
        /// where to put the play button overlay
        /// </summary>
        public double VideoHorizOverlay { get; private set; }

        /// <summary>
        /// Gets the video
        /// </summary>
        public BitmapImage PlayButton { get; private set; }

        /// <summary>
        /// Has the pause button Image
        /// </summary>
        public MediaElement myVideo { get; private set; }

        /// <summary>
        /// Gets the previous image displayed.
        /// </summary>
        public BitmapImage PreviousPicture { get; private set; }

        /// <summary>
        /// Gets the current image to be displayed.
        /// </summary>
        public BitmapImage Picture { get; private set; }

        /// <summary>
        /// Gets teh next image displayed.
        /// </summary>
        public BitmapImage NextPicture { get; private set; }


        /// <summary>
        /// Gets the current image to be displayed.
        /// </summary>
        public BitmapImage ParentPicture { get; private set; }

       
        /// <summary>
        /// Gets the first related image displayed.
        /// </summary>
        public BitmapImage RelatedPicture1 { get; private set; }

        /// <summary>
        /// Gets the second related image displayed.
        /// </summary>
        public BitmapImage RelatedPicture2 { get; private set; }

        /// <summary>
        /// Gets the third related image displayed.
        /// </summary>
        public BitmapImage RelatedPicture3 { get; private set; }

        /// <summary>
        /// Gets the fourth related image displayed.
        /// </summary>
        public BitmapImage RelatedPicture4 { get; private set; }

        /// <summary>
        /// Gets the fifth related image displayed.
        /// </summary>
        public BitmapImage RelatedPicture5 { get; private set; }


        /// <summary>
        /// Get list of files to display as pictures.
        /// </summary>
        /// <returns>Paths to pictures.</returns>
        private static string[] CreatePicturePaths()
        {

            var list = new List<string>();

            //Debug.WriteLine("count: " + p.retrieveSlides().Count);

            foreach (Slide s in p.retrieveSlides())
            {
                list.Add(s.getImagePath());
            }

            return list.ToArray();
        }

        /// <summary>
        /// Load the picture with the given index.
        /// </summary>
        /// <param name="index">The index to use.</param>
        /// <returns>Corresponding image.</returns>
        private BitmapImage LoadPicture(int index)
        {
            BitmapImage value;

            if (this.picturePaths.Length != 0)
            {
                var actualIndex = index % this.picturePaths.Length;
                if (actualIndex < 0)
                {
                    actualIndex += this.picturePaths.Length;
                }

                Debug.Assert(0 <= actualIndex, "Index used will be non-negative");
                Debug.Assert(actualIndex < this.picturePaths.Length, "Index is within bounds of path array");

                try
                {
                    value = new BitmapImage(new Uri(this.picturePaths[actualIndex]));
                }
                catch (NotSupportedException)
                {
                    value = null;
                }
            }
            else
            {
                value = null;
            }

            return value;
        }

        
        /// <summary>
        /// Create a wired-up recognizer for running the slideshow.
        /// </summary>
        /// <returns>The wired-up recognizer.</returns>
        private Recognizer CreateRecognizer()
        {
            // Instantiate a recognizer.
            var recognizer = new Recognizer();

           // Wire-up swipe right to manually advance picture.
            recognizer.SwipeRightDetected += (s, e) =>
              {
                  //System.Diagnostics.Debug.WriteLine("Right swipe detected");

                  if (e.Skeleton.TrackingId == nearestId)
                  {
                      Index++;

                      // Setup corresponding picture if pictures are available.
                      this.NextPicture = p.getNextSlide().getImage();
                      this.PreviousPicture = this.Picture;
                      this.Picture = this.NextPicture;
                      p.moveToNextSlide();    
                      GetVideo(p.getCurrentSlide());

                      RefreshRelated();
                      
                      // Notify world of change to Index and Picture.
                      if (this.PropertyChanged != null)
                      {
                          this.PropertyChanged(this, new PropertyChangedEventArgs("PreviousPicture"));
                          this.PropertyChanged(this, new PropertyChangedEventArgs("Picture"));
                          this.PropertyChanged(this, new PropertyChangedEventArgs("NextPicture"));
                      }

                      var storyboard = Resources["LeftAnimate"] as Storyboard;
                      if (storyboard != null)
                      {
                          storyboard.Begin();
                      }

                     // HighlightSkeleton(e.Skeleton);
                  }
              };

              // Wire-up swipe left to manually reverse picture.
              recognizer.SwipeLeftDetected += (s, e) =>
              {

                  //System.Diagnostics.Debug.WriteLine("Left swipe detected");

                  if (e.Skeleton.TrackingId == nearestId)
                  {
                      Index--;

                      // Setup corresponding picture if pictures are available.
                      this.NextPicture = this.Picture;
                      this.Picture = this.PreviousPicture;
                      p.moveToPreviousSlide();
                      this.PreviousPicture = p.getPreviousSlide().getImage();
                      GetVideo(p.getCurrentSlide());

                      RefreshRelated();

                      // Notify world of change to Index and Picture.
                      if (this.PropertyChanged != null)
                      {
                          this.PropertyChanged(this, new PropertyChangedEventArgs("PreviousPicture"));
                          this.PropertyChanged(this, new PropertyChangedEventArgs("Picture"));
                          this.PropertyChanged(this, new PropertyChangedEventArgs("NextPicture"));
                      }

                      var storyboard = Resources["RightAnimate"] as Storyboard;
                      if (storyboard != null)
                      {
                          storyboard.Begin();
                      }

                     // HighlightSkeleton(e.Skeleton);
                  }
              };
             

                return recognizer;
        }

        /// <summary>
        /// Handle insertion of Kinect sensor.
        /// </summary>
        private void InitializeNui()
        {
            this.UninitializeNui();

            var index = 0;
            while (this.nui == null && index < KinectSensor.KinectSensors.Count)
            {
                try
                {
                    this.nui = KinectSensor.KinectSensors[index];

                    this.nui.Start();

                    this.IsDisconnected = false;
                    this.DisconnectedReason = null;
                }
                catch (IOException ex)
                {
                    this.nui = null;

                    this.DisconnectedReason = ex.Message;
                }
                catch (InvalidOperationException ex)
                {
                    this.nui = null;

                    this.DisconnectedReason = ex.Message;
                }

                index++;
            }

            if (this.nui != null)
            {
                this.nui.SkeletonStream.Enable();

                this.nui.SkeletonFrameReady += this.OnSkeletonFrameReady;

                stopwatch = new Stopwatch();
                stopwatch.Start();
            }
        }

        /// <summary>
        /// Handle removal of Kinect sensor.
        /// </summary>
        private void UninitializeNui()
        {
            if (this.nui != null)
            {
                this.nui.SkeletonFrameReady -= this.OnSkeletonFrameReady;

                this.nui.Stop();

                this.nui = null;
            }

            this.IsDisconnected = true;
            this.DisconnectedReason = null;
        }


        private void InitializePresentation()
        {

            p = new Presentation();
            //this.CreateGraphics();
            string startupPath = Environment.CurrentDirectory;
            Debug.WriteLine(startupPath);
            Slide zero = new Slide(startupPath+"\\Pictures\\Slide0.jpg", "");
            Slide one = new Slide(startupPath + "\\Pictures\\blank3.jpg", startupPath + "\\Pictures\\Wildlife.wmv");
            Slide two = new Slide(startupPath+"\\Pictures\\Slide3.jpg", "");
            Slide three = new Slide(startupPath+"\\Pictures\\Slide4.jpg", "");
            Slide four = new Slide(startupPath+"\\Pictures\\Slide5.jpg", "");
            Slide five = new Slide(startupPath+"\\Pictures\\Slide6.jpg", "");
            Slide six = new Slide(startupPath+"\\Pictures\\Slide7.jpg", "");
            Slide seven = new Slide(startupPath+"\\Pictures\\Slide1.jpg", "");
            Slide eight = new Slide(startupPath+"\\Pictures\\Slide8.jpg", "");
            Slide nine = new Slide(startupPath+"\\Pictures\\Slide9.jpg", "");
            canvas = new Canvas();
            Canvas.SetLeft(canvas, 0);
            Canvas.SetTop(canvas, 0);
            this.PlayButton = new BitmapImage(new Uri(startupPath + "\\Pictures\\play_button.png"));
            blankImage = new BitmapImage(new Uri(startupPath + "\\Pictures\\Blank.jpg"));

            List<Slide> group0 = new List<Slide>()
            {
                seven
            };
            List<Slide> group1 = new List<Slide>()
            {
                //two,
                three,
                four,
                five,
            };
            List<Slide> group2 = new List<Slide>()
            {
                three,
                five,
                six,
            };
            List<Slide> group3 = new List<Slide>()
            {
                three,
                five,
                seven,
            };
            List<Slide> group4 = new List<Slide>()
            {
                three,
                four,
                six,
                seven,
            };
            List<Slide> group5 = new List<Slide>()
            {
                //two,
                three,
                four,
            };

            zero.addAssociatedSlides(group0);
            one.addAssociatedSlides(group1);
            two.addAssociatedSlides(group2);
            three.addAssociatedSlides(group5);
            four.addAssociatedSlides(group2);
            five.addAssociatedSlides(group4);
            six.addAssociatedSlides(group3);
            seven.addAssociatedSlides(group5);
            eight.addAssociatedSlides(group3);
            nine.addAssociatedSlides(group0);

            p.addSlide(zero);
            p.addSlide(one);
            p.addSlide(two);
            p.addSlide(three);
            p.addSlide(four);
            p.addSlide(five);
            p.addSlide(six);
            p.addSlide(seven);
            p.addSlide(eight);
            p.addSlide(nine);


        }

        /*
        static void DrawPixel(double x , double y)
        {
            int column = (int) x;
            int row = (int)y;

            // Reserve the back buffer for updates.
            writeableBitmap.Lock();

            unsafe
            {
                // Get a pointer to the back buffer. 
                int pBackBuffer = (int)writeableBitmap.BackBuffer;
                Debug.WriteLine("pback buffer is");
                Debug.WriteLine(pBackBuffer);
                // Find the address of the pixel to draw.
                pBackBuffer += row * writeableBitmap.BackBufferStride;
                pBackBuffer += column * 4;

                // Compute the pixel's color. 
                int color_data = 255 << 16; // R
                color_data |= 128 << 8;   // G
                color_data |= 255 << 0;   // B 

                // Assign the color data to the pixel.
                *((int*)pBackBuffer) = color_data;
            }

            // Specify the area of the bitmap that changed.
            writeableBitmap.AddDirtyRect(new Int32Rect(column, row, 1, 1));

            // Release the back buffer and make it available for display.
            writeableBitmap.Unlock();
        }
        */
        /// <summary>
        /// Window loaded actions to initialize Kinect handling.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {

            String path;
            try
            {
                path = Directory.GetCurrentDirectory();
                //Debug.WriteLine(path);
            }
            catch (Exception ef) { 
                Debug.WriteLine(ef);
            }

            // Start the Kinect system, this will cause StatusChanged events to be queued.
            this.InitializeNui();

            // Handle StatusChange events to pick the first sensor that connects.
            KinectSensor.KinectSensors.StatusChanged += (s, ee) =>
            {
                switch (ee.Status)
                {
                    case KinectStatus.Connected:
                        if (nui == null)
                        {
                            Debug.WriteLine("New Kinect connected");

                            InitializeNui();
                        }
                        else
                        {
                            Debug.WriteLine("Existing Kinect signalled connection");
                        }

                        break;
                    default:
                        if (ee.Sensor == nui)
                        {
                            Debug.WriteLine("Existing Kinect disconnected");

                            UninitializeNui();
                        }
                        else
                        {
                            Debug.WriteLine("Other Kinect event occurred");
                        }

                        break;
                }
            };
        }

        private void SetSizes()
        {
            /*VideoCanvas.Height = current.ActualHeight;
            VideoCanvas.Width = current.ActualWidth;
            HotCorners.Height = current.ActualHeight;
            HotCorners.Width = current.ActualWidth;
            PointerCanvas.Width = current.ActualWidth;
            PointerCanvas.Height = current.ActualHeight;
            annotateCanvas.Height = current.ActualHeight;
            annotateCanvas.Width = current.ActualWidth;*/
        }


        /// <summary>
        /// Handler for skeleton ready handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void OnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SetSizes();
            // Get the frame.
            using (var frame = e.OpenSkeletonFrame())
            {
                // Ensure we have a frame.
                if (frame != null)
                {
                    // Resize the skeletons array if a new size (normally only on first call).
                    if (this.skeletons.Length != frame.SkeletonArrayLength)
                    {
                        this.skeletons = new Skeleton[frame.SkeletonArrayLength];
                    }

                    // Get the skeletons.
                    frame.CopySkeletonDataTo(this.skeletons);

                    //Skeleton firstSkeleton = (this.skeletons)[0];    


                    Skeleton firstSkeleton = null;
                    foreach (Skeleton trackedSkeleton in this.skeletons)
                    {
                        if (trackedSkeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            firstSkeleton = trackedSkeleton;
                            break;
                        } 
                    }

                    if (firstSkeleton == null)
                    {
                        firstSkeleton = this.skeletons[0];
                    }

                    /*Skeleton firstSkeleton = (from trackskeleton in this.skeletons
                                              where trackskeleton.TrackingState == SkeletonTrackingState.Tracked
                                              select trackskeleton).FirstOrDefault();*/

                    // Hand tracking code
                    if (firstSkeleton.Joints[JointType.HandRight].TrackingState == JointTrackingState.Tracked)
                    {
                        this.MapJointsWithUIElement(firstSkeleton);
                    }

                    
                    // Assume no nearest skeleton and that the nearest skeleton is a long way away.
                    var newNearestId = -1;
                    var nearestDistance2 = double.MaxValue;

                    // Look through the skeletons.
                    foreach (var skeleton in this.skeletons)
                    {
                        // Only consider tracked skeletons.
                        if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            // Find the distance squared.
                            var distance2 = (skeleton.Position.X * skeleton.Position.X) +
                                (skeleton.Position.Y * skeleton.Position.Y) +
                                (skeleton.Position.Z * skeleton.Position.Z);

                            // Is the new distance squared closer than the nearest so far?
                            if (distance2 < nearestDistance2)
                            {
                                // Use the new values.
                                newNearestId = skeleton.TrackingId;
                                nearestDistance2 = distance2;
                            }
                        }
                    }

                    if (this.nearestId != newNearestId)
                    {
                        this.nearestId = newNearestId;
                    }
                    


                    // Pass skeletons to recognizer.
                    this.activeRecognizer.Recognize(sender, frame, this.skeletons);
                    recognitionEngine.Skeleton = firstSkeleton;
                    recognitionEngine.StartRecognize();

                    this.DrawStickMen(this.skeletons);
                }
            }
        }



        private double SLIDE_WIDTH = 220;//((Grid)(this.Content)).ActualWidth / 4;


        
        /// <summary>
        /// Uses the current y value to get the associated related item. 
        /// Then, switches to related item.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void SelectRelatedItem(double x, double y)
        {
                // assume that the related slides are in an array filling up a horizontal bar at the top of the screen

                //Debug.WriteLine("WINDOW: " + SLIDE_WIDTH);


                //Debug.Print("In select related item");


                //List<Slide> relatedSlides = new List<Slide>;
                //double SLIDE_PADDING
                //int numRelatedSlides // this is mapped to a "parent" slide
                //double WINDOW_WIDTH


                // given an x and y, return the slide at that position
                // for simplicity, let's assume there is no padding between the slides         
                int relatedSlideIndex =((int)(x) / (int)SLIDE_WIDTH);

                //if (selectedSlideIndex < 0) selectedSlideIndex = 9;
                //Slide selectedSlide = relatedSlides[selectedSlideIndex];

                //int newIndex = IndexFromXValue(selectedSlideIndex);

                //Debug.WriteLine("SELECT ITEM AT " + selectedSlideIndex);
                if (relatedSlideIndex < p.getCurrentSlide().getAllAssociated().Count && relatedActivated)
                {
                    int selectedSlideIndex = p.getCurrentSlide().getAllAssociated()[relatedSlideIndex].getIndex();
                    this.ParentPicture = this.Picture;
                    p.jumpToSlide(selectedSlideIndex);
                    this.PreviousPicture = p.getPreviousSlide().getImage();
                    this.Picture = p.getCurrentSlide().getImage();
                    this.NextPicture = p.getNextSlide().getImage();
                    RefreshRelated();
                    GetVideo(p.getCurrentSlide());

                    var removeRelatedItem = Resources["RemoveRelatedItem"] as Storyboard;
                    if (removeRelatedItem != null)
                    {
                        removeRelatedItem.Begin();
                    }

                    // Notify world of change to Index and Picture.
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("PreviousPicture"));
                        this.PropertyChanged(this, new PropertyChangedEventArgs("Picture"));
                        this.PropertyChanged(this, new PropertyChangedEventArgs("NextPicture"));

                        var pushUpStoryboard = Resources["TopPushUp"] as Storyboard;

                        if (pushUpStoryboard != null)
                        {
                            pushUpStoryboard.Begin();
                            relatedItemsDown = false;
                            relatedActivated = false;
                        }


                    }
                }
        }

        RotateTransform topRT = null;
        DoubleAnimation topDA = null;
        RotateTransform lowerRT = null;
        DoubleAnimation lowerDA = null;

        private void InitializeRT()
        {

            Debug.WriteLine("GOT HOT CORNER");
            Loader_UpperRight.Opacity = 1;
            Duration duration = new Duration(TimeSpan.FromSeconds(2));
            topDA = new DoubleAnimation(360, 180, duration);
            topRT = new RotateTransform();
            Loader_UpperRight.RenderTransform = topRT;
            Loader_UpperRight.RenderTransformOrigin = new Point(0.5, 0.5);

            Loader_LowerLeft.Opacity = 1;
            duration = new Duration(TimeSpan.FromSeconds(2));
            lowerDA = new DoubleAnimation(0, 180, duration);
            lowerRT = new RotateTransform();
            Loader_LowerLeft.RenderTransform = lowerRT;
            Loader_LowerLeft.RenderTransformOrigin = new Point(0.5, 0.5);
            /*topRight = Resources["RotateRect"] as Storyboard;
            topRight.Duration = duration;
            topRight.Children.Add(da);
            Storyboard.SetTarget(da, Loader_UpperRight);
            Storyboard.SetTargetProperty(da, new PropertyPath("(Image.RenderTransform).(RotateTransform.Angle)"));
            topRight.Begin();*/
        }



        private void animateSelection(double x, double y, SlideElem obj)
        {
            if (obj.Equals(SlideElem.upperRightHotCorner))
            {
                if (topRT == null)
                {
                    InitializeRT();
                }
                topRT.BeginAnimation(RotateTransform.AngleProperty, topDA);      
            }

            if (obj.Equals(SlideElem.lowerLeftHotCorner))
            {
                if (lowerRT == null)
                {
                    InitializeRT();
                }
                lowerRT.BeginAnimation(RotateTransform.AngleProperty, lowerDA);
            }

            //da.RepeatBehavior = RepeatBehavior.Forever;
            //rt.BeginAnimation(RotateTransform.AngleProperty, da);






            var animateRelatedItem = Resources["AnimateRelatedItem"] as Storyboard;
            var animatePlayButton = Resources["AnimatePlayButton"] as Storyboard;
            if (relatedItemsDown && y < RelatedItems.ActualHeight)
            {
                

                //double SLIDE_WIDTH = 220;
                int relatedSlideIndex = ((int)(x) / (int)SLIDE_WIDTH);
                if (relatedSlideIndex < p.getCurrentSlide().getAllAssociated().Count)  //TEMPORARY.... SHOULD BE < 5
                {
                    Canvas.SetLeft(relatedImageOverlay, SLIDE_WIDTH * relatedSlideIndex - 10 * (relatedSlideIndex));
                    if (animateRelatedItem != null)
                    {
                        animateRelatedItem.Begin(this, true);
                    }
                }
            }
            else if (p.getCurrentSlide().hasVideo() && isInPlayButton(x, y))
            {
                if (animatePlayButton != null)
                {
                 animatePlayButton.Begin(this, true);
                }
            }
            else
            {
                if (animatePlayButton != null)
                {
                    animateRelatedItem.Stop(this);
                    animatePlayButton.Stop(this);
                    animatePlayButton.FillBehavior = FillBehavior.Stop;
                }

            }
        }
          

        private bool isInPlayButton(double x, double y)
        {
            return ((x > (VideoCanvas.ActualWidth - PlayButton.Width) / 2.0) && (y > (VideoCanvas.ActualHeight - PlayButton.Height) / 2.0)
                && (x < (VideoCanvas.ActualWidth + PlayButton.Width) / 2.0) && (y < (768 + VideoCanvas.ActualHeight) / 2.0));
        }


        private void pauseVideo()
        {
            /*Debug.WriteLine("This slide actually has video");
            myVideoX.Pause();
            videoPlaying = false;
            playButton.Opacity = 1;*/
            playButtonOverlay.Opacity = 0;

        }

        private void playVideo()
        {         
            Debug.WriteLine("playing video");
            playButtonOverlay.Opacity = 0;
            playButton.Opacity = 0;
            myVideoX.Play();
            playButtonOverlay.Opacity = 0;
            videoPlaying = true;
            playButtonOverlay.Opacity = 0;           
        }



        /// <summary>
        /// Selects object at current x, y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void SelectObject(double x, double y)
        {
            Debug.WriteLine("In select object");
       
            if (relatedItemsDown && y < RelatedItems.ActualHeight)
            {
                SelectRelatedItem(x, y);
            }
            else if (isInPlayButton(x, y))
            {
                Debug.WriteLine("should start video now");
                if (p.getCurrentSlide().hasVideo())      
                {
                    if (videoPlaying)
                    {
                        pauseVideo();
                    }
                    else
                    {
                        playVideo();
                    }
                }

              
                //any other objects that could be selected (media, etc).
            }

            else if (currObj.Equals(SlideElem.upperRightHotCorner))
            {
                Debug.WriteLine("SELECTING TOP CORNER");
                topRT.BeginAnimation(RotateTransform.AngleProperty, null);
                Debug.WriteLine("STOPPED THE ANIMATION");
                justSelectedHotspot = true;
                HotCorner_UpperRight.Opacity = 0;
                LoadRelatedSlides();
            }
            else if (currObj.Equals(SlideElem.lowerLeftHotCorner))
            {
                Debug.WriteLine("SELECTING Bottom CORNER");
                lowerRT.BeginAnimation(RotateTransform.AngleProperty, null);
                justSelectedHotspot = true;
                HotCorner_LowerLeft.Opacity = 0;
                SetAnnotationMode();
                //LoadRelatedSlides();
            }
        }

        enum SlideElem {related1, related2, related3, related4, none, playButton, lowerLeftHotCorner, upperRightHotCorner }
        private SlideElem currObj = SlideElem.none;
        private bool justSelectedHotspot = false;

        //Returns true if the newest point is within APPROX_VALUE distance of the current x, y
        private bool SetNewSelectionPoint(double x, double y)
        {
            SlideElem newObj = SlideElem.none;
            if (p.getCurrentSlide().hasVideo() && isInPlayButton(x,y)) newObj = SlideElem.playButton;
            else if (relatedItemsDown && y < RelatedItems.ActualHeight && !inAnnotationMode) {
                newObj = (SlideElem)((int)(x) / (int)SLIDE_WIDTH);
            }
            else if (SetLowerHotspot(x, y)) {
                newObj = SlideElem.lowerLeftHotCorner;
            }
            else if (!inAnnotationMode && SetUpperHotspot(x, y))
            {
                newObj = SlideElem.upperRightHotCorner;
            }
            if (!currObj.Equals(newObj))
            {
                justSelectedHotspot = false;
                if(currObj.Equals(SlideElem.upperRightHotCorner)) {
                    topRT.BeginAnimation(RotateTransform.AngleProperty, null);     
                }
               else if(currObj.Equals(SlideElem.lowerLeftHotCorner)) {
                   lowerRT.BeginAnimation(RotateTransform.AngleProperty, null);     
                }
                currObj = newObj;
                animateSelection(x, y, currObj);
                return true;
            }
            return false;
        }



        /// <summary>
        /// Stuff.
        /// </summary>
        public Point lastPoint = new Point(600, 500);
        /// <summary>
        /// Stuff.
        /// </summary>
        public Point lastLastPoint = new Point(600, 500);

        private Point getCurrentPoint(Point newPoint)
        {
            if (lastLastPoint == null)
            {
                lastLastPoint = newPoint;
                return newPoint;
            }

            if (lastPoint == null)
            {
                lastPoint = newPoint;
                return newPoint;
            }


            double weightedX = lastLastPoint.X * 0.3 + lastPoint.X * 0.6 + newPoint.X * 0.1; // this seems to work nicely
            double weightedY = lastLastPoint.Y * 0.3 + lastPoint.Y * 0.6 + newPoint.Y * 0.1; // this seems to work nicely

            lastPoint = new Point(weightedX, weightedY);
            return lastPoint;
         
        }



        private void MapJointsWithUIElement(Skeleton skeleton)
        {

            Point hipPoint = this.ScalePosition(skeleton.Joints[JointType.ShoulderLeft].Position);
            Point leftHandPoint = this.ScalePosition(skeleton.Joints[JointType.HandLeft].Position);
            Point handPoint = this.ScalePosition(skeleton.Joints[JointType.HandRight].Position);
            Point elbowPoint = this.ScalePosition(skeleton.Joints[JointType.ElbowRight].Position);
            DepthImagePoint elbowDepthPoint = this.getDepthPoint(skeleton.Joints[JointType.ElbowRight].Position);
            DepthImagePoint handDepthPoint = this.getDepthPoint(skeleton.Joints[JointType.HandRight].Position);

            int elbowZ = elbowDepthPoint.Depth;
            double elbowX = elbowPoint.X;
            double elbowY = elbowPoint.Y;
          
            int handZ = handDepthPoint.Depth;
            double handX = handPoint.X;
            double handY = handPoint.Y;

            double deltaX = elbowZ * ((handX - elbowX) / (Math.Abs(handZ - elbowZ)+1));
            double deltaY = elbowZ * ((handY - elbowY) / (Math.Abs(handZ - elbowZ)+1));

            double newX = elbowX + deltaX;
            double newY = elbowY + deltaY;
            Point newPoint = new Point((int)newX, (int)newY);
            Line l = new Line();
            l.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
            l.X1 = lastPoint.X;
            l.Y1 = lastPoint.Y;
            Point currentPoint = getCurrentPoint(newPoint);
            l.X2 = currentPoint.X;
            l.Y2 = currentPoint.Y;
            l.StrokeThickness = 4;
            Canvas.SetLeft(RightHandPointer, currentPoint.X);
            Canvas.SetTop(RightHandPointer, currentPoint.Y);

            // INSERT ALL THE CODE HERE
     
               // SetLowerHotspot(currentPoint.X, currentPoint.Y);
                //SetUpperHotspot(currentPoint.X, currentPoint.Y);
            
                if (SetNewSelectionPoint(currentPoint.X, currentPoint.Y))
                {
                    currentX = currentPoint.X;
                    currentY = currentPoint.Y;
                    stopwatch.Restart();
                    Debug.WriteLine("NEW POINT: " + (int)currObj);
                }
                else if (stopwatch.ElapsedMilliseconds >= 2000 && !currObj.Equals(SlideElem.none))
                {
                    Debug.WriteLine("CLICK: " + (int)currObj);
                    SelectObject(currentPoint.X, currentPoint.Y);
                    stopwatch.Restart();

                }

               /* if (HotCorner_LowerLeft.Opacity > 0)
                {
                    Loader_LowerLeft.Opacity = 0.8;
                    //SetLowerLoader(stopwatch.ElapsedMilliseconds / 1000.0);
                }
                else
                {
                    Loader_LowerLeft.Opacity = 0;
                    if (lowerRT != null) lowerRT.BeginAnimation(RotateTransform.AngleProperty, null);

                }
                if (HotCorner_UpperRight.Opacity > 0)
                {
                    Loader_UpperRight.Opacity = 0.8;
                    //Debug.WriteLine("more");
                    //SetUpperLoader(stopwatch.ElapsedMilliseconds / 1000.0);
                }
                else
                {
                    //if (topRight != null) topRight.Stop();
                    if (topRT!=null) topRT.BeginAnimation(RotateTransform.AngleProperty, null);

                    //Debug.WriteLine("less");
                    Loader_UpperRight.Opacity = 0;
                }*/

           // DrawPixel(currentPoint.X, currentPoint.Y);
            Ellipse p = new Ellipse();
            //if (leftHandPoint.Y < hipPoint.Y)
            if(inAnnotationMode && leftHandPoint.Y < hipPoint.Y)
            {
                canvas.Children.Add(l);

            } 
            /*Debug.WriteLine(l.X1);
            Debug.WriteLine(l.X2);
            Debug.WriteLine(l.Y1);
            Debug.WriteLine(l.Y2);*/
        }

        private DepthImagePoint getDepthPoint(SkeletonPoint skeletonPoint)
        {
            DepthImagePoint depthPoint = this.nui.CoordinateMapper.MapSkeletonPointToDepthPoint(skeletonPoint, DepthImageFormat.Resolution640x480Fps30);
            return depthPoint;
        }

        private Point ScalePosition(SkeletonPoint skeletonPoint)
        {
   
            DepthImagePoint depthPoint = this.nui.CoordinateMapper.MapSkeletonPointToDepthPoint(skeletonPoint, DepthImageFormat.Resolution640x480Fps30);

            //return new Point(depthPoint.X, depthPoint.Y);
            return new Point(depthPoint.X * 1.4 * ((Grid)(this.Content)).ActualWidth / 640, depthPoint.Y * 1.4 * ((Grid)(this.Content)).ActualHeight / 480);

        }


        /// <summary>
        /// Select a skeleton to be highlighted.
        /// </summary>
        /// <param name="skeleton">The skeleton</param>
        private void HighlightSkeleton(Skeleton skeleton)
        {
            // Set the highlight time to be a short time from now.
            this.highlightTime = DateTime.UtcNow + TimeSpan.FromSeconds(0.5);

            // Record the ID of the skeleton.
            this.highlightId = skeleton.TrackingId;
        }

        /// <summary>
        /// Draw stick men for all the tracked skeletons.
        /// </summary>
        /// <param name="skeletons">The skeletons to draw.</param>
        private void DrawStickMen(Skeleton[] skeletons)
        {
            // Remove any previous skeletons.
            StickMen.Children.Clear();

            foreach (var skeleton in skeletons)
            {
                // Only draw tracked skeletons.
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    // Draw a background for the next pass.
                    this.DrawStickMan(skeleton, System.Windows.Media.Brushes.WhiteSmoke, 7);
                }
            }

            foreach (var skeleton in skeletons)
            {
                // Only draw tracked skeletons.
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    // Pick a brush, Red for a skeleton that has recently gestures, black for the nearest, gray otherwise.
                    System.Windows.Media.Brush brush = DateTime.UtcNow < this.highlightTime && skeleton.TrackingId == this.highlightId ? System.Windows.Media.Brushes.Red :
                        skeleton.TrackingId == this.nearestId ? System.Windows.Media.Brushes.Black : System.Windows.Media.Brushes.Gray;

                    // Draw the individual skeleton.
                    this.DrawStickMan(skeleton, brush, 3);
                }
            }
        }

        /// <summary>
        /// Draw an individual skeleton.
        /// </summary>
        /// <param name="skeleton">The skeleton to draw.</param>
        /// <param name="brush">The brush to use.</param>
        /// <param name="thickness">This thickness of the stroke.</param>
        private void DrawStickMan(Skeleton skeleton, System.Windows.Media.Brush brush, int thickness)
        {
            Debug.Assert(skeleton.TrackingState == SkeletonTrackingState.Tracked, "The skeleton is being tracked.");

            foreach (var run in SkeletonSegmentRuns)
            {
                var next = this.GetJointPoint(skeleton, run[0]);
                for (var i = 1; i < run.Length; i++)
                {
                    var prev = next;
                    next = this.GetJointPoint(skeleton, run[i]);

                    var line = new Line
                    {
                        Stroke = brush,
                        StrokeThickness = thickness,
                        X1 = prev.X,
                        Y1 = prev.Y,
                        X2 = next.X,
                        Y2 = next.Y,
                        StrokeEndLineCap = PenLineCap.Round,
                        StrokeStartLineCap = PenLineCap.Round
                    };

                    StickMen.Children.Add(line);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void GetVideo(Slide slide)
        {
            if (p.getCurrentSlide().hasVideo())
            {
                Debug.WriteLine("HAS VIDEO");
                Canvas.SetTop(playButton, (VideoCanvas.ActualHeight - playButton.ActualHeight) / 2);
                Canvas.SetLeft(playButton, (VideoCanvas.ActualWidth - playButton.ActualWidth) / 2);
                Canvas.SetTop(playButtonOverlay, Canvas.GetTop(playButton) + 30);
                Canvas.SetLeft(playButtonOverlay, Canvas.GetLeft(playButton) + 15);

                //Debug.WriteLine("HEIGHT: " + VideoCanvas.ActualHeight);
                myVideoX.Source = new Uri(slide.getVideoPath());
                myVideoX.Opacity = 1;
                playButton.Opacity = 1;
            }
            else if (!p.getCurrentSlide().hasVideo())
            {
                Debug.WriteLine("NO VIDEO");
                if (videoPlaying)
                {
                    myVideoX.Pause();
                    videoPlaying = false;
                }
                myVideoX.Opacity = 0;
                playButton.Opacity = 0;
            }
        }


        private bool SetLowerHotspot(double x, double y)
        {

            double hotspotX = 0;//Canvas.GetLeft(HotCorners);
            double hotspotY = window.ActualHeight;//800; //Canvas.GetBottom(HotCorners);

            double distance = Math.Sqrt(Math.Pow(hotspotX - x, 2) + Math.Pow(hotspotY - y, 2));

            if (distance > 220)
            {
                HotCorner_LowerLeft.Opacity = 0;
                Loader_LowerLeft.Opacity = 0;
                if (lowerRT != null && !justSelectedHotspot) lowerRT.BeginAnimation(RotateTransform.AngleProperty, null);
                return false;
            }

            else
            {
                if (!justSelectedHotspot)
                {
                    HotCorner_LowerLeft.Opacity = Math.Abs(1 - ((distance) / 250));
                    Loader_LowerLeft.Opacity = Math.Abs(1 - ((distance) / 250));
                }
                return true;
            }

        }


        private bool SetUpperHotspot(double x, double y)
        {
            double hotspotX = window.ActualWidth;//HotCorners.ActualWidth;//window.ActualWidth - current.Margin.Right;
            double hotspotY = 0;

            double distance = Math.Sqrt(Math.Pow(hotspotX - x, 2) + Math.Pow(hotspotY - y, 2));

            if (distance > 220)
            {
                HotCorner_UpperRight.Opacity = 0;
                Loader_UpperRight.Opacity = 0;
                if (topRT!=null && !justSelectedHotspot) topRT.BeginAnimation(RotateTransform.AngleProperty, null);
                return false;
            }

            else
            {
                if (!justSelectedHotspot)
                {
                    HotCorner_UpperRight.Opacity = Math.Abs(1 - ((distance) / 250));
                    Loader_UpperRight.Opacity = Math.Abs(1 - ((distance) / 250));
                }
                return true;
            }

        }


        private void SetLowerLoader(double time)
        {

            double scaledTime = 200 * time;


            Canvas.SetLeft(Loader_LowerLeft, scaledTime);

            if (time < 0.5)
                Canvas.SetBottom(Loader_LowerLeft, Math.Sqrt(Math.Pow(140,2) - Math.Pow(scaledTime, 2)));
            else
                Canvas.SetBottom(Loader_LowerLeft, 0.6 * Math.Sqrt(Math.Pow(140, 2) - Math.Pow(scaledTime, 2)));

            if (time > 1) {
                Loader_LowerLeft.Opacity = 0;
                SetAnnotationMode();
                //LoadRelatedSlides();
            }
        }

        private void SetUpperLoader(double time)
        {

            double scaledTime = 200 * time;


            Canvas.SetRight(Loader_UpperRight, scaledTime);

            if (time < 0.5)
                Canvas.SetTop(Loader_UpperRight, Math.Sqrt(Math.Pow(140, 2) - Math.Pow(scaledTime, 2)));
            else
                Canvas.SetTop(Loader_UpperRight, 0.6 * Math.Sqrt(Math.Pow(140, 2) - Math.Pow(scaledTime, 2)));

            if (time > 1)
            {
                Loader_UpperRight.Opacity = 0;
                LoadRelatedSlides();
            }
        }


        private void SetAnnotationMode()
        {
            if (inAnnotationMode)
            {
                inAnnotationMode = false;
                RightHandPointer.Fill = System.Windows.Media.Brushes.Green;
            }
            else
            {
                inAnnotationMode = true;
                RightHandPointer.Fill = System.Windows.Media.Brushes.LightSteelBlue;
            }
        }


        private void LoadRelatedSlides() {
            Debug.WriteLine("LOADING SLIDES");
            /*watch.Stop();
            if (watch.ElapsedMilliseconds > 2000)
            {
                if (relatedJustDeactivated)
                    relatedJustDeactivated = false;
            }*/
            if (relatedActivated) Debug.WriteLine("RELATED ACTIVATED");
            if (relatedActivated)
                return;

            if (relatedJustDeactivated)
            {
                relatedJustDeactivated = false;
                Debug.WriteLine("RELATED JUST ACTIVATED");
                //return;
            }

            RefreshRelated();

            var pullDownStoryboard = Resources["TopPullDown"] as Storyboard;

            if (pullDownStoryboard != null)
            {
                pullDownStoryboard.Begin();
            }

            relatedItemsDown = true;

            relatedActivated = true;



        }


        /// <summary>
        /// Convert skeleton joint to a point on the StickMen canvas.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <param name="jointType">The joint to project.</param>
        /// <returns>The projected point.</returns>
        private System.Drawing.Point GetJointPoint(Skeleton skeleton, JointType jointType)
        {
            var joint = skeleton.Joints[jointType];

            // Points are centered on the StickMen canvas and scaled according to its height allowing
            // approximately +/- 1.5m from center line.
            var point = new System.Drawing.Point
            {
                X = (int)((StickMen.Width / 2) + (StickMen.Height * joint.Position.X / 3)),
                Y = (int)((StickMen.Width / 2) - (StickMen.Height * joint.Position.Y / 3))
            };

            return point;
        }
    }
}
