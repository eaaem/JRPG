using Godot;
using System;

public partial class AudioManager : Node
{
   public float MasterVolume { get; set; }
   private float musicVolume;
   public float MusicVolume
   { 
      get 
      { 
         return musicVolume; 
      } 
      set 
      { 
         AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), value); 
         musicVolume = value;
      } 
   }
   
   public float EffectsVolume { get; set; }
   public float AmbientVolume { get; set; }
   public float UIVolume { get; set; }
}
