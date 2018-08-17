// #define PAUSE_READS_PER_TIMESTAMPS
// #define THROW_TIMEOUT_EXCEPTIONS

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace CMC_Library
{
	public class DataStreamReader : BinaryReader
		, IDisposable // via BinaryReader
	{
		// Properties /////////////////////////////////////////////////

		// track timing of data arrivals
		public Stopwatch Elapsed { get; protected set; }
		public int IntervalMax { get; protected set; }
		public TimeSpan ElapsedActual { get { return Elapsed.Elapsed; } }
		public TimeSpan ElapsedSimulated {
			get {
				return new TimeSpan( IntervalMax * TimeSpan.TicksPerMillisecond );
			}
		}

		public string Filename { get; protected set; }

		public DataStreamRecord Header { get; protected set; }

		public bool AtEOF { get { return BaseStream.Position >= BaseStream.Length; } }


		#region // Constructors ///////////////////////////////////////////////

		public DataStreamReader( Stream stream )
			: base( stream )
		{
			Elapsed = Stopwatch.StartNew();
		}

		public DataStreamReader( string filename )
			: this( new FileStream( filename, FileMode.Open ) )
		{
			Filename = filename;
			Header = ReadRecord();
		}

		#endregion

		#region // Generic Read Methods ///////////////////////////////////////

		public DataStreamRecord ReadRecord()    // TODO: what does BinaryReader.Read() do??
		{
			var data = new DataStreamRecord();
			data.Read( this );
			IntervalMax = Math.Max( IntervalMax, data.Interval );

#if PAUSE_READS_PER_TIMESTAMPS
			if( Stopwatch.ElapsedMilliseconds < data.Interval )
			{
				Thread.Sleep( (int)( Stopwatch.ElapsedMilliseconds - data.Interval ) );
			}
#endif

#if THROW_TIMEOUT_EXCEPTIONS // maybe should have Channel do this? Nah, needs to be done herein
			if( data.RecordType == DataStreamRecordType.Timeout )
			{
				throw new TimeoutException();
			}
#endif

			return data;
		}

		#endregion

		#region // Read Commmands vs Data ////////////////////////////////////

		public Encoding UTF8 = System.Text.Encoding.UTF8;

		protected string ReadString( DataStreamRecordType type )
		{
			var data = ReadRecord();

			Debug.Assert( data.RecordType == type );

			return UTF8.GetString( data.Data );
		}

		// Text Commands and Responses ////////////////////////////////

		public string ReadCommand()
		{
			return ReadString( DataStreamRecordType.Command );
		}

		public string ReadData()
		{
			return ReadString( DataStreamRecordType.Data );
		}

		public void ReadDataReset()
		{
			// TODO: think we can safely ignore this
		}

		public void WriteCommand( string command )
		{
			var text = ReadString( DataStreamRecordType.Command );
			while( text != command )
			{
				Debug.WriteLine(
					"DataStreamReader: skipping out of sequence command:"
					+ $" expected {text}"
					+ $" got {command}" );

				text = ReadString( DataStreamRecordType.Command );
			}
		}

		// Binary Data ////////////////////////////////////////////////

		public void ReadData( byte[] data, int offset, int count )
		{
			if( offset != 0 )	// TODO: seems we should be able to eliminate this use case
				Debug.WriteLine( $"DataStreamReader offset == {offset} != 0" );

			var record = ReadRecord();

			Debug.Assert( record.RecordType == DataStreamRecordType.Data );
			Debug.Assert( count == record.Length );

			Array.Copy( record.Data, offset, data, 0, count );
		}

		#endregion
	}
}
#if false
#endif
