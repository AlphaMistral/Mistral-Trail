/*
 ### Mistral Model ###
 Author: Jingping Yu
 RTX: joshuayu
 Created on: 2017/07/11
 */

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mistral.Utility.FileX
{
	/// <summary>
	/// Manipulates Files and Directories. 
	/// </summary>
	public static class FileManager 
	{
		#region Public Interfaces

		/// <summary>
		/// Use this Freaking Method wisely. 
		/// I don't take any responsibility in the consequences. 
		/// The same with rm -rf :)
		/// </summary>
		/// <param name="directory">Directory.</param>
		public static void DeleteEverythingInDirectory(string directory)
		{
			DirectoryInfo di = new DirectoryInfo(directory);
			foreach(FileInfo file in di.GetFiles())
			{
				file.Delete();
			}
			foreach (DirectoryInfo dir in di.GetDirectories())
			{
				dir.Delete(true);
			}
		}

		/// <summary>
		/// Deletes All the Files but ignore the sub-directories. 
		/// </summary>
		/// <param name="directory">Directory.</param>
		public static void DeleteFilesInDirectory(string directory)
		{
			DirectoryInfo di = new DirectoryInfo(directory);
			foreach (FileInfo file in di.GetFiles())
			{
				file.Delete();
			}
		}

		/// <summary>
		/// If the indicated directory does not exist. 
		/// Then creates the directory. 
		/// </summary>
		/// <param name="directory">Directory.</param>
		public static void TryCreateDirectory(string directory)
		{
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
		}

		#endregion
	}

}
