using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows.Documents;
using System.Windows.Markup;
using SourceLog.Interface;

namespace SourceLog.Model
{
	public class ChangedFile
	{
		public int ChangedFileId { get; set; }

		// EF 4.1 doesn't support Enums, so we need two fields
		public string ChangeTypeValue { get; set; }
		public ChangeType ChangeType
		{
			get { return (ChangeType)Enum.Parse(typeof(ChangeType), ChangeTypeValue); }
			set { ChangeTypeValue = value.ToString(); }
		}

		public string FileName { get; set; }

		public double FirstModifiedLineVerticalOffset { get; set; }

		private string _oldVersion;
		[NotMapped]
		public string OldVersion
		{
			get
			{
				return _oldVersion;
			}
			set
			{
				_oldVersion = CheckForBinary(value);
			}
		}

		private string _newVersion;
		[NotMapped]
		public string NewVersion
		{
			get
			{
				return _newVersion;
			}
			set
			{
				_newVersion = CheckForBinary(value);
			}
		}

		[Column(TypeName = "image")]
		[MaxLength]
		public byte[] LeftFlowDocumentData { get; set; }
		[NotMapped]
		public FlowDocument LeftFlowDocument
		{
			get
			{
				if (_leftFlowDocument == null)
				{
					_leftFlowDocument = GetFlowDocument(LeftFlowDocumentData);
				}
				return _leftFlowDocument;
			}
			set
			{
				//_leftFlowDocument = value; Can't set here because the thread setting won't be the UI thread so the UI thread won't be able to access the object
				LeftFlowDocumentData = FlowDocumentToByteArray(value);
			}
		}

		private FlowDocument _leftFlowDocument;

		[Column(TypeName = "image")]
		[MaxLength]
		public byte[] RightFlowDocumentData { get; set; }
		[NotMapped]
		public FlowDocument RightFlowDocument
		{
			get
			{
				if (_rightFlowDocument == null)
				{
					_rightFlowDocument = GetFlowDocument(RightFlowDocumentData);
				}
				return _rightFlowDocument;
			}
			set
			{
				//_rightFlowDocument = value;
				//var stopwatch = new Stopwatch();
				//stopwatch.Start();
				RightFlowDocumentData = FlowDocumentToByteArray(value);
				//stopwatch.Stop();
				//Debug.WriteLine("    FlowDocumentToByteArray() took " + stopwatch.ElapsedMilliseconds + "ms");
			}
		}
		private FlowDocument _rightFlowDocument;

		public ChangedFile() { }

		public ChangedFile(ChangedFileDto dto)
		{
			ChangeType = dto.ChangeType;
			FileName = dto.FileName;
			OldVersion = dto.OldVersion;
			NewVersion = dto.NewVersion;
		}

		private static byte[] FlowDocumentToByteArray(FlowDocument flowDocument)
		{
			using (var stream = new MemoryStream())
			{
				XamlWriter.Save(flowDocument, stream);
				stream.Position = 0;
				using (var compressedStream = new MemoryStream())
				{
					using (var gZipCompressor = new GZipStream(compressedStream, CompressionMode.Compress))
					{
						stream.CopyTo(gZipCompressor);
					}
					return compressedStream.ToArray();
				}
			}
		}

		private static FlowDocument GetFlowDocument(byte[] flowDocumentData)
		{
			FlowDocument flowDocument;
			try
			{
				using (var compressedStream = new MemoryStream(flowDocumentData))
				using (var uncompressedStream = new MemoryStream())
				{
					using (var gZipDecompressor = new GZipStream(compressedStream, CompressionMode.Decompress))
					{
						gZipDecompressor.CopyTo(uncompressedStream);
					}
					uncompressedStream.Position = 0;
					flowDocument = (FlowDocument)XamlReader.Load(uncompressedStream);
				}
			}
			catch (Exception exception)
			{
				Debug.WriteLine(exception);
				flowDocument = new FlowDocument();
			}
			return flowDocument;
		}


		private static string CheckForBinary(string s)
		{
			if (s.Contains("\0\0\0\0"))
			{
				Debug.WriteLine("    [Binary]");
				return "[Binary]";
			}
			return s;
		}
	}
}
