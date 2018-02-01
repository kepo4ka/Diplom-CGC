<Query Kind="Statements">
  <Namespace>System.IO.Compression</Namespace>
</Query>

var fileInputName = @"D:\Cloudmail\Исходники\C#\Diplom-CGC\OneExeProgram_source\scripts\User_class.dll";
var assembly = File.ReadAllBytes( fileInputName  );

var fileOutputName = @"D:\Cloudmail\Исходники\C#\Diplom-CGC\OneExeProgram_source\scripts\User_class.dll.deflated";
using( var file = File.Open( fileOutputName, FileMode.Create ) )
using( var stream = new DeflateStream( file, CompressionMode.Compress ) )
using( var writer = new BinaryWriter( stream ) )
{
	writer.Write( assembly );
}