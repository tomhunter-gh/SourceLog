using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows.Documents;
using System.Windows.Markup;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using SourceLog.Interface;
using System.Linq;

namespace SourceLog.Model
{
	public class ChangedFile
	{
		public int ChangedFileId { get; set; }
		[Required]
		public LogEntry LogEntry { get; set; }

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
			//set
			//{
			//    _oldVersion = CheckForBinary(value);
			//}
		}

		[NotMapped]
		public byte[] OldVersionBytes { set { _oldVersion = CheckForBinary(value); } }

		private string _newVersion;
		[NotMapped]
		public string NewVersion
		{
			get
			{
				return _newVersion;
			}
			//set
			//{
			//    _newVersion = CheckForBinary(value);
			//}
		}

		[NotMapped]
		public byte[] NewVersionBytes { set { _newVersion = CheckForBinary(value); } }

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
				RightFlowDocumentData = FlowDocumentToByteArray(value);
			}
		}
		private FlowDocument _rightFlowDocument;

		public ChangedFile() { }

		public ChangedFile(ChangedFileDto dto)
		{
			ChangeType = dto.ChangeType;
			FileName = dto.FileName;
			OldVersionBytes = dto.OldVersion;
			NewVersionBytes = dto.NewVersion;
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
				Logger.Write(new Microsoft.Practices.EnterpriseLibrary.Logging.LogEntry
					{
						Message = "Error deserialising FlowDocument: " + exception,
						Severity = TraceEventType.Error
					});
				flowDocument = new FlowDocument();
			}
			return flowDocument;
		}

		private static string CheckForBinary(byte[] bytes)
		{
			if (BitConverter.ToString(bytes.Take(3).ToArray()) == "1F-8B-08")
				return "[GZIP archive file]";

			if (bytes.ContainsHorspool(new byte[] { 0, 0, 0, 0 }))// .Contains("\0\0\0\0"))
				return "[Binary]";

			// Getting OutOfMemoryExceptions for files around 50MB - limit to 10MB
			bool truncated = false;
			if (bytes.Length > 10485760) // 10MB
			{
				bytes = bytes.Take(10485760).ToArray();
				truncated = true;
			}

			// StreamReader 
			using (var memoryStream = new MemoryStream(bytes))
			{
				using (var streamReader = new StreamReader(memoryStream))
				{
					return streamReader.ReadToEnd()
						+ (truncated ? Environment.NewLine + "-- Truncated to 10MB" : String.Empty);
				}
			};
		}
	}

	static class Horspool
	{
		private static int[] BuildBadSkipArray(byte[] needle)
		{
			const int MAX_SIZE = 256;

			int[] skip = new int[MAX_SIZE];
			var needleLength = needle.Length;

			for (int c = 0; c < MAX_SIZE; c += 1)
			{
				skip[c] = needleLength;
			}

			var last = needleLength - 1;

			for (int scan = 0; scan < last; scan++)
			{
				skip[needle[scan]] = last - scan;
			}

			return skip;
		}

		public static bool ContainsHorspool(this byte[] haystack, byte[] needle)
		{
			var hlen = haystack.Length;
			var nlen = needle.Length;
			var badCharSkip = BuildBadSkipArray(needle);
			var last = nlen - 1;

			int offset = 0;
			int scan = nlen;

			while (offset + last < hlen)
			{

				for (scan = last; haystack[scan + offset] == needle[scan]; scan = scan - 1)
				{
					if (scan == 0)
					{
						return true;
					}
				}

				offset += badCharSkip[haystack[scan + offset]];

			}

			return false;
		}
	}
}
