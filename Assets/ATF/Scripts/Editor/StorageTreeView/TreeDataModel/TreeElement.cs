using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


namespace ATF.Storage
{

	[Serializable]
	public class TreeElement
	{
		[SerializeField] private int mId;
		[SerializeField] private string mName;
		
		[SerializeField] private int mDepth;
		[NonSerialized] private TreeElement MParent;
		[NonSerialized] private List<TreeElement> MChildren;

		public int Depth
		{
			get => mDepth;
			set => mDepth = value;
		}

		public TreeElement Parent
		{
			get => MParent;
			set => MParent = value;
		}

		public List<TreeElement> Children
		{
			get => MChildren;
			set => MChildren = value;
		}

		public bool HasChildren => Children != null && Children.Count > 0;

		public string Name
		{
			get => mName;
			set => mName = value;
		}

		public int Id
		{
			get => mId;
			set => mId = value;
		}

		public TreeElement ()
		{
		}

		public TreeElement (string name, int depth, int id)
		{
			mName = name;
			mId = id;
			mDepth = depth;
		}
	}

}


