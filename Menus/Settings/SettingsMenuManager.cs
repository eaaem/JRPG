using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;

public partial class SettingsMenuManager : Panel
{
   private int activeTab = 0;

   [Export]
   private ManagerReferenceHolder managers;

   Panel[] panels = new Panel[3];
   [Export]
   Button[] tabs = new Button[3];

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

      videoButton.ButtonDown += () => OnTabButtonDown(0);
      controlsButton.ButtonDown += () => OnTabButtonDown(1);
      GetNode<Button>("ButtonsContainer/Audio").ButtonDown += () => OnTabButtonDown(2);

      InitializeResolutions();

      configFile = new ConfigFile();
      Error error = configFile.Load("user://settings.cfg");

      tabs[0] = videoButton;
      tabs[1] = controlsButton;
      tabs[2] = GetNode<Button>("ButtonsContainer/Audio");

      panels[0] = videoPanel;
      panels[1] = controlsPanel;
      panels[2] = GetNode<Panel>("Audio");

      // There is no config file, we need to make one
      if (error != Error.Ok)
      {
         GD.Print("Creating new");
         configFile.SetValue("video", "windowed_mode", 4);
         configFile.SetValue("video", "resolution", "1920x1080");

         configFile.SetValue("controls", "sensitivity", characterController.HorizontalSensitivity);

         configFile.SetValue("audio", "master", 0);
         configFile.SetValue("audio", "music", 0);
         configFile.SetValue("audio", "effects", 0);
         configFile.SetValue("audio", "ambience", 0);
         configFile.SetValue("audio", "ui", 0);

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
            managers.AudioManager.MasterVolume = (float)configFile.GetValue(section, "master");
            GetNode<Slider>("Audio/Master/Slider").Value = managers.AudioManager.MasterVolume;

            managers.AudioManager.MusicVolume = (float)configFile.GetValue(section, "music");
            GetNode<Slider>("Audio/Music/Slider").Value = managers.AudioManager.MusicVolume;

            managers.AudioManager.EffectsVolume = (float)configFile.GetValue(section, "effects");
            GetNode<Slider>("Audio/Effects/Slider").Value = managers.AudioManager.EffectsVolume;

            managers.AudioManager.AmbienceVolume = (float)configFile.GetValue(section, "ambience");
            GetNode<Slider>("Audio/Ambience/Slider").Value = managers.AudioManager.AmbienceVolume;

            managers.AudioManager.UIVolume = (float)configFile.GetValue(section, "ui");
            GetNode<Slider>("Audio/UI/Slider").Value = managers.AudioManager.UIVolume;
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

   void OnSlideMasterVolume(float value)
   {
      managers.AudioManager.MasterVolume = value;
      GetNode<Label>("Audio/Master/Number").Text = (value + 80).ToString() + "%";
      configFile.SetValue("audio", "master", value);
   }

   void OnSlideMusicVolume(float value)
   {
      managers.AudioManager.MusicVolume = value;
      GetNode<Label>("Audio/Music/Number").Text = (value + 80).ToString() + "%";
      configFile.SetValue("audio", "music", managers.AudioManager.MusicVolume);
   }

   void OnSlideEffectsVolume(float value)
   {
      managers.AudioManager.EffectsVolume = value;
      GetNode<Label>("Audio/Effects/Number").Text = (value + 80).ToString() + "%";
      configFile.SetValue("audio", "effects", managers.AudioManager.EffectsVolume);
   }

   void OnSlideAmbienceVolume(float value)
   {
      managers.AudioManager.AmbienceVolume = value;
      GetNode<Label>("Audio/Ambience/Number").Text = (value + 80).ToString() + "%";
      configFile.SetValue("audio", "ambience", managers.AudioManager.AmbienceVolume);
   }

   void OnSlideUIVolume(float value)
   {
      managers.AudioManager.UIVolume = value;
      GetNode<Label>("Audio/UI/Number").Text = (value + 80).ToString() + "%";
      configFile.SetValue("audio", "ui", managers.AudioManager.UIVolume);
   }

   public override void _ExitTree()
   {
      configFile.Save("user://settings.cfg");
   }
}
