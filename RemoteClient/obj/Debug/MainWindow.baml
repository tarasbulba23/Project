   M S B A M L   `   `   `  ����G  CRemoteClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:RemoteClientRemoteClient  5   	       RemoteClient.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ,localclr-namespace:RemoteClient     5   	   -   5          � Title$  RemoteClient��5   	   $	��150��6   $	��250��6+   $	��250��67   $	��150��6F   $	��250��6V   $	��150��6e   $��LeftToRight=�6u    � WindowState$ Normal=�6�    � 
ResizeMode$ NoResize=�6�    � ShowInTaskbar .  � WindowStyle$ SingleBorderWindow=�6�   $y�Visible=�6�   .��5	      � $y�Visible=�6   $��Disabled=�6     � RowDefinitions 5
   
   �� 5      �� 5      �� 5       � ColumnDefinitions 5   
   �� 5      �� 5      $
��0,2* �6   .+�5   
   �� $��0��6   $��0��6    $��Set IP��6-   �� 5   
   $��0��6   $��1��6    $��Set Quality��6-   �� 5   
   -   5         ��Name$ ip��5      $��1��6   $��0��6.   $��	127.0.0.1��6;   �� 5   
   -   5         ��Name$ comboBox��5      $��1��6%   $��1��65   .@�5      �� .��6   Low6    �� 5      .��6   Medium6#   �� 5      	 �� 
IsSelected	 . .��6-   Hight64   5      �� 5   
   -   5        
 ��Name$
 submit��5      $��0��6!   $��3��61   $��2��6>   $��Submit��6R   6c   5      5      