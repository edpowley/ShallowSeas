using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShallowNet
{
	public abstract class Message
	{
		
	}

	public class TestMessage : Message
	{
		public string Text { get; set; }
	}
}
