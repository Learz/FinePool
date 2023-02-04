using Godot;
using System;

namespace ExtensionMethods
{
    public static class NodeExtensions
        {
            public static Global GetGlobal(this Node node)
            {
                return node.GetNode<Global>("/root/Global");
            }
        }
}