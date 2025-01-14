﻿using System;
using System.IO;
using Newtonsoft.Json;

namespace SDG.Framework.IO.Serialization
{
	public class JSONSerializer : ISerializer
	{
		public void serialize<T>(T instance, byte[] data, int index, out int size, bool isFormatted)
		{
			MemoryStream memoryStream = new MemoryStream(data, index, data.Length - index);
			this.serialize<T>(instance, memoryStream, isFormatted);
			size = (int)memoryStream.Position;
			memoryStream.Close();
			memoryStream.Dispose();
		}

		public void serialize<T>(T instance, MemoryStream memoryStream, bool isFormatted)
		{
			StreamWriter streamWriter = new StreamWriter(memoryStream);
			JsonWriter jsonWriter = (!isFormatted) ? new JsonTextWriterFormatted(streamWriter) : new JsonTextWriter(streamWriter);
			JsonSerializer jsonSerializer = new JsonSerializer();
			try
			{
				jsonSerializer.Serialize(jsonWriter, instance);
				jsonWriter.Flush();
			}
			finally
			{
				jsonWriter.Close();
				streamWriter.Close();
				streamWriter.Dispose();
			}
		}

		public void serialize<T>(T instance, string path, bool isFormatted)
		{
			StreamWriter streamWriter = new StreamWriter(path);
			JsonWriter jsonWriter = new JsonTextWriterFormatted(streamWriter);
			JsonSerializer jsonSerializer = new JsonSerializer();
			jsonSerializer.Formatting = ((!isFormatted) ? 0 : 1);
			try
			{
				jsonSerializer.Serialize(jsonWriter, instance);
				jsonWriter.Flush();
			}
			finally
			{
				jsonWriter.Close();
				streamWriter.Close();
				streamWriter.Dispose();
			}
		}
	}
}
