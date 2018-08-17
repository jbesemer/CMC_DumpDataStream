using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using SharedLibrary;

namespace DumpDataStream
{
	[ValueConversion( typeof( byte[] ), typeof( string ) )]
	public class DataConverter : OneWayConverter, IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if( targetType != typeof( string ) )
				throw new InvalidOperationException( "The target must be a string" );

			if( value == null )
				return "";

			if( value is byte[] data )
			{
				return FormatData( data );
			}

			throw new InvalidOperationException( "The source must be a byte[] " );
		}

		public static string FormatData( byte[] data )
		{
			var UTF8 = System.Text.Encoding.UTF8;
			StringBuilder sb = new StringBuilder();

			bool binary = data.Any( b => b == 0 || ( b & 0x80 ) != 0 );
			if( binary )
			{
				var items = data.Select( b => b.ToString( "x2" ) );
				string s = string.Join( ", ", items );
				sb.Append( $" [{data.Length}] " );
				sb.Append( s );
				sb.Append( "  " );
			}

			sb.Append( UTF8.GetString( data )
				.Visible() );

			return sb.ToString();
		}
	}
}
