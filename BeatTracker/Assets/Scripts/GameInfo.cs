using System;
using UnityEngine;

namespace BeatTracking
{
    public class GameInfo : MonoBehaviour
    {
        public static bool UseMicrophone {get; set;} = false;
        public static FileVisual SelectedFile {get; set;}
        public static bool HasBeenSet { get; set; } = false;
    }
}