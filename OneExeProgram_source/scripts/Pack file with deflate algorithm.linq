<Query Kind="Statements">
  <Namespace>System.IO.Compression</Namespace>
</Query>


var fileInputName = @"c:\Users\FallenGameR\Documents\Visual Studio 2010\Projects\OneExeProgram\Libraries\AutoMapper 1.0 RTW\AutoMapper.dll";
var assembly = File.ReadAllBytes( fileInputName  );

var fileOutputName = @"c:\Users\FallenGameR\Documents\Visual Studio 2010\Projects\OneExeProgram\Libraries\AutoMapper 1.0 RTW\AutoMapper.dll.deflated";
using( var file = File.Open( fileOutputName, FileMode.Create ) )
using( var stream = new DeflateStream( file, CompressionMode.Compress ) )
using( var writer = new BinaryWriter( stream ) )
{
	writer.Write( assembly );
}