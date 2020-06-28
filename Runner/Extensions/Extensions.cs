using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;

namespace Runner.Extensions
{
	public static class Extensions
	{
		public static void Output(this object obj)
		{
			var serializer = new SerializerBuilder().Build();
			var yaml = serializer.Serialize(obj);
			Console.WriteLine(yaml);
		}
	}
}
