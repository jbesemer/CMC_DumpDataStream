using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using CMC_Library;
using SharedLibrary;

namespace DumpDataStream
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class DDS_MainWindow : Window
		, ILocationAndSize
	{
		protected DDS_MainViewModel MainVM;

		public DDS_MainWindow()
		{
			InitializeComponent();

			DataContext 
				= MainVM 
				= new DDS_MainViewModel();
		}

		private void Window_Initialized( object sender, EventArgs e )
		{
			// set new window layout before window is visible
			this.LoadLocationAndSize(
				Properties.Settings.Default.MainWinLocAndSize );
		}

		private void Window_Loaded( object sender, RoutedEventArgs e )
		{
			MainVM.SelectedPathname 
				= Properties.Settings.Default.SelectedPathname;
		}

		private void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e )
		{
			// save settings for next time
			Properties.Settings.Default.MainWinLocAndSize
				= this.SaveLocationAndSize();
			Properties.Settings.Default.SelectedPathname 
				= MainVM.SelectedPathname;

			Properties.Settings.Default.Save();
		}
	}
}
