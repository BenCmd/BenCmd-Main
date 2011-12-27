using System;
using System.Collections.Generic;

namespace BScript
{
	public class VariableController
	{
		public Dictionary<string, VariableNode> nodes;
		
		public void SetVariable(string name, bool create, object value)
		{
			string[] nodes = name.Split('+');
			for (int i = 0; i < nodes.Length; i++)
			{
				nodes[i] = nodes[i].Trim();
			}
			if (!this.nodes.ContainsKey(nodes[0]))
			{
				if (create) this.nodes.Add(nodes[0], new VariableNode(nodes[0]));
				else throw new NullReferenceException("Uncreated variable!");
			}
			VariableNode node = this.nodes[nodes[0]];
			for (int i = 1; i < nodes.Length - 1; i++)
			{
				if (!node.nodes.ContainsKey(nodes[i]))
				{
					if (create) node.nodes.Add(nodes[i], new VariableNode(nodes[0]));
					else throw new NullReferenceException("Uncreated variable!");
				}
				node = node.nodes[nodes[i]];
			}
			if (!node.variables.ContainsKey(nodes[nodes.Length - 1]))
			{
				if (create) node.variables.Add(nodes[nodes.Length - 1], "");
				else throw new NullReferenceException("Uncreated variable!");
			}
			node.variables[nodes[nodes.Length - 1]] = value;
		}
		
		public object GetVariable(string name)
		{
			string[] nodes = name.Split('+');
			for (int i = 0; i < nodes.Length; i++)
			{
				nodes[i] = nodes[i].Trim();
			}
			if (!this.nodes.ContainsKey(nodes[0]))
			{
				return null;
			}
			VariableNode node = this.nodes[nodes[0]];
			for (int i = 1; i < nodes.Length - 1; i++)
			{
				if (!node.nodes.ContainsKey(nodes[i]))
				{
					return null;
				}
				node = node.nodes[nodes[i]];
			}
			if (!node.variables.ContainsKey(nodes[nodes.Length - 1]))
			{
				return null;
			}
			return node.variables[nodes[nodes.Length - 1]];
		}
	}
	
	public struct VariableNode
	{
		public Dictionary<string, object> variables;
		public Dictionary<string, VariableNode> nodes;
		public string name;
		
		public VariableNode(string name)
		{
			this.variables = new Dictionary<string, object>();
			this.nodes = new Dictionary<string, VariableNode>();
			this.name = name;
		}
	}
}

