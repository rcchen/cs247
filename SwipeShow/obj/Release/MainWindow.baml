   M S B A M L   `   `   `  ����   SlideshowGestures-WPF3    -Microsoft.Samples.Kinect.Slideshow.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     -     �Name$  window�� � Title$ 
Slide Show��$	��735��$	��850�� � 	Resources    MediumGreyBrush �� Color   BooleanToVisibilityConverter  LeftAnimate �� TargetProperty �� 
TargetName9  3System.Windows.Media.Animation.EasingDoubleKeyFrame O� KeyTime O� Value<  6System.Windows.Media.Animation.EasingThicknessKeyFrame p� KeyTime	 p� Value  RightAnimate  
TopAnimate  BottomAnimate  TopPullDown  	TopPushUp  AnimateRelatedItem1
 W� From	 W� To g� Duration 	 HideRelatedItem1 �� 
TargetType �� Property �� Value��%Y  &	        &	      &	      &	 S    &	 �    &	 �    &	 �    &	 �    &	 �    &		 e    '�� �    �� $ 	#ff6e6e6e���� �� .��U� $ (UIElement.Opacity)�$ previous��.\�  $ 0��$ 1Q�  $ 0:0:0.4��$ 1Q�  $ 0:0:0.5��$ 0Q�U� $ (UIElement.Opacity)�$
 next��.\�  $ 0��$ 0Q�r� $ (FrameworkElement.Margin)�$ current��.	�  $ 0��$	 2000,0,-2000,0q�  $ 0:0:0.5��$	 0q��� .��U� $ (UIElement.Opacity)�$ previous��.\�  $ 0��$ 0Q�U� $ (UIElement.Opacity)�$
 next��.\�  $ 0��$ 1Q�  $ 0:0:0.4��$ 1Q�  $ 0:0:0.5��$ 0Q�r� $ (FrameworkElement.Margin)�$ current��.	�  $ 0��$	 -2000,0,2000,0q�  $ 0:0:0.5��$	 0q��� .��U� $ (UIElement.Opacity)�$ previous��.\�  $ 0��$ 1Q�  $ 0:0:0.4��$ 1Q�  $ 0:0:0.5��$ 0Q�U� $ (UIElement.Opacity)�$
 next��.\�  $ 0��$ 0Q�r� $ (FrameworkElement.Margin)�$ current��.	�  $ 0��$	 2000,0,-2000,0q�  $ 0:0:0.5��$	 0q��� .��U� $ (UIElement.Opacity)�$ previous��.\�  $ 0��$ 1Q�  $ 0:0:0.4��$ 1Q�  $ 0:0:0.5��$ 0Q�U� $ (UIElement.Opacity)�$
 next��.\�  $ 0��$ 0Q�r� $ (FrameworkElement.Margin)�$ current��.	�  $ 0��$	 2000,0,-2000,0q�  $ 0:0:0.5��$	 0q��� .��U� $ (UIElement.Opacity)�$ RelatedItems��.\�  $ 0��$	 0.8Q�  $ 0:0:0.4��$	 0.8Q�r� $ (FrameworkElement.Margin)�$ RelatedItems��.	�  $ 0��$	 0,-500,0,500q�  $ 0:0:0.7��$	 0q��� .��U� $ (UIElement.Opacity)�$ RelatedItems��.\�  $ 0��$ 0Q�  $ 0:0:0.4��$ 0Q�r� $ (FrameworkElement.Margin)�$ RelatedItems��.	�  $ 0��$	 0,500,0,-500q�  $ 0:0:0.7��$	 0q��� .��W� $ relatedImageOverlay��$ Height�$
 0X�$	 200X�$ 0:0:2D�W� $ relatedImageOverlay��$ Opacity�$
 0X�$	 0.5X�$ 0:0:2D��� .��W� $ relatedImageOverlay��$ Opacity�$
 0X�$ 0X�$ 0:0:2D��� " ��.���  �@J�SnapsToDevicePixels . .��� $��	10 0 10 0q� � DataContext ��  �� ElementName$ window�� � RowDefinitions �� $
��Auto ��� $��* ��� $��20 ��� $
��Auto �.+�]� $��0��$��0 0 0 20q�� -   $��MainGrid��$��1��.+��� -    ��Name$
 next�� J� Opacity$ 0Q����� *NextPicture+�� -   $ previous��$ 0Q����� *PreviousPicture+�� -   $ current������ *	Picture+�� -   $ parent��$ 0Q����� *ParentPicture+�� -   $ RelatedItemsTab��$��C:\\Pictures\\tab.PNG��$	��100��$	��Top=�$��Right=��� -    ��Name$ RelatedItems��$ 0Q�$	��200��$	��Top=�$��Stretch=�
���   �.+��� -	   $ 	mainImage�� �� Left$ 0��$	��200��$	��200������ *RelatedPicture1+�� -
   $ 
mainImage2��$	 210��$	��200��$	��200������ *RelatedPicture2+�� -   $ 
mainImage3��$	 420��$	��200��$	��200������ *RelatedPicture3+�� -   $ 
mainImage4��$	 630��$	��200��$	��200������ *RelatedPicture4+�� -   $ relatedImageOverlay��$ 0��$	��200��$	��200��
�������$ 0Q��� -   $ StickMen��$	��200��$	��150��$��Right=�$��Bottom=��� $��No Kinect available��$��Center=�$��Bottom=�$��24(�
���  ��$
��Wrap=�$
��Bold"� �� TextAlignment$ Center=�y��� *IsDisconnected+ �� 	Converter# [ �� $��Center=�$	��Top=�$��24(�
���  ��$
��Wrap=�$
��Bold"�$ Center=����� *DisconnectedReason+�� -   $ PointerCanvas��.+�A� -   $��RightHandPointer��$ 0��
 �� Top$ 0��
���  ��$��15��$��15���� -   $��	statusBar��$��3��$��Stretch=�$��Bottom=�
�������#��[  .@��� $��0 0 0 10q�