using Godot;
using System;

public partial class AudioManager : Node
{
   private float masterVolume;
   public float MasterVolume
   { 
      get 
      { 
         return masterVolume; 
      } 
      set 
      { 
         AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), value); 
         masterVolume = value;
      } 
   }

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
   
   private float effectsVolume;
   public float EffectsVolume
   { 
      get 
      { 
         return effectsVolume; 
      } 
      set 
      { 
         AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Effects"), value); 
         effectsVolume = value;
      } 
   }
   
   private float ambienceVolume;
   public float AmbienceVolume
   { 
      get 
      { 
         return ambienceVolume; 
      } 
      set 
      { 
         AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Ambience"), value); 
         ambienceVolume = value;
      } 
   }
   
   private float uiVolume;
   public float UIVolume
   { 
      get 
      { 
         return uiVolume; 
      } 
      set 
      { 
         AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("UI"), value); 
         uiVolume = value;
      } 
   }
}
