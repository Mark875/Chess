   M S B A M L   `   `   `  ����@  <Chess, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:Chess.WindowsChess.Windows  5   	        Chess.Windows.SearchWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -localclr-namespace:Chess.Windows     5   	     � Title$  
Поиск��5   	   #��Z��5	   	   #��Z��5
   	    � WindowState$ 	Maximized=�5   	    � WindowStyle$
 None=�5   	    � 	Resources 5       �� 
TargetType   JustButtons �� Property �� Value �� 
TargetType��%E  &	        �� 5   
   " ��6   .�5      ��  �@�OverridesDefaultStyle$
 True��66   �� 5       � �� 5      �� 5      " ��6&   .R�5      � $��40q�6   
�������.+�5      �� 
���   �$��2q�69   �� 5      $��Center=�6/   $��Center=�6J   5      5      5      5      5      .��5      �  � RowDefinitions 5   
   �� 5      $��2* �6   �� 5       .+�5#   
   �� -   5        	 ��Name$	 	lblSearch��5#      $(��"Поиск противника...��6#   $��Center=�6A   $��Center=�5$      $��60(�6+   � 5%   
   $��1��6   
 � ColumnDefinitions
 5&      �� 5'      �� 5(      $��* �6#   �� 5)      .+�5+      �� -   5        $��1��5+      $��Отмена��6%   $��40(�66   #��[  6D   5,      5-      5.      5/      