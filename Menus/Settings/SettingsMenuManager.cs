using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;

public partial class SettingsMenuManager : Panel
{
   private int activeTab = 0;

   [Export]
   private ManagerReferenceHolder managers;

   [Export]
   Panel[] panels;
   [Export]
   Button[] tabs;

   Panel videoPanel;
   Panel controlsPanel;

   Button videoButton;
   Button controlsButton;

   private CharacterController characterController;

   Godot.Collections.Dictionary<string, Vector2I> resolutions = new Godot.Collections.Dictionary<string, Vector2I>
   {
      { "1920x1080", new Vector2I(1920, 1080) },
      { "1280x720", new Vector2I(1280, 720) },
      { "800x600", new Vector2I(800, 600) }
   };

   private Vector2I currentResolution;

   private ConfigFile configFile;

   public override void _Ready()
   {
      characterController = GetNode<CharacterController>("/root/BaseNode/PartyMembers/Member1");

      videoPanel = GetNode<Panel>("Video");
      controlsPanel = GetNode<Panel>("Controls");

      videoButton = GetNode<Button>("ButtonsContainer/Video");
      controlsButton = GetNode<Button>("ButtonsContainer/Controls");

      InitializeResolutions();

      configFile = new ConfigFile();
      Error error = configFile.Load("user://settings.cfg");

      // There is no config file, we need to make one
      if (error != Error.Ok)
      {
         configFile.SetValue("video", "windowed_mode", 4);
         configFile.SetValue("video", "resolution", "1920x1080");
         return;
      }

      InitializeSettings();
   }

   void InitializeSettings()
   {
      foreach (string section in configFile.GetSections())
      {
         if (section == "video")
         {
            OnSelectWindowedMode((int)configFile.GetValue(section, "windowed_mode"));
            currentResolution = resolutions[(string)configFile.GetValue(section, "resolution")];
            GetWindow().Size = currentResolution;

            OptionButton resolutionDropdown = videoPanel.GetNode<OptionButton>("Resolution/ResolutionButton");

            for (int i = 0; i < resolutionDropdown.ItemCount; i++)
            {
               if (resolutionDropdown.GetItemText(i) == (string)configFile.GetValue(section, "resolution"))
               {
                  resolutionDropdown.Select(i);
               }
            }

            OptionButton windowedDropdown = videoPanel.GetNode<OptionButton>("Resolution/WindowButton");

            if ((int)configFile.GetValue(section, "windowed_mode") == 0)
            {
               windowedDropdown.Select(0);
            }
            else if ((int)configFile.GetValue(section, "windowed_mode") == 1)
            {
               windowedDropdown.Select(1);
            }
            else
            {
               windowedDropdown.Select(2);
            }
         }
         else if (section == "controls")
         {
            characterController.HorizontalSensitivity = (float)configFile.GetValue(section, "sensitivity");

            controlsPanel.GetNode<SpinBox>("Sensitivity/SpinBox").Value = characterController.HorizontalSensitivity * 10f;
            controlsPanel.GetNode<Slider>("Sensitivity/Slider").Value = characterController.HorizontalSensitivity * 10f;
         }
         else if (section == "audio")
         {
            managers.AudioManager.MusicVolume = (float)configFile.GetValue(section, "music");
            GetNode<Slider>("Audio/Music/Slider").Value = managers.AudioManager.MusicVolume;
         }
      }
   }

   void InitializeResolutions()
   {
      OptionButton resolutionDropDown = videoPanel.GetNode<OptionButton>("Resolution/ResolutionButton");

      foreach (string key in resolutions.Keys)
      {
         resolutionDropDown.AddItem(key);
      }
   }

   public void LoadSettingsMenu()
   {
      for (int i = 1; i < tabs.Length; i++)
      {
         tabs[i].Disabled = false;
         panels[i].Visible = false;
      }

      videoPanel.Visible = true;
      videoButton.Disabled = true;

      activeTab = 0;
   }

   void OnTabButtonDown(int id)
   {
      tabs[activeTab].Disabled = false;
      tabs[id].Disabled = true;

      panels[activeTab].Visible = false;
      panels[id].Visible = true;

      activeTab = id;
   }

   void OnSelectWindowedMode(int index)
   {
      GetWindow().Borderless = false;

      if (index == 0)
      {
         GetWindow().Mode = Window.ModeEnum.ExclusiveFullscreen;
      }
      else if (index == 1)
      {
         GetWindow().Mode = Window.ModeEnum.Fullscreen;
      }
      else
      {
         GetWindow().Mode = Window.ModeEnum.Windowed;
      }

      GetWindow().Size = currentResolution;

      configFile.SetValue("video", "windowed_mode", index);
   }

   void OnSelectResolution(int index)
   {
      string resolutionString = videoPanel.GetNode<OptionButton>("Resolution/ResolutionButton").GetItemText(index);
      currentResolution = resolutions[resolutionString];
      GetWindow().Size = currentResolution;

      configFile.SetValue("video", "resolution", resolutionString);
   }

   void OnSlideSensitivity(float value)
   {
      characterController.HorizontalSensitivity = value / 10f;
      controlsPanel.GetNode<SpinBox>("Sensitivity/SpinBox").Value = value;

      configFile.SetValue("controls", "sensitivity", characterController.HorizontalSensitivity);
   }

   void OnEnterSensitivity(float value)
   {
      characterController.HorizontalSensitivity = value / 10f;
      controlsPanel.GetNode<Slider>("Sensitivity/Slider").Value = value;

      configFile.SetValue("controls", "sensitivity", characterController.HorizontalSensitivity);
   }

   void OnSlideMusicVolume(float value)
   {
      managers.AudioManager.MusicVolume = value;
      //GetNode<Label>("Audio/Music/Number").Text = managers.AudioManager.MusicVolume;

      configFile.SetValue("audio", "music", managers.AudioManager.MusicVolume);
   }

   public override void _ExitTree()
   {
      configFile.Save("user://settings.cfg");
   }
}
