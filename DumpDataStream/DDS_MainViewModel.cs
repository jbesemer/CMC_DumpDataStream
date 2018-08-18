using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;

using CMC_Library;
using SharedLibrary;
using SharedLibrary.MVVM;

namespace DumpDataStream
{
	public class DDS_MainViewModel : ViewModelBase
	{
		public const string DefaultFilename = "LegacyPower1.dat";

		#region // SelectedPathname & Related /////////////////////////////////

		private string selectedPathname;
		public string SelectedPathname
		{
			get { return selectedPathname; }
			set
			{
				if( OnPropertyChanged( ref selectedPathname, value ) )
				{
					if( string.IsNullOrEmpty( value ) )
					{
						SelectedPathname = Path.Combine(
							Environment.GetFolderPath( Environment.SpecialFolder.DesktopDirectory ),
							DefaultFilename );
					}
					else
					{
						SelectedFilename = Path.GetFileName( value );
						SelectedFolder = Path.GetDirectoryName( value );
						ReloadCommandCanExecute = File.Exists( value );
					}

					FileSize = new System.IO.FileInfo( SelectedPathname ).Length;
				}
			}
		}

		private string selectedFilename;
		public string SelectedFilename
		{
			get { return selectedFilename; }
			set { OnPropertyChanged( ref selectedFilename, value ); }
		}

		private string selectedFolder;
		public string SelectedFolder
		{
			get { return selectedFolder; }
			set { OnPropertyChanged( ref selectedFolder, value ); }
		}

		#endregion

		#region // DataItemSource & DataSelectedItem //////////////////////////

		private List<DataStreamRecord> dataItemSource;
		public List<DataStreamRecord> DataItemSource
		{
			get { return dataItemSource; }
			set
			{
				if( OnPropertyChanged( ref dataItemSource, value ) )
				{
					RecordCount = value.Count;
				}
			}
		}

		private DataStreamRecord dataSelectedItem;
		public DataStreamRecord DataSelectedItem
		{
			get { return dataSelectedItem; }
			set
			{
				if( OnPropertyChanged( ref dataSelectedItem, value ) )
				{
				}
			}
		}

		#endregion

		#region // Summary & related //////////////////////////////////////////

		public string Summary
		{
			get { return $"{SelectedFilename} v{FileVersion} {FileSize} bytes {RecordCount} rows"; }
		}

		public int recordCount;
		public int RecordCount
		{
			get { return recordCount; }
			set
			{
				if( OnPropertyChanged( ref recordCount, value ) )
				{
					OnPropertyChanged( "Summary" );
				}
			}
		}

		private DataStreamRecord fileHeader;
		public DataStreamRecord FileHeader
		{
			get { return fileHeader; }
			set
			{
				if( OnPropertyChanged( ref fileHeader, value ) )
				{
					FileVersion = value.Data[ 0 ].ToString();
					OnPropertyChanged( "Summary" );
				}
			}
		}

		public string fileVersion;
		public string FileVersion
		{
			get { return fileVersion; }
			set
			{
				if( OnPropertyChanged( ref fileVersion, value ) )
				{
					OnPropertyChanged( "Summary" );
				}
			}
		}

		public long fileSize;
		public long FileSize
		{
			get { return fileSize; }
			set
			{
				if( OnPropertyChanged( ref fileSize, value ) )
				{
					OnPropertyChanged( "Summary" );
				}
			}
		}

		#endregion

		// Commands ///////////////////////////////////////////////////

		#region // Browse Command /////////////////////////////////////////////

		private ICommand browseCommand;
		public ICommand BrowseCommand
		{
			get
			{
				return browseCommand
					?? ( browseCommand = new RelayCommand( Browse, () => BrowseCommandCanExecute ) );
			}
		}

		private bool browseCommandCanExecute = true;
		public bool BrowseCommandCanExecute
		{
			get { return browseCommandCanExecute; }
			set { OnPropertyChanged( ref browseCommandCanExecute, value ); }
		}

		public void Browse()
		{
			var Pathname = SelectedPathname;

			OpenFileDialog dlg = new OpenFileDialog()
			{
				Title = "Open Data Stream File",
				InitialDirectory = System.IO.Path.GetDirectoryName( Pathname ),
				CheckPathExists = true,
				FileName = Pathname,
				CheckFileExists = true,
				Multiselect = false,
				Filter = ImportExportFilter,
				FilterIndex = FilterIndex,
				DefaultExt = DefaultExtension,
				AddExtension = true,
			};

			if( dlg.ShowDialog().Value == true )
			{
				SelectedPathname = dlg.FileName;
				FilterIndex = dlg.FilterIndex;
				Reload();
			}
		}

		int FilterIndex = 1;
		string DefaultExtension = ".dat";

		public const string ImportExportFilter
			= "Data Files (*.dat)|*.dat"
				// + "|Tab-Separated Files (*.tsv)|*.tsv"
				// + "|Tab-Separated Files (*.txt)|*.txt"
				+ "|All Files (*.*)|*.*";

		#endregion

		#region // Reload Command /////////////////////////////////////////////

		private ICommand reloadCommand;
		public ICommand ReloadCommand
		{
			get
			{
				return reloadCommand
					?? ( reloadCommand = new RelayCommand( Reload, () => ReloadCommandCanExecute ) );
			}
		}

		private bool reloadCommandCanExecute = true;
		public bool ReloadCommandCanExecute
		{
			get { return reloadCommandCanExecute; }
			set { OnPropertyChanged( ref reloadCommandCanExecute, value ); }
		}

		public void Reload()
		{
			using( var reader = new DataStreamReader( SelectedPathname ) )
			{
				FileHeader = reader.Header;
				var data = new List<DataStreamRecord>();
				while( !reader.AtEOF )
				{
					try
					{
						data.Add( reader.ReadRecord() );
					}
					catch( Exception ex )
					{
						Debug.WriteLine( $"Reload Exception: {ex.Message}" );
						break;
					}
				}
				DataItemSource = data;
			}
		}

		#endregion

		#region // Save Report To File ////////////////////////////////////////

		private ICommand saveReportToFileCommand;
		public ICommand SaveReportToFileCommand
		{
			get
			{
				return saveReportToFileCommand
					?? ( saveReportToFileCommand
						= new RelayCommand( SaveReportToFile,
							() => SaveReportToFileCommandCanExecute ) );
			}
		}

		private bool saveReportToFileCommandCanExecute = true;
		public bool SaveReportToFileCommandCanExecute
		{
			get { return saveReportToFileCommandCanExecute; }
			set { OnPropertyChanged( ref saveReportToFileCommandCanExecute, value ); }
		}

		public void SaveReportToFile()
		{
			#region // Show the Dialog ////////////////////////////////////////

			const string DefaultReportExtension = ".lst";
			string folder = Path.GetDirectoryName( SelectedPathname );
			string filename = Path.Combine(
				folder,
				Path.GetFileNameWithoutExtension( SelectedFilename )
					+ DefaultReportExtension );

			var dlg = new SaveFileDialog()
			{
				Title = "Dump Data to File",
				InitialDirectory = folder,
				FileName = filename,
				FilterIndex = FilterIndex,
				CheckPathExists = true,
				AddExtension = true,
				Filter = SaveReportFilters,
				DefaultExt = DefaultReportExtension,
			};

			#endregion

			if( dlg.ShowDialog() != true )
				return;

			FilterIndex = dlg.FilterIndex;

			#region // Perform export /////////////////////////////////////
			
			Reload();	// shouldn't be necessary but probably doesn't hurt

			using( var writer = File.CreateText( dlg.FileName ) )
			{
				writer.WriteLine( FileHeader );

				foreach( var datum in DataItemSource )
				{
					writer.WriteLine( datum );
				}
			}

			#endregion
		}

		public const string SaveReportFilters
			= "Report Files (*.lst)|*.lst"
				// + "|Tab-Separated Files (*.tsv)|*.tsv"
				// + "|Tab-Separated Files (*.txt)|*.txt"
				+ "|All Files (*.*)|*.*";


		#endregion
	}
}
