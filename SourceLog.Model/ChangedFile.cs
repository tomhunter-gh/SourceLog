using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Windows.Documents;
using System.Windows.Markup;
using SourceLog.Interface;

namespace SourceLog.Model
{
	public class ChangedFile : IChangedFile
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

		[MaxLength]
		public string OldVersion { get; set; }
		[MaxLength]
		public string NewVersion { get; set; }

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
				var stopwatch = new Stopwatch();
				stopwatch.Start();
				RightFlowDocumentData = FlowDocumentToByteArray(value);
				stopwatch.Stop();
				Debug.WriteLine("    FlowDocumentToByteArray() took " + stopwatch.ElapsedMilliseconds + "ms");
			}
		}

		private FlowDocument _rightFlowDocument;

		private static byte[] FlowDocumentToByteArray(FlowDocument value)
		{
			
			var stream = new MemoryStream();
			XamlWriter.Save(value, stream);
			stream.Position = 0;
			return stream.ToArray();
		}

		private static FlowDocument GetFlowDocument(byte[] flowDocumentData)
		{
			FlowDocument flowDocument;
			try
			{
				var stream = new MemoryStream(flowDocumentData) { Position = 0 };
				flowDocument = (FlowDocument)XamlReader.Load(stream);
			}
			catch (Exception exception)
			{
				Debug.WriteLine(exception);
				flowDocument = new FlowDocument();
			}
			return flowDocument;
		}

		public double FirstModifiedLineVerticalOffset { get; set; }
	}

	//public class ChangeTypeWrapper
	//{
	//    private ChangeType _value;

	//    public string Value
	//    {
	//        get { return _value.ToString(); }
	//        set
	//        {
	//            _value = (ChangeType)Enum.Parse(typeof(ChangeType), value);
	//        }
	//    }

	//    public ChangeType EnumValue
	//    {
	//        get { return _value; }
	//        set { _value = value; }
	//    }

	//    public static implicit operator ChangeTypeWrapper(ChangeType changeType)
	//    {
	//        return new ChangeTypeWrapper { EnumValue = changeType };
	//    }

	//    public static implicit operator ChangeType(ChangeTypeWrapper changeTypeWrapper)
	//    {
	//        return changeTypeWrapper.EnumValue;
	//    }
	//}
}
