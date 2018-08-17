using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using SharedLibrary;

namespace CMC_Library
{
	public enum DataStreamRecordType {
		Header,		// header identifying all DataStreams
		Command,	// bi-directional commands and responses, always ascii text
		Data,		// one-way flow of data, text and binary formats
		Timeout }	// marker for recording and playing back timeout exceptions

	public class DataStreamRecord
	{
		// Properties /////////////////////////////////////////////////

		public DataStreamRecordType RecordType { get; set; }
		// time elapsed since previous transfer
		public int Interval { get; set; }
		public int Length { get; set; }
		public byte[] Data { get; set; }

		#region // Constructors ///////////////////////////////////////////////

		public DataStreamRecord()
		{
		}

		public DataStreamRecord( BinaryReader reader )
		{
			Read( reader );
		}

		public DataStreamRecord( int interval, DataStreamRecordType type, byte[] data )
		{
			Interval = interval;
			RecordType = type;
			Length = data.Length;
			Data = data;
		}

		#endregion

		#region // Read and Write /////////////////////////////////////////////

		public void Write( BinaryWriter writer )
		{
			Debug.WriteLine( $"Writing: {this}" );
			writer.Write( (int)RecordType );
			writer.Write( Interval );
			writer.Write( Length );
			writer.Write( Data, 0, Length );
		}

		public void Read( BinaryReader reader )
		{
			var type = reader.ReadInt32();
			RecordType = (DataStreamRecordType)type;
			Interval = reader.ReadInt32();
			Length = reader.ReadInt32();
			Data = reader.ReadBytes( Length );
			Debug.WriteLine( $"Reading: {this}" );
		}

		#endregion

		#region // ToString and FormatData ////////////////////////////////////

		public override string ToString()
		{
			return $"{Interval}\t{RecordType}\t"
				+ FormatData();
		}

		public Encoding UTF8 = System.Text.Encoding.UTF8;

		public string FormatData()
		{
			StringBuilder sb = new StringBuilder();

			bool binary = Data.Any( b => b == 0 || ( b & 0x80 ) != 0 );
			if( binary )
			{
				var items = Data.Select( b => b.ToString( "x2" ) );
				string s = string.Join( ", ", items );
				sb.Append( $" [{Data.Length}] " );
				sb.Append( s );
				sb.Append( "  " );
			}

			sb.Append( UTF8.GetString( Data )
				.Visible() );

			return sb.ToString();
		}

		#endregion
	}
}
#if false
#endif
