using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;

namespace Saturn9;

public class LoadSaveManager
{
	public SaveGame m_SaveGame;

	public LoadSaveManager()
	{
		m_SaveGame = new SaveGame();
	}

	public void SaveGame()
	{
		try
		{
			if (!Guide.IsTrialMode && !Guide.IsVisible && g.m_App.saveDevice.IsReady)
			{
				g.m_App.saveDevice.Save("Saturn9", "A11E248A747F", delegate(Stream stream)
				{
					BinaryWriter binaryWriter = new BinaryWriter(stream);
					m_SaveGame.Save(binaryWriter);
					binaryWriter.Close();
					g.m_App.m_bSaveExists = true;
				});
			}
		}
		catch (Exception arg)
		{
			Console.WriteLine("{0} Exception caught.", arg);
		}
	}

	public void LoadGame()
	{
		try
		{
			if (Guide.IsTrialMode || Guide.IsVisible)
			{
				return;
			}
			if (g.m_App.saveDevice.FileExists("Saturn9", "A11E248A747F"))
			{
				g.m_App.saveDevice.Load("Saturn9", "A11E248A747F", delegate(Stream stream)
				{
					BinaryReader binaryReader = new BinaryReader(stream);
					m_SaveGame.Restore(binaryReader);
					binaryReader.Close();
					g.m_App.m_bSaveExists = true;
				});
			}
			else
			{
				g.m_App.m_bSaveExists = false;
			}
		}
		catch (Exception arg)
		{
			Console.WriteLine("{0} Exception caught.", arg);
		}
	}

	public void SaveOptions()
	{
		try
		{
			if (!Guide.IsTrialMode && !Guide.IsVisible && g.m_App.saveDevice.IsReady)
			{
				g.m_App.saveDevice.Save("Saturn9", "A11EFF0B553C", delegate(Stream stream)
				{
					BinaryWriter binaryWriter = new BinaryWriter(stream);
					binaryWriter.Write(g.m_App.m_OptionsHoriz);
					binaryWriter.Write(g.m_App.m_OptionsVert);
					binaryWriter.Write(g.m_App.m_OptionsInvertY);
					binaryWriter.Write(g.m_App.m_OptionsVibration);
					binaryWriter.Write(g.m_App.m_OptionsVol);
					binaryWriter.Write(g.m_App.m_OptionsBrightness);
					binaryWriter.Close();
				});
			}
		}
		catch (Exception arg)
		{
			Console.WriteLine("{0} Exception caught.", arg);
		}
	}

	public void LoadOptions()
	{
		try
		{
			if (!Guide.IsTrialMode && !Guide.IsVisible && g.m_App.saveDevice.FileExists("Saturn9", "A11EFF0B553C"))
			{
				g.m_App.saveDevice.Load("Saturn9", "A11EFF0B553C", delegate(Stream stream)
				{
					BinaryReader binaryReader = new BinaryReader(stream);
					g.m_App.m_OptionsHoriz = binaryReader.ReadSingle();
					g.m_App.m_OptionsVert = binaryReader.ReadSingle();
					g.m_App.m_OptionsInvertY = binaryReader.ReadBoolean();
					g.m_App.m_OptionsVibration = binaryReader.ReadBoolean();
					g.m_App.m_OptionsVol = binaryReader.ReadSingle();
					g.m_App.m_OptionsBrightness = binaryReader.ReadInt32();
					binaryReader.Close();
					SoundEffect.MasterVolume = g.m_App.m_OptionsVol;
				});
			}
		}
		catch (Exception arg)
		{
			Console.WriteLine("{0} Exception caught.", arg);
		}
	}
}
