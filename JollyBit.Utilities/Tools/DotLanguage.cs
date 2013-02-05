/*
Copyright (c) 2012 Richard Klafter

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
associated documentation files (the "Software"), to deal in the Softwarewithout restriction, 
including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial 
portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT 
NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES
OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using JollyBit.DataTypes;

namespace JollyBit.Tools.DotLanguage
{
	public class DotWriter : IDisposable
	{
		private readonly TextWriter _writer;
		private GraphType _graphType = GraphType.Graph;
		public bool PrettyPrint { get; set; }
		public int CurrentDepth { get; private set; }
		bool _hasWrittenIndent = false;
		bool _hasWrittenAttribute = false;
		bool _hasWrittenNode = false;
		private Line _line;
		public DotWriter(TextWriter writer)
		{
			_line = new Line(this);
			PrettyPrint = true;
			_writer = writer;
		}
		public void EndLine()
		{
			EndLine(true);
		}
		public void EndLine(bool lineBreak)
		{
			if (!_hasWrittenIndent) return; //if we have not written indent then we have not written anything
			if (_hasWrittenAttribute) _writer.Write("]");
			if (_hasWrittenAttribute || _hasWrittenNode) _writer.Write(";");
			if (lineBreak)
			{
				_writer.WriteLine();
				_hasWrittenIndent = false;
			}
			else
			{
				_writer.Write(" ");
			}
			_hasWrittenAttribute = false;
			_hasWrittenNode = false;
		}
		public void WriteNode(string name, params string[] ports)
		{
			WriteNode(name, (IEnumerable<string>)ports);
		}
		public void WriteNode(string name, IEnumerable<string> ports)
		{
			indent();
			if (_hasWrittenAttribute)
			{
				EndLine();
				WriteNode(name, ports);
			}
			else if (_hasWrittenNode)
			{
				_writer.Write(_graphType == GraphType.Graph ? " -- " : " -> ");
			}
			if (ports == null) ports = Enumerable.Empty<string>();
			_writer.Write(name + ports.Select(i => ":" + i).Aggregate("", (i, j) => i + j));
			//Set state vars
			_hasWrittenNode = true;
		}
		public void WriteAttribute(string name, string value)
		{
			indent();
			if (_hasWrittenAttribute || _hasWrittenNode)
			{
				_writer.Write(" ");
			}
			if (!_hasWrittenAttribute)
			{
				_writer.Write("[");
			}
			_writer.Write(string.Format(@"""{0}""=""{1}""", name.Replace(@"""", @"\"""), value.Replace(@"""", @"\""")));
			//Set state vars
			_hasWrittenNode = true;
			_hasWrittenAttribute = true;
		}
		public void WriteComment(string comment)
		{
			EndLine();
			indent();
			_writer.WriteLine(string.Format(@"//{0}", comment));
			EndLine();
		}

		public void WriteOpenGraph(GraphType graphType, string name)
		{
			if (_hasWrittenAttribute) EndLine();
			if (_hasWrittenNode) _writer.Write(_graphType == GraphType.Graph ? " -- " : " -> ");
			if (graphType != GraphType.Subgraph) _graphType = graphType;
			indent();
			if (string.IsNullOrEmpty(name))
				_writer.Write(string.Format(@"{0} {{", graphType.ToString()));
			else
				_writer.Write(string.Format(@"{0} ""{1}"" {{", graphType.ToString(), name));
			_hasWrittenAttribute = false;
			_hasWrittenNode = false;
			EndLine();
			CurrentDepth++;
		}

		public void WriteCloseGraph()
		{
			EndLine();
			CurrentDepth--;
			indent();
			_writer.Write("}");
			EndLine();
		}
		public void WriteGraphAttributeLine()
		{
			EndLine();
			indent();
			_writer.Write("Graph ");
			_hasWrittenNode = true;
			_hasWrittenAttribute = true;
		}
		public void WriteNodeAttributeLine()
		{
			EndLine();
			indent();
			_writer.Write("Node ");
			_hasWrittenNode = true;
			_hasWrittenAttribute = true;
		}
		public void WriteEdgeAttributeLine()
		{
			EndLine();
			indent();
			_writer.Write("Edge ");
			_hasWrittenNode = true;
			_hasWrittenAttribute = true;
		}
		public ISyntaxNodeOrAttribute StartLine()
		{
			EndLine();
			return _line;
		}
		public ISyntaxAttribute StartGraphAttributeLine()
		{
			WriteGraphAttributeLine();
			return StartLine();
		}
		public ISyntaxAttribute StartNodeAttributeLine()
		{
			WriteNodeAttributeLine();
			return StartLine();
		}
		public ISyntaxAttribute StartEdgeAttributeLine()
		{
			WriteEdgeAttributeLine();
			return StartLine();
		}

		private class Line : ISyntaxNodeOrAttribute
		{
			private readonly DotWriter _writer;
			public Line(DotWriter writer)
			{
				_writer = writer;
			}
			public void Add(INode node)
			{
				_writer.WriteNode(node.Name, node.Ports);
			}
			public void Add(IAttribute attribute)
			{
				_writer.WriteAttribute(attribute.Name, attribute.Value);
			}
			public DotWriter Writer
			{
				get { return _writer; }
			}
		}
		private void indent()
		{
			if (!_hasWrittenIndent && PrettyPrint)
			{
				for (int i = 0; i < CurrentDepth; i++)
					_writer.Write("    ");
			}
			_hasWrittenIndent = true;
		}
		public void Dispose()
		{
			_writer.Dispose();
		}
	}

	#region Supporting Types
	public interface INode
	{
		string Name { get; }
		IEnumerable<string> Ports { get; }
	}
	public interface IAttribute
	{
		string Name { get; }
		string Value { get; }
	}
	public interface ISyntaxBase
	{
		DotWriter Writer { get; }
	}
	public interface ISyntaxAttribute : ISyntaxBase
	{
		void Add(IAttribute attribute);
	}
	public interface ISyntaxNode : ISyntaxBase
	{
		void Add(INode node);
	}
	public interface ISyntaxNodeOrAttribute : ISyntaxAttribute, ISyntaxNode
	{
	}
	public class DotNode : INode
	{
		public DotNode() { }
		public DotNode(string name, IEnumerable<string> ports) { Name = name; Ports = new List<string>(ports); }
		public string Name { get; set; }
		public IList<string> Ports { get; set; }
		IEnumerable<string> INode.Ports { get { return Ports; } }
	}
	public class DotAttribute : IAttribute
	{
		public string Value { get; set; }
		public string Name { get; set; }
		public DotAttribute() { }
		public DotAttribute(string name, string value) { Name = name; Value = value; }
	}
	public enum GraphType
	{
		Graph,
		Digraph,
		Subgraph
	}
	#endregion

	public static class LineExt
	{
		public static ISyntaxAttribute Attribute(this ISyntaxAttribute line, string name, object value)
		{
			line.Add(new DotAttribute() { Name = name, Value = value.ToString() });
			return line;
		}
		public static ISyntaxNodeOrAttribute Node(this ISyntaxNodeOrAttribute line, string name, params string[] ports)
		{
			return Node(line, name, (IEnumerable<string>)ports);
		}
		public static ISyntaxNodeOrAttribute Node(this ISyntaxNodeOrAttribute line, string name, IEnumerable<string> ports)
		{
			if (string.IsNullOrEmpty(name))
			{
				line.Add(new DotNode() { Name = Guid.NewGuid().ToString().Replace("-", "") });
			}
			else
				line.Add(new DotNode() { Name = name });
			return line;
		}
		public static ISyntaxNodeOrAttribute End<T>(this T line) where T : ISyntaxAttribute
		{
			line.Writer.EndLine(false);
			return line as ISyntaxNodeOrAttribute;
		}
		public static ISyntaxNodeOrAttribute SubGraph(this ISyntaxNodeOrAttribute line)
		{
			return line.SubGraph(null);
		}
		public static ISyntaxNodeOrAttribute SubGraph(this ISyntaxNodeOrAttribute line, string name)
		{
			line.Writer.WriteOpenGraph(GraphType.Subgraph, name);
			return line;
		}
		public static ISyntaxNodeOrAttribute CloseSubGraph<T>(this T line) where T : ISyntaxAttribute
		{
			line.Writer.WriteCloseGraph();
			return line as ISyntaxNodeOrAttribute;
		}
	}
}
namespace JollyBit.Tools.DotLanguage.Graphviz
{
	public static class GraphvizLineExt
	{
		public static ISyntaxAttribute Label(this ISyntaxAttribute line, string label)
		{
			label = label == null ? "" : label;
			return line.Attribute("label", label);
		}
		public static ISyntaxAttribute Color(this ISyntaxAttribute line, string color)
		{
			return line.Attribute("color", color);
		}
		public static ISyntaxAttribute Pos(this ISyntaxAttribute line, double x, double y)
		{
			return line.Attribute("pos", string.Format("{0},{1}", x, y));
		}
		public static ISyntaxAttribute Width(this ISyntaxAttribute line, double width)
		{
			return line.Attribute("width", string.Format("{0}", width));
		}
		public static ISyntaxAttribute Height(this ISyntaxAttribute line, double height)
		{
			return line.Attribute("height", string.Format("{0}", height));
		}
		public static ISyntaxAttribute Size(this ISyntaxAttribute line, double width, double height)
		{
			return line.Width(width).Height(height);
		}
		public static ISyntaxAttribute Pin(this ISyntaxAttribute line, bool doPin)
		{
			return line.Attribute("pin", doPin.ToString());
		}
		public static ISyntaxAttribute Shape(this ISyntaxAttribute line, PolygonShape shape)
		{
			return line.Attribute("shape", shape.ToString().ToLower());
		}
		public static ISyntaxAttribute Style(this ISyntaxAttribute line, Style style)
		{
			return line.Attribute("style", style.ToString().ToLower());
		}
		public static ISyntaxAttribute Color(this ISyntaxAttribute line, Color color)
		{
			return line.Attribute("color", color.ToHtmlString());
		}
		public static ISyntaxAttribute FillColor(this ISyntaxAttribute line, Color color)
		{
			return line.Attribute("fillcolor", color.ToHtmlString());
		}
		public static ISyntaxAttribute FontSize(this ISyntaxAttribute line, int points)
		{
			return line.Attribute("fontsize", points.ToString());
		}
	}

	public enum Style
	{
		Dashed,
		Dotted,
		Solid,
		Invis,
		Bold,
		Tapered,
		Filled,
		Diagonals,
		Rounded
	}
	public enum PolygonShape
	{
		Box,
		Polygon,
		Ellipse,
		Oval,
		Circle,
		Point,
		Egg,
		Triangle,
		PlainText,
		Dimond,
		Trapezium,
		Parallelogram,
		House,
		Pentagon,
		Hexagon,
		Septagon,
		Octagon,
		DoubleCircle,
		DoubleOctagon,
		TripleOctagon,
		InvTriangle,
		InvTrapezium,
		InvHouse,
		MDiamond,
		MSquare,
		MCircle,
		Rect,
		Rectangle,
		Square,
		None,
		Note,
		Tab,
		Folder,
		Box3d,
		Component
	}
}