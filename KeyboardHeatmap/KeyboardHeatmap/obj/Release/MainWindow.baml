   M S B A M L   `   `   `  ���� J  FKeyboardHeatmap, Version=1.3.4.0, Culture=neutral, PublicKeyToken=null1clr-namespace:KeyboardHeatmapKeyboardHeatmap  G(clr-namespace:KeyboardHeatmap.PropertiesKeyboardHeatmap.Properties       KeyboardHeatmap.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     /localclr-namespace:KeyboardHeatmap     ?
Properties(clr-namespace:KeyboardHeatmap.Properties     -     �Name$  	Container�� � Title$ Keyboard Heatmap��$	��370��$	��780�� � 
ResizeMode$ CanMinimize=�.��� .+��� $��10,10,10,20q�.@��� -    ��Name$ Heatmap��$��Heatmap��.��� ���	#FFE5E5E5$��0,0,0,-1q�.+� � -     �Name$ layoutFrame��  � NavigationUIVisibility$ Hidden=�9� $��40,10,43,219q� 9� 
IsExpanded .   9� ExpandDirection$ Right=�.��� 
������� J� Effect 3  -System.Windows.Media.Effects.DropShadowEffect  .+��� -   	 ��Name$	 cbLayout��$��25��$��	20,10,0,0q�$��Center=�$
��Left=�
 �� 
IsReadOnly
 . �� -   $	 cbKeyCapture��$��25��$��
180,10,0,0q�$��Center=�$
��Left=�
 . �� -   $��Reset��$��Right=�$��Center=�$��20��$��
0,10,10,10q� J� RenderTransformOrigin$ 0.5,0.5&�$	��100���� -    ��Name$ 
lblWarning��$(��"You should not be able to see this��$��Center=�$��	40,10,0,0q�$	��Top=�$
��Bold"����	#FFB40000���	#FFC80000$y�Hidden=��� -   $
 Data��$��Table��.��� ���	#FFE5E5E5.+��� -	   $��Export to CSV��$
��Left=�$	��150��$��25��$	��Top=�$��5,0,0,0q��� -
   $��Reset��$
��Left=�$	��150��$��25��$	��Top=�$��	160,0,0,0q��� -    ��Name$ keyListView��$��0,25,0,0q� �� View �� .P���  �� Width$ 50��$��Id�� �� DisplayMemberBinding �� *Id+ �� Mode$ TwoWay=��� $	 200��$	��Key�� �� *	Character+$ TwoWay=��� $	 100��$��	Key Press�� �� *
NumPress+$ TwoWay=��� -   $ lblInfo��$��Keyboard Heatmap��$��20��$��Bottom=�$��Right=�$��0,0,20,0q�$��8(�