using System;
using System.Timers;
using System.Collections.Generic;
using Eleon.Modding;
using System.Diagnostics;


namespace SolarHelperMod
{
	public class SolarHelperNewAPI : IMod
	{
		IModApi	mAPI;

		//list of loaded player base structures on the playfield
		//may or may not have solar batteries
		Dictionary<int, IStructure>	mStructs	=new Dictionary<int, IStructure>();

		Timer	mBatTimer;

		const int	BatteryCheckInterval	=15000;	//in MS


		public void Init(IModApi modAPI)
		{
			mAPI	=modAPI;

			mAPI.Log("SolarHelperNewAPI: Init called.");

			mAPI.GameEvent							+=OnGameEvent;
			mAPI.Application.OnPlayfieldLoaded		+=OnPlayFieldLoaded;
			mAPI.Application.OnPlayfieldUnloaded	+=OnPlayFieldUnLoaded;
			mAPI.Application.GameEntered			+=OnGameEntered;

			mBatTimer	=new FunTimer(BatteryCheckInterval, mStructs);

			mBatTimer.Elapsed	+=OnBatteryTimer;
			mBatTimer.AutoReset	=true;
			mBatTimer.Start();
		}


		public void Game_Update()
		{
		}


		public void Shutdown()
		{
			mAPI.Log("SolarHelperNewAPI: Shutdown called.");

			mAPI.GameEvent							-=OnGameEvent;
			mAPI.Application.OnPlayfieldLoaded		-=OnPlayFieldLoaded;
			mAPI.Application.OnPlayfieldUnloaded	-=OnPlayFieldUnLoaded;
			mAPI.Application.GameEntered			-=OnGameEntered;
		}


		void OnPlayFieldUnLoaded(string pfName)
		{
			mAPI.Log("Playfield: " + pfName + " unloaded...");

			if(mAPI.Playfield == null)
			{
				mAPI.Log("Playfield is null in OnPlayFieldUnLoaded()");
				return;
			}

			if(pfName != mAPI.Playfield.Name)
			{
				//not really interested in other playfields
				return;
			}

			//unwire entity loaded so no leakage
			mAPI.Playfield.OnEntityLoaded	-=OnEntityLoaded;
			mAPI.Playfield.OnEntityUnloaded	-=OnEntityUnLoaded;
		}


		void OnPlayFieldLoaded(string pfName)
		{
			mAPI.Log("Playfield: " + pfName + " loaded...");

			if(mAPI.Playfield == null)
			{
				mAPI.Log("Playfield is null in OnPlayFieldLoaded()");
				return;
			}

			if(pfName != mAPI.Playfield.Name)
			{
				//not really interested in other playfields
				return;
			}

			foreach(KeyValuePair<int, IEntity> ents in mAPI.Playfield.Entities)
			{
				mAPI.Log("Entity: " + ents.Key + ", Type: " + ents.Value.Type);
			}

			mAPI.Playfield.OnEntityLoaded	+=OnEntityLoaded;
			mAPI.Playfield.OnEntityUnloaded	+=OnEntityUnLoaded;
		}


		void OnGameEntered(bool bSomething)
		{
			mAPI.Log("OnGameEntered(): " + bSomething);
		}


		void OnGameEvent(GameEventType gvt, object stuff0, object stuff1, object stuff2, object stuff3, object stuff4)
		{
			mAPI.Log("Game Event: " + gvt);
		}


		void OnEntityUnLoaded(IEntity ent)
		{
			if(mStructs.ContainsKey(ent.Id))
			{
				mAPI.Log("Unloaded Entity: " + ent.Id + " being removed from structure list...");
				mStructs.Remove(ent.Id);
			}
		}


		void OnEntityLoaded(IEntity ent)
		{
			mAPI.Log("Entity: " + ent.Id + ", loaded...");
			mAPI.Log("FactionData: " + ent.Faction);
			mAPI.Log("ForwardVec: " + ent.Forward);
			mAPI.Log("Name: " + ent.Name);
			mAPI.Log("Structure: " + ent.Structure);

			if(ent.Structure != null)
			{
				mAPI.Log("Structure FactionData.Id: " + ent.Faction.Id);
				mAPI.Log("Structure FactionData.Group: " + ent.Faction.Group);

				if(ent.Faction.Group == FactionGroup.Player
					|| ent.Faction.Group == FactionGroup.Faction)
				{
					mAPI.Log("Player structure loaded...");

					if(ent.Type == EntityType.BA)
					{
						mAPI.Log("Keeping ref to player base...");

						if(!mStructs.ContainsKey(ent.Id))
						{
							mStructs.Add(ent.Id, ent.Structure);
						}
					}
				}
			}
		}


		void OnBatteryTimer(object sender, ElapsedEventArgs eea)
		{
			foreach(KeyValuePair<int, IStructure> str in mStructs)
			{
				if(!str.Value.IsReady)
				{
					continue;
				}
				if(!str.Value.IsPowered)
				{
					continue;
				}

				if(str.Value.FuelTank != null)
				{
					mAPI.Log("Fuel tank capacity: " + str.Value.FuelTank.Capacity +
						", Content: " + str.Value.FuelTank.Content);
				}

				IDevicePosList	idpl	=str.Value.GetDevices("AmmoCntr");
				for(int i=0;i < idpl.Count;i++)
				{
					VectorInt3	pos	=idpl.GetAt(i);

					IContainer	con	=str.Value.GetDevice<IContainer>(pos);

					mAPI.Log("Ammo container has " + con.VolumeCapacity + " volume.");
				}

				idpl	=str.Value.GetDevices("Container");
				for(int i=0;i < idpl.Count;i++)
				{
					VectorInt3	pos	=idpl.GetAt(i);

					IContainer	con	=str.Value.GetDevice<IContainer>(pos);

					mAPI.Log("Container has " + con.VolumeCapacity + " volume.");
				}

				idpl	=str.Value.GetDevices("Fridge");
				for(int i=0;i < idpl.Count;i++)
				{
					VectorInt3	pos	=idpl.GetAt(i);

					IContainer	con	=str.Value.GetDevice<IContainer>(pos);

					mAPI.Log("Fridge has " + con.VolumeCapacity + " volume.");
				}

				idpl	=str.Value.GetDevices("LCD");
				for(int i=0;i < idpl.Count;i++)
				{
					VectorInt3	pos	=idpl.GetAt(i);

					ILcd	lcd	=str.Value.GetDevice<ILcd>(pos);

					mAPI.Log("LCD says: " + lcd.GetText());
				}

				idpl	=str.Value.GetDevices("Light");
				for(int i=0;i < idpl.Count;i++)
				{
					VectorInt3	pos	=idpl.GetAt(i);

					ILight	lt	=str.Value.GetDevice<ILight>(pos);

					mAPI.Log("Light has range: " + lt.GetRange());
				}

				idpl	=str.Value.GetDevices("Portal");
				for(int i=0;i < idpl.Count;i++)
				{
					VectorInt3	pos	=idpl.GetAt(i);

					IPortal	door	=str.Value.GetDevice<IPortal>(pos);

					mAPI.Log("Door is a door");
				}
			}
		}
	}
}