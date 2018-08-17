using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CMC_Library
{
	public class DataStreamWriter : BinaryWriter
		, IDisposable // via BinaryWriter
	{
		// Properties /////////////////////////////////////////////////
		
		// track timing of data arrivals
		public Stopwatch Stopwatch { get; protected set; }

		public string Filename { get; protected set; }

		#region // Constructors ///////////////////////////////////////////////

		public DataStreamWriter( Stream stream )
			: base( stream )
		{
			Stopwatch = Stopwatch.StartNew();
		}

		public DataStreamWriter( string filename )
			: this( new FileStream( filename, FileMode.Create ) )
		{
			Filename = filename;

			var header = new DataStreamRecord()
			{
				RecordType = DataStreamRecordType.Header,
				Length = 1,
				Data = new byte[] { 0 }, // version
			};

			Write( header );
		}

		#endregion

		#region // Generic Write Methods //////////////////////////////////////

		public void Write( DataStreamRecord record )
		{
			record.Write( this );
		}

		public void Write( int interval, DataStreamRecordType type, byte[] data )
		{
			var record = new DataStreamRecord( interval, type, data );
			Write( record );
		}

		public void Write( DataStreamRecordType type, byte[] data )
		{
			Write( (int)Stopwatch.ElapsedMilliseconds, type, data );
		}

		public void Write( DataStreamRecordType type, int offset, byte[] source, int count )
		{
			byte[] data = new byte[ count ];
			Array.Copy( source, offset, data, 0, count );
			Write( type, data );
		}

		public void Write( DataStreamRecordType type, int offset, byte[] source )
		{
			int count = source.Length - offset;
			byte[] data = new byte[ count ];
			Array.Copy( source, offset, data, 0, count );
			Write( type, data );
		}

		#endregion

		#region // Write Commmands vs Data ////////////////////////////////////

		public Encoding UTF8 = System.Text.Encoding.UTF8;

		public void WriteCommand( string command )
		{
			Write( DataStreamRecordType.Command, UTF8.GetBytes( command ) );
		}

		public void WriteData( string data )
		{
			Write( DataStreamRecordType.Data, UTF8.GetBytes( data ) );
		}

		public void WriteData( byte[] data, int offset, int count )
		{
			Write( DataStreamRecordType.Data, offset, data, count );
		}

		#endregion
	}
}
